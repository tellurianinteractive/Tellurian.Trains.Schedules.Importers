namespace Tellurian.Trains.Schedules.Planning.Components;

/// <summary>
/// This should be reused whene ever itens are displayed in a HTML select-options 
/// </summary>
/// <param name="Value">If integer value, it can be retrieved by the <see cref="Id"/>. An empty <see cref="Value"/> denotes a null value.</param>
/// <param name="LocalizedDescription"></param>
public record ListboxItem(string Value, string LocalizedDescription)
{
    public int? Id => int.TryParse(Value, out var id) ? id : null;

    // TODO: Add a ToHtml that render this as an <option/>  item. This can be in an extension in Planning.Components
};
