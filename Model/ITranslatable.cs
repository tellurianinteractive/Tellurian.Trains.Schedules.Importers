namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Marks a model type as having a localised display name. The default name is keyed by the type name
/// (<c>GetType().Name</c>) through the interface's default implementation; a type overrides
/// <see cref="TranslationKey"/> only in special cases — for example a <c>ScheduledObject</c> keyed by
/// its <c>ScheduledObjectType</c>, so a locomotive and a wagonset get distinct labels.
/// </summary>
/// <remarks>
/// The key is an instance member (not a static type→key map) so an override can depend on instance
/// state. Resolve a key to text with <c>ClassNames.Localized</c>; resolve an object directly with
/// <c>ClassNames.LocalizedFor</c>, which reads this key (and falls back to the type name for objects
/// that do not implement the interface).
/// </remarks>
public interface ITranslatable
{
    /// <summary>The resource key for this object's localised display name. Defaults to the type name.</summary>
    string TranslationKey => GetType().Name;
}
