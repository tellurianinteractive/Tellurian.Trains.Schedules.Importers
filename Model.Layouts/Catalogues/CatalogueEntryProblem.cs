namespace Tellurian.Trains.Schedules.Model.Layouts.Catalogues;

/// <summary>The kinds of user catalogue entries that are checked as a whole.</summary>
public enum CatalogueEntryKind
{
    /// <summary>A module standard.</summary>
    Standard,
    /// <summary>An add-on to a module standard.</summary>
    AddOn,
    /// <summary>A compatibility override for a pair of end profiles.</summary>
    Override,
}

/// <summary>
/// A problem with an entry in the user's catalogue. <paramref name="Id"/> is the standard's or add-on's id,
/// or an override's first end profile id; <paramref name="SecondId"/> is an override's second end profile id.
/// </summary>
public sealed record CatalogueEntryProblem(CatalogueEntryKind Kind, Guid Id, Guid? SecondId, CatalogueProblem Problem);
