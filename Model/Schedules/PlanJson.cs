using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// The JSON settings for reading and writing a whole <see cref="Plan"/>, shared by every persistence
/// path — browser storage, file import and file export — so a plan written by one is always readable
/// by the others.
/// </summary>
/// <remarks>
/// The object graph contains reference cycles, so <see cref="ReferenceHandler.Preserve"/> and a deep
/// <see cref="JsonSerializerOptions.MaxDepth"/> are both required to round-trip it. Reading also
/// accepts property names used by earlier versions, see <see cref="AcceptLegacyNames"/>, and the
/// shapes that only ever repeated what is stored elsewhere, see <see cref="WriteCallsOnlyWithTheirTrain"/>.
/// </remarks>
public static class PlanJson
{
    /// <summary>
    /// Creates the options for reading and writing a <see cref="Plan"/>.
    /// </summary>
    /// <param name="writeIndented">Whether to write indented JSON, for a plan saved to a file a person may read.</param>
    public static JsonSerializerOptions CreateOptions(bool writeIndented = false) => new()
    {
        WriteIndented = writeIndented,
        ReferenceHandler = ReferenceHandler.Preserve,
        MaxDepth = 256,
        Converters = { new CountryByIdConverter() },
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers =
            {
                AcceptLegacyNames,
                WriteCallsOnlyWithTheirTrain,
                WriteCatalogueEntriesOnlyInTheirCatalogue,
                WriteRegionIdsOnlyWhereAStationHasSome,
            },
        },
    };

    /// <summary>
    /// Writes a train category, a company and a region only where each belongs — in the catalogue that
    /// owns it — and not again on everything that refers to it. Each keeps just the id it is picked out
    /// by, which is what a plan is read with:
    /// <see cref="Timetables.TimetableExtensions.ResolveCatalogueReferences"/>,
    /// <see cref="Layouts.LayoutRegionExtensions"/> and <see cref="Plan"/> put the objects back
    /// afterwards.
    /// </summary>
    /// <remarks>
    /// As with <see cref="WriteCallsOnlyWithTheirTrain"/>, only writing is turned off, never reading. A
    /// plan written by an earlier version stored the category, company or region on the first train,
    /// vehicle, duty or station that used it and pointed everything else — including the catalogue — at
    /// that copy with a <c>$ref</c>, so refusing to read it would leave every one of those references
    /// dangling.
    /// <para>
    /// The catalogues are written before anything that refers to them: <see cref="Layouts.Layout"/>
    /// declares its companies and regions ahead of its locations, and <see cref="Timetables.Timetable"/>
    /// declares its categories ahead of its trains. That is what makes the catalogue the place the entry
    /// is stored, rather than wherever the writer first happened to meet it.
    /// </para>
    /// </remarks>
    private static void WriteCatalogueEntriesOnlyInTheirCatalogue(JsonTypeInfo typeInfo)
    {
        foreach (var property in CatalogueReferences.TryGetValue(typeInfo.Type, out var names)
            ? typeInfo.Properties.Where(p => names.Contains(p.Name, StringComparer.Ordinal))
            : [])
        {
            property.ShouldSerialize = static (_, _) => false;
        }
    }

    // The references that are written as an id alone, by the type holding them. Each names a property
    // whose value lives in Layout.Companies, Layout.Regions or Timetable.TrainCategories.
    private static readonly Dictionary<Type, string[]> CatalogueReferences = new()
    {
        [typeof(Timetables.Train)] = [nameof(Timetables.Train.Category), nameof(Timetables.Train.Company)],
        [typeof(Timetables.TrainCategory)] = [nameof(Timetables.TrainCategory.Company)],
        [typeof(ScheduledObject)] = [nameof(ScheduledObject.Company)],
        [typeof(Duties.DriverDuty)] = [nameof(Duties.DriverDuty.Company)],
        [typeof(Layouts.Station)] = [nameof(Layouts.Station.Regions)],
    };

    /// <summary>
    /// Leaves a station's <see cref="Layouts.Station.RegionIds"/> out where it has no regions, which is
    /// every station but the few shadow yards that stand for somewhere off the layout. An empty list
    /// says exactly what its absence does, on every location of the plan.
    /// </summary>
    private static void WriteRegionIdsOnlyWhereAStationHasSome(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type != typeof(Layouts.Station)) return;
        if (typeInfo.Properties.FirstOrDefault(p => p.Name == nameof(Layouts.Station.RegionIds)) is not { } regionIds) return;
        regionIds.ShouldSerialize = static (_, value) => value is IList<int> { Count: > 0 };
    }

    /// <summary>
    /// Writes a station call only where it belongs — in its train — and not a second time under the
    /// track it stands on. <see cref="Layouts.StationTrack.Calls"/> is an index into
    /// <see cref="Timetables.Train.Calls"/>, rebuilt from it whenever a timetable is read
    /// (<see cref="Timetables.TimetableExtensions.RebuildStationCalls"/>), so writing it stored nothing
    /// that was not stored already.
    /// </summary>
    /// <remarks>
    /// Only writing is turned off (<see cref="JsonPropertyInfo.ShouldSerialize"/>), never reading: a
    /// plan written by an earlier version put the calls under the track first — the layout is written
    /// before the trains — and half the plan hangs below them there, referred to by <c>$ref</c>
    /// everywhere else. Skipping that on read would leave every one of those references dangling.
    /// Reading it costs nothing, since the index is rebuilt afterwards either way.
    /// <para>
    /// Not writing it also flattens the document. Descending from a track into its calls, on into their
    /// trains and back again through each call's track was what drove the nesting deep enough to need
    /// <see cref="JsonSerializerOptions.MaxDepth"/> raised far above the default.
    /// </para>
    /// </remarks>
    private static void WriteCallsOnlyWithTheirTrain(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type != typeof(Layouts.StationTrack)) return;
        if (typeInfo.Properties.FirstOrDefault(p => p.Name == nameof(Layouts.StationTrack.Calls)) is not { } calls) return;
        calls.ShouldSerialize = static (_, _) => false;
    }

    /// <summary>
    /// Lets a plan written by an earlier version still be read, by accepting the property names that
    /// version wrote. Each legacy name is read-only — it has no getter, so it is never written back,
    /// and a plan saved after being read once carries only the current names.
    /// </summary>
    /// <remarks>
    /// <see cref="Origin"/> and <see cref="Destination"/> named their location <c>Station</c> while only
    /// a station could be one. Both now take any cargo-exchanging <see cref="OperationLocation"/> and
    /// call it <c>Location</c>. The current name is also relaxed from required to optional, because a
    /// plan holding the legacy name has no <c>Location</c> property at all and would otherwise be
    /// rejected before this alias could fill it in.
    /// </remarks>
    private static void AcceptLegacyNames(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type != typeof(Origin) && typeInfo.Type != typeof(Destination)) return;
        if (typeInfo.Properties.FirstOrDefault(p => p.Name == nameof(Destination.Location)) is not { Set: { } setLocation } location) return;

        location.IsRequired = false;
        var legacy = typeInfo.CreateJsonPropertyInfo(typeof(OperationLocation), "Station");
        legacy.Set = setLocation;
        typeInfo.Properties.Add(legacy);
    }
}
