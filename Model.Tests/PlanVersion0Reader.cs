using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Tellurian.Trains.Schedules.Model.Settings;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Reads a plan of schema version 0 through the model, the way the app did before version 1: with
/// <see cref="ReferenceHandler.Preserve"/>. It is the yardstick for the migrations: a plan migrated and read must hold
/// the same as the plan read the old way. The names that the shared model changed are read under their version 0
/// names: a location's <c>$type</c>, a track's <c>Number</c> and a location's <c>CountryId</c>. It is removed when the
/// model no longer has what version 0 holds.
/// </summary>
internal static class PlanVersion0Reader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        MaxDepth = 256,
        AllowOutOfOrderMetadataProperties = true,
        Converters = { new CountryById() },
        TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { Version0Names } },
    };

    /// <summary>The plan in <paramref name="json"/>, read as version 0 was, platforms given on reading included.</summary>
    public static Plan Read(string json)
    {
        var plan = JsonSerializer.Deserialize<Plan>(json, Options) ?? throw new InvalidOperationException("No plan.");
        plan.Layout.EnsurePlatforms();
        return plan;
    }

    private static readonly string[] Version7TimetableData =
    [
        nameof(Timetable.Settings), nameof(Timetable.Companies), nameof(Timetable.TimetableStretches), nameof(Timetable.DispatchStretches),
        nameof(Timetable.TopologyPositions), nameof(Timetable.LocationPlanning), nameof(Timetable.StationTrackRoutes),
    ];

    /// <summary>
    /// Makes a type's metadata read and write what version 0 had: the file-only id properties of later versions, which are
    /// internal, are taken out, so that their names don't clash with what version 0 wrote; and every public property that
    /// the model now marks <c>[JsonIgnore]</c> and that can be set is put back, since version 0 wrote them all: parents,
    /// and the objects that later versions write by their id. For the readers and writers of version 0 in the tests.
    /// </summary>
    internal static void Version0References(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object) return;
        // Version 0 refers by $ref, which can't be given to a constructor's parameters: a type that later versions read
        // through a constructor taking an id is created with its public parameterless one instead.
        if (typeInfo.Type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).Any(c => c.GetParameters().Length > 0 && c.GetCustomAttribute<JsonConstructorAttribute>() is not null) &&
            typeInfo.Type.GetConstructor(Type.EmptyTypes) is { } parameterless)
            typeInfo.CreateObject = () => parameterless.Invoke(null);
        for (var i = typeInfo.Properties.Count - 1; i >= 0; i--)
            if (typeInfo.Properties[i].AttributeProvider is PropertyInfo { GetMethod.IsPublic: false }) typeInfo.Properties.RemoveAt(i);
        // What the timetable keeps since version 7 was the layout's in version 0, so a version 0 timetable had none of it.
        if (typeInfo.Type == typeof(Timetable))
            for (var i = typeInfo.Properties.Count - 1; i >= 0; i--)
                if (Version7TimetableData.Contains(typeInfo.Properties[i].Name)) typeInfo.Properties.RemoveAt(i);
        foreach (var member in typeInfo.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (member.GetIndexParameters().Length > 0 || member.SetMethod is not { IsPublic: true } || member.GetMethod is not { IsPublic: true }) continue;
            if (member.GetCustomAttribute<JsonIgnoreAttribute>(inherit: true) is not { Condition: JsonIgnoreCondition.Always }) continue;
            // The timetable a company or a timetable stretch belongs to came after version 0, which had no such reference.
            if (member.Name == nameof(Company.Timetable) && (member.DeclaringType == typeof(Company) || member.DeclaringType == typeof(TimetableStretch))) continue;
            // A property the model marks [JsonIgnore] is still listed, ignored; it is replaced.
            if (typeInfo.Properties.FirstOrDefault(p => p.Name == member.Name) is { } ignored) typeInfo.Properties.Remove(ignored);
            var property = typeInfo.CreateJsonPropertyInfo(member.PropertyType, member.Name);
            property.Get = member.GetValue;
            property.Set = member.SetValue;
            typeInfo.Properties.Add(property);
        }
    }

    private static void Version0Names(JsonTypeInfo typeInfo)
    {
        Version0References(typeInfo);
        PlannersData(typeInfo);
        if (typeInfo.Type == typeof(OperationLocation) && typeInfo.PolymorphismOptions is { } polymorphism)
            polymorphism.TypeDiscriminatorPropertyName = "$type";
        if (typeInfo.Type == typeof(StationTrack) && typeInfo.Properties.FirstOrDefault(p => p.Name == nameof(StationTrack.Designation)) is { } designation)
            designation.Name = "Number";
        if (typeof(OperationLocation).IsAssignableFrom(typeInfo.Type) && typeInfo.Kind == JsonTypeInfoKind.Object)
        {
            var countryId = typeInfo.CreateJsonPropertyInfo(typeof(int?), "CountryId");
            countryId.Set = (location, id) => ((OperationLocation)location).CountryId = (int?)id;
            typeInfo.Properties.Add(countryId);
        }
    }

    // What version 0 kept on the layout, its locations and tracks that is now the timetable's, as it is read.
    private sealed class LayoutData
    {
        public PlanSettings? Settings;
        public ICollection<Company>? Companies;
        public ICollection<TimetableStretch>? TimetableStretches;
        public ICollection<DispatchStretch>? DispatchStretches;
        public ICollection<TopologyPosition>? TopologyPositions;
    }

    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Layout, LayoutData> LayoutsData = [];
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<OperationLocation, LocationPlanning> LocationsData = [];
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<StationTrack, StationTrackRoute> TracksData = [];

    private static void Read<TOwner, TValue, TData>(JsonTypeInfo typeInfo, string name, System.Runtime.CompilerServices.ConditionalWeakTable<TOwner, TData> table, Action<TData, TValue?> set)
        where TOwner : class where TData : class, new()
    {
        if (typeInfo.Properties.Any(p => p.Name == name)) return;
        var property = typeInfo.CreateJsonPropertyInfo(typeof(TValue), name);
        property.Set = (owner, value) => set(table.GetOrCreateValue((TOwner)owner), (TValue?)value);
        typeInfo.Properties.Add(property);
    }

    private static void PlannersData(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object) return;
        if (typeInfo.Type == typeof(Timetable))
        {
            // Before the timetable resolves its references, so that they find the companies in its catalogue.
            var resolve = typeInfo.OnDeserialized;
            typeInfo.OnDeserialized = timetable =>
            {
                TakePlannersData((Timetable)timetable);
                resolve?.Invoke(timetable);
            };
        }
        if (typeInfo.Type == typeof(Layout))
        {
            Read<Layout, PlanSettings, LayoutData>(typeInfo, "Settings", LayoutsData, (d, v) => d.Settings = v);
            Read<Layout, ICollection<Company>, LayoutData>(typeInfo, "Companies", LayoutsData, (d, v) => d.Companies = v);
            Read<Layout, ICollection<TimetableStretch>, LayoutData>(typeInfo, "TimetableStretches", LayoutsData, (d, v) => d.TimetableStretches = v);
            Read<Layout, ICollection<DispatchStretch>, LayoutData>(typeInfo, "DispatchStretches", LayoutsData, (d, v) => d.DispatchStretches = v);
            Read<Layout, ICollection<TopologyPosition>, LayoutData>(typeInfo, "TopologyPositions", LayoutsData, (d, v) => d.TopologyPositions = v);
        }
        if (typeof(OperationLocation).IsAssignableFrom(typeInfo.Type))
        {
            Read<OperationLocation, StationTimings, LocationPlanning>(typeInfo, "Timings", LocationsData, (d, v) => d.Timings = v ?? new());
            Read<OperationLocation, int?, LocationPlanning>(typeInfo, "PhoneNumber", LocationsData, (d, v) => d.PhoneNumber = v);
            Read<OperationLocation, string, LocationPlanning>(typeInfo, "Instructions", LocationsData, (d, v) => d.Instructions = v ?? string.Empty);
            Read<OperationLocation, bool, LocationPlanning>(typeInfo, "HideMeets", LocationsData, (d, v) => d.HideMeets = v);
            Read<OperationLocation, bool, LocationPlanning>(typeInfo, "HidePassings", LocationsData, (d, v) => d.HidePassings = v);
        }
        if (typeInfo.Type == typeof(Station))
        {
            Read<OperationLocation, IList<StaffPost>, LocationPlanning>(typeInfo, "StaffPosts", LocationsData, (d, v) => d.StaffPosts = v ?? []);
            Read<OperationLocation, int, LocationPlanning>(typeInfo, "ShiftCount", LocationsData, (d, v) => d.ShiftCount = v);
            Read<OperationLocation, StartLabelPrinting, LocationPlanning>(typeInfo, "StartLabels", LocationsData, (d, v) => d.StartLabels = v);
            Read<OperationLocation, bool, LocationPlanning>(typeInfo, "SplitDispatchByDirection", LocationsData, (d, v) => d.SplitDispatchByDirection = v);
            Read<OperationLocation, bool, LocationPlanning>(typeInfo, "IsPassengerInterchange", LocationsData, (d, v) => d.IsPassengerInterchange = v);
        }
        if (typeInfo.Type == typeof(StationTrack))
        {
            Read<StationTrack, int?, StationTrackRoute>(typeInfo, "PreviousLocationId", TracksData, (d, v) => d.PreviousLocationId = v);
            Read<StationTrack, int?, StationTrackRoute>(typeInfo, "NextLocationId", TracksData, (d, v) => d.NextLocationId = v);
            Read<StationTrack, bool, StationTrackRoute>(typeInfo, "AppliesInBothDirections", TracksData, (d, v) => d.AppliesInBothDirections = v);
        }
    }

    // Moves what was read onto the layout, its locations and tracks into the timetable, where it is kept now.
    private static void TakePlannersData(Timetable timetable)
    {
        if (LayoutsData.TryGetValue(timetable.Layout, out var data))
        {
            if (data.Settings is { } settings) timetable.Settings = settings;
            foreach (var company in data.Companies ?? []) timetable.Add(company);
            foreach (var stretch in data.TimetableStretches ?? []) timetable.Add(stretch);
            foreach (var stretch in data.DispatchStretches ?? []) timetable.DispatchStretches.Add(stretch);
            foreach (var position in data.TopologyPositions ?? []) timetable.TopologyPositions.Add(position);
        }
        foreach (var location in timetable.Layout.OperationLocations)
        {
            if (LocationsData.TryGetValue(location, out var planning))
            {
                planning.LocationId = location.Id;
                timetable.LocationPlanning.Add(planning);
            }
            foreach (var track in location.Tracks)
            {
                if (!TracksData.TryGetValue(track, out var route) || !route.IsGiven) continue;
                (route.TrackId, route.LocationId) = (track.Id, location.Id);
                timetable.StationTrackRoutes.Add(route);
            }
        }
    }

    private sealed class CountryById : JsonConverter<Country>
    {
        public override Country? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType == JsonTokenType.Number ? Country.ById(reader.GetInt32()) : throw new JsonException("A country is written as its id.");

        public override void Write(Utf8JsonWriter writer, Country value, JsonSerializerOptions options) => writer.WriteNumberValue(value.Id);
    }
}
