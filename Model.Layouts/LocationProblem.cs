namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>A broken rule, the property it concerns and, for a problem of a track, the track's designation.</summary>
/// <param name="Code">The rule.</param>
/// <param name="Property">The name of the property of <see cref="OperationLocation"/> or <see cref="StationTrack"/>.</param>
/// <param name="TrackDesignation">The designation of the track concerned; null for a problem of the location.</param>
public sealed record LocationProblem(LocationProblemCode Code, string Property, string? TrackDesignation = null);
