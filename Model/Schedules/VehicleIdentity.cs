namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// What identifies one physical vehicle, and so may name only one of a plan's vehicles on any one
/// session: the <see cref="ScheduledObject.ExternalId"/> when the vehicle carries one — the identifier it
/// was imported under, which is unique in the system it came from — otherwise its operator and number, or
/// its number alone when it has no operator.
/// </summary>
/// <remarks>
/// The two kinds never match each other: a vehicle with an external id is identified by that alone,
/// whatever operator and number it also happens to carry, and a vehicle without one is identified by
/// operator and number alone. A vehicle created in the planner is given no external id, so it is always
/// the operator and number that identify it.
/// <para>
/// The identity does not include the vehicle type, so a wagonset and a locomotive may not share it either.
/// It is compared through <c>ScheduledObject.Identity</c>; see <c>Plan.VehicleClaiming</c> for the check an
/// editor makes before an edit, and the identity validation (rule P5) for the one made over a whole plan.
/// </para>
/// </remarks>
/// <param name="ExternalId">The external id, upper-cased so it compares case-insensitively, or
/// <c>null</c> when the vehicle is identified by its operator and number instead.</param>
/// <param name="OperatorId">The operating company's <see cref="Company.Id"/>, or <c>null</c> for no
/// operator. Unused when <paramref name="ExternalId"/> is set.</param>
/// <param name="Number">The vehicle number. Unused when <paramref name="ExternalId"/> is set.</param>
public readonly record struct VehicleIdentity(string? ExternalId, int? OperatorId, int Number)
{
    /// <summary>
    /// The identity of a vehicle carrying an external id. A blank id is no id, so it falls back to
    /// <see cref="Of(int?, int)"/> with the given operator and number.
    /// </summary>
    /// <param name="externalId">The external id; blank means the vehicle carries none.</param>
    /// <param name="operatorId">The operator to fall back to when the external id is blank.</param>
    /// <param name="number">The number to fall back to when the external id is blank.</param>
    public static VehicleIdentity Of(string? externalId, int? operatorId, int number) =>
        string.IsNullOrWhiteSpace(externalId)
            ? Of(operatorId, number)
            : new(externalId.Trim().ToUpperInvariant(), null, 0);

    /// <summary>The identity of a vehicle carrying no external id: its operator and number.</summary>
    /// <param name="operatorId">The operating company's <see cref="Company.Id"/>, or <c>null</c>.</param>
    /// <param name="number">The vehicle number.</param>
    public static VehicleIdentity Of(int? operatorId, int number) => new(null, operatorId, number);

    /// <summary>True when the vehicle is identified by its external id rather than operator and number.</summary>
    public bool IsExternalId => ExternalId is not null;
}
