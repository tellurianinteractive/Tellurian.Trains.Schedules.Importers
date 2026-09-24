using System.Globalization;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// A person taking part in the meeting a <see cref="Plan"/> is for, known here by name alone. Stored once,
/// in the plan's participant catalogue (<see cref="Plan.Participants"/>), and referred to everywhere else by
/// <see cref="Id"/>, so a name is spelt one way throughout the plan and correcting it corrects it everywhere.
/// </summary>
/// <remarks>
/// The catalogue is the plan's own list of names, not the register of members a meeting is organised
/// from: that is kept by the Module Registry, and nothing here depends on it.
/// </remarks>
public sealed class Participant
{
    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private Participant()
    {
        Name = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Participant"/>.
    /// </summary>
    /// <param name="id">The identifier, unique within the plan's participant catalogue.</param>
    /// <param name="name">The participant's name; surrounding and repeated white space is removed.</param>
    public Participant(int id, string name)
    {
        Id = id;
        Name = name.NormalizedParticipantName;
    }

    /// <summary>
    /// Gets or sets the identifier, unique within the plan's participant catalogue.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the participant's name, as it is to be shown and printed.
    /// </summary>
    public string Name { get; set; }

    /// <inheritdoc/>
    public override string ToString() => Name;
}

/// <summary>
/// Provides extension members for <see cref="Participant"/> and the names participants are known by.
/// </summary>
public static class ParticipantExtensions
{
    extension(string? name)
    {
        /// <summary>
        /// The name as it is stored: without surrounding white space, and with every run of white space
        /// inside it made a single space, so "Anna  Berg " and "Anna Berg" are one name.
        /// </summary>
        public string NormalizedParticipantName =>
            string.Join(' ', (name ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    extension(Participant participant)
    {
        /// <summary>
        /// Whether <paramref name="name"/> is this participant's name, ignoring case and white space. Two
        /// participants whose names differ only in that way would be one person entered twice.
        /// </summary>
        /// <param name="name">The name to compare with.</param>
        public bool IsNamed(string? name) =>
            string.Equals(participant.Name, name.NormalizedParticipantName, StringComparison.CurrentCultureIgnoreCase);

        /// <summary>
        /// How well <paramref name="typed"/> matches the start of this participant's name: 0 when the name
        /// starts with it, 1 when a later word of the name does (so a surname finds its owner too), and
        /// <c>null</c> when neither does. Case is ignored. Nothing typed matches every name.
        /// </summary>
        /// <param name="typed">What has been typed so far.</param>
        public int? MatchRank(string? typed)
        {
            var text = typed.NormalizedParticipantName;
            if (text.Length == 0) return 0;
            var compare = CultureInfo.CurrentCulture.CompareInfo;
            if (compare.IsPrefix(participant.Name, text, CompareOptions.IgnoreCase)) return 0;
            var words = participant.Name.Split([' ', '-'], StringSplitOptions.RemoveEmptyEntries);
            return words.Skip(1).Any(word => compare.IsPrefix(word, text, CompareOptions.IgnoreCase)) ? 1 : null;
        }
    }
}
