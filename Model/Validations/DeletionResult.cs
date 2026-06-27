using Tellurian.Trains.Schedules.Model.Resources;

namespace Tellurian.Trains.Schedules.Model.Validations;

/// <summary>
/// The outcome of a request to delete a model object, as a closed hierarchy of two cases:
/// <see cref="Success"/> (the object may be — or has been — deleted) and <see cref="Failure"/>
/// (the object is still referenced, and <see cref="Failure.References"/> lists by what).
/// </summary>
/// <remarks>
/// The same type serves both the pure query (<c>MayDelete</c>) and the command (<c>TryDelete</c>):
/// a <see cref="Success"/> from <c>MayDelete</c> means "deletion is allowed", a <see cref="Success"/>
/// from <c>TryDelete</c> means "the object was removed". The caller always knows which it invoked, so
/// the two readings are unambiguous in context. The hierarchy is closed (the constructor is private,
/// so only the nested <see cref="Success"/> and <see cref="Failure"/> can derive), which lets callers
/// pattern-match exhaustively.
/// </remarks>
public abstract record DeletionResult
{
    private DeletionResult(object subject) => Subject = subject;

    /// <summary>The object the deletion request concerns.</summary>
    public object Subject { get; }

    /// <summary>
    /// The localised class name of the <see cref="Subject"/> in the current UI language, for building
    /// messages such as "Cannot delete <c>Company</c> 'DB Cargo'". Resolved at access time from the
    /// model's <c>Classes</c> resources, falling back to the raw type name.
    /// </summary>
    public string SubjectClassName => ClassNames.LocalizedFor(Subject);

    /// <summary>Convenience for view binding: <c>true</c> for <see cref="Success"/>.</summary>
    public bool IsSuccess => this is Success;

    /// <summary>The object may be deleted (from a query) or was deleted (from a command).</summary>
    /// <param name="Subject">The object the request concerned.</param>
    public sealed record Success(object Subject) : DeletionResult(Subject);

    /// <summary>The object is still referenced and cannot be deleted.</summary>
    /// <param name="Subject">The object the request concerned.</param>
    /// <param name="References">The places that still reference the <paramref name="Subject"/>; never empty.</param>
    public sealed record Failure(object Subject, IReadOnlyList<Reference> References) : DeletionResult(Subject);
}

/// <summary>
/// A single place that references an object being considered for deletion: the referencing object's
/// class (as a resource key) together with a human-readable name for the referencing instance.
/// </summary>
/// <param name="ClassNameKey">The referencing type's name, used as a key into the model's <c>Classes</c>
/// resources (for example <c>Train</c>, <c>Company</c>).</param>
/// <param name="Name">A display name for the referencing instance (its <c>ToString()</c>).</param>
public readonly record struct Reference(string ClassNameKey, string Name)
{
    /// <summary>
    /// Creates a <see cref="Reference"/> to a referencing object, taking its display key from
    /// <see cref="ClassNames.KeyOf"/> (honouring an <see cref="ITranslatable"/> override) and its
    /// name from <c>ToString()</c>.
    /// </summary>
    /// <param name="referencingObject">The object that holds the reference.</param>
    public static Reference For(object referencingObject) =>
        new(ClassNames.KeyOf(referencingObject), referencingObject.ToString() ?? string.Empty);

    /// <summary>The <see cref="ClassNameKey"/> localised to the current UI language.</summary>
    public string ClassName => ClassNames.Localized(ClassNameKey);

    /// <inheritdoc/>
    public override string ToString() => $"{ClassName}: {Name}";
}
