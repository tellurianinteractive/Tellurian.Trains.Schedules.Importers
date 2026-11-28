namespace Tellurian.Trains.Schedules.Model.Drawings;

/// <summary>One piece of a <see cref="TrackRoute"/>: the part of one route of one track element that the route runs along.</summary>
/// <param name="ModuleId">The <see cref="PlacedModule.Id"/> of the module the element is in.</param>
/// <param name="TrackElementId">The id of the track element in the module's drawing.</param>
/// <param name="RouteNumber">Which of the element's routes, from 1: a turnout's straight and diverging routes are 1 and 2.</param>
/// <param name="StationTrackId">The <see cref="StationTrack.Id"/> the route is part of, if any.</param>
/// <param name="Length">The length run along the route, mm.</param>
/// <param name="Radius">The smallest radius of the route, mm; null when it is straight.</param>
public sealed record TrackRoutePiece(int ModuleId, int TrackElementId, int RouteNumber, int? StationTrackId, double Length, double? Radius);
