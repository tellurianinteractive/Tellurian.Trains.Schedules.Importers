namespace Tellurian.Trains.Schedules.Model.Layouts.Catalogues;

/// <summary>
/// A reference to a catalogue entry (standard, add-on, end profile) by id and by name, so that a
/// receiver without this app's catalogue, such as the Module Registry, can map it to its own data (spec §6).
/// </summary>
public sealed record CatalogueReference(Guid Id, string Name) : ExtensibleRecord
{
    /// <inheritdoc/>
    public override string ToString() => Name;
}
