using System.Text.Json;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// How a <see cref="Layout"/> is written as JSON, in the layout block of a hall plan and inside a plan file, as members of
/// <see cref="Layout"/>: <c>Layout.JsonSerializerOptions</c> and <c>layout.RestoreReferences()</c>. A layout is a tree:
/// what a thing owns is nested in it, everything else is referred to by its id, and no parent is written. Each location
/// is written once, in <see cref="Layout.OperationLocations"/>; a stretch refers to its ends by <c>startId</c> and
/// <c>endId</c>, a location to the station that controls it by <c>controlledById</c>, and a layout module to its location
/// by <c>operationLocationId</c>. After reading, <c>RestoreReferences</c> puts the objects back.
/// </summary>
/// <remarks>
/// The shape is said by attributes on the types, read by source-generated metadata (<see cref="LayoutJsonContext"/>):
/// an object referred to by id is <c>[JsonIgnore]</c>, and beside it an id property gives the object's id, or holds the id
/// read until the object is found. No reflection is used, so it works trimmed and compiled ahead of time. The properties
/// that shape the file are public, some hidden from IntelliSense, so that another app's own source-generated serialiser
/// writes the contract's types the same way.
/// </remarks>
public static class LayoutSerialization
{
    extension(Layout)
    {
        /// <summary>
        /// The options a layout is read and written with on its own, as in the layout block of a hall plan: camelCase
        /// names, enum values by their name in the code, default values not written, and the kind of a location read
        /// wherever it stands. Source-generated; read-only. For indented JSON, copy them with <c>WriteIndented</c> set.
        /// </summary>
        public static JsonSerializerOptions JsonSerializerOptions => LayoutJsonContext.Default.Options;
    }

    extension(Layout layout)
    {
        /// <summary>
        /// Resolves the ids of a layout that has been read into the objects they stand for, and sets the parents of what
        /// it owns: a location's layout, a track's location, a stretch's ends, the station that controls a location or
        /// serves its cargo, where a lock key is held, the stretches of a timetable or dispatch stretch, and the location
        /// of a layout module, whose tracks are then the location's own list.
        /// </summary>
        public void RestoreReferences() => Restore(layout);
    }

    /// <summary>The source-generated metadata of the layout's types, for the plan file to combine with its own.</summary>
    internal static LayoutJsonContext Context => LayoutJsonContext.Default;

    private static void Restore(Layout layout)
    {
        ArgumentNullException.ThrowIfNull(layout);
        var locations = layout.OperationLocations.GroupBy(l => l.Id).ToDictionary(g => g.Key, g => g.First());
        var stretches = layout.TrackStretches.GroupBy(s => s.Id).ToDictionary(g => g.Key, g => g.First());
        foreach (var company in layout.Companies)
        {
            company.Layout = layout;
            company.LayoutId = layout.Id;
        }
        foreach (var location in layout.OperationLocations)
        {
            location.Layout = layout;
            location.LayoutId = layout.Id;
            foreach (var track in location.Tracks)
            {
                track.Station = location;
                track.StationId = location.Id;
            }
            if (location.CargoServedFrom is null && location.CargoServedFromId is int servedFrom && locations.GetValueOrDefault(servedFrom) is Station server) location.CargoServedFrom = server;
            if (location.ControlledBy is null && location.ControlledById is int controlledBy && locations.GetValueOrDefault(controlledBy) is Station controller) location.ControlledBy = controller;
            if (location.LockKey is { HeldAt: null, HeldAtId: int heldAt } key && locations.GetValueOrDefault(heldAt) is Station holder) key.HeldAt = holder;
        }
        foreach (var stretch in layout.TrackStretches)
        {
            stretch.Layout = layout;
            stretch.LayoutId = layout.Id;
            stretch.Start ??= locations.GetValueOrDefault(stretch.StartId)!;
            stretch.End ??= locations.GetValueOrDefault(stretch.EndId)!;
        }
        foreach (var stretch in layout.TimetableStretches)
            if (stretch.Stretches.Count == 0 && stretch.StretchIds is { Count: > 0 } ids) stretch.Stretches = [.. ids.Select(stretches.GetValueOrDefault).OfType<TrackStretch>()];
        foreach (var stretch in layout.DispatchStretches)
        {
            if (stretch.From is null && stretch.FromId is int from && locations.GetValueOrDefault(from) is { } fromLocation) stretch.From = fromLocation;
            if (stretch.To is null && stretch.ToId is int to && locations.GetValueOrDefault(to) is { } toLocation) stretch.To = toLocation;
            if (stretch.Stretches.Count == 0 && stretch.StretchIds is { Count: > 0 } ids) stretch.Stretches = [.. ids.Select(stretches.GetValueOrDefault).OfType<TrackStretch>()];
        }
    }
}
