namespace Tellurian.Trains.Schedules.Model.Validations;

/// <summary>
/// Holds the result of a request to delete an object.
/// </summary>
/// <param name="Value"></param>
public record DeleteRequest(object Value)
{
    private IList<(string LocalizedClassName, string ObjectName)> ReferencedAt { get; } = [];

    /// <summary>
    /// Tells if the <see cref="Value"/> can bee deleted.
    /// </summary>
    public bool MayDelete => ReferencedAt.Count == 0;

    /// <summary>
    /// The class name of the object subject to deletion. This is the value to use as a resource key to lookup transplations. 
    /// </summary>
    public string ClassName => Value.GetType().Name;

    /// <inheritdoc/>
    // TODO: Replace hardcoded strings with Strings.Deletable and Strings.ReferencedAt
    public override string ToString() => MayDelete ? "Deletable" : $"ReferencedAt {string.Join(", ", ReferencedAt)}";

    /// <summary>
    /// Adds a value that described what object holding a reference using class name and ToString() value, example Company: DB Cargo.
    /// </summary>
    /// <param name="locaizedClassName">This is the class name translated to the selected GUI language.</param>
    /// <param name="objectName">This is the objects value from ToSt</param>
    public void AddReference(string locaizedClassName, string objectName) => ReferencedAt.Add((locaizedClassName, objectName));
}

/// <summary>
/// This class contains extension methods for determining if an object may be deleted or not.
/// </summary>
public static class DeletionRules
{
    extension(Plan plan)
    {
        /// <summary>
        /// Determines if the onbject is referenced.
        /// </summary>
        /// <returns>If referenced at at least one place returns false; otherwise true.</returns>
        public DeleteRequest MayDelete(Country country)
        {
            //TODO: Add checks for all places where its id is set => it is referenced.
            return new(country);
        }
        /// <summary>
        /// Determines if the onbject is referenced.
        /// </summary>
        /// <returns>If referenced at at least one place returns false; otherwise true.</returns>

        public DeleteRequest MayDelete(Company company)
        {
            //TODO: Add checks for all places where its id is set => it is referenced.
            return new(company);
        }
    }
}
