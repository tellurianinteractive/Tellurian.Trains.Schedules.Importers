namespace Tellurian.Trains.Schedules.Model.Layouts.Catalogues;

/// <summary>A problem with a catalogue entry, and the property it concerns.</summary>
public sealed record CatalogueProblem(CatalogueProblemCode Code, string Property);
