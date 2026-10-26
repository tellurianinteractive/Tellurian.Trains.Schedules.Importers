namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// The roles a person can fill at a manned station. A person fills one or several: at a station manned
/// by one person that person usually is dispatcher, switchgear operator and yard manager at once.
/// </summary>
[Flags]
public enum StaffRoles
{
    /// <summary>No role.</summary>
    None = 0,

    /// <summary>Clears trains to and from the neighbouring stations.</summary>
    Dispatcher = 1,

    /// <summary>Controls the routes within the station.</summary>
    SwitchgearOperator = 2,

    /// <summary>Sees to it that traction units, wagonsets and cargo wagons are on the right train.</summary>
    YardManager = 4,

    /// <summary>Operates the shunter locomotives in the shunting yard.</summary>
    Shuntman = 8,

    /// <summary>Is there to learn about operations at the station.</summary>
    Trainee = 16,
}

/// <summary>
/// One person staffing a station, as the roles they fill. Posts are anonymous: they say what is to be
/// done, not by whom.
/// </summary>
public class StaffPost
{
    /// <summary>The roles this person fills.</summary>
    public StaffRoles Roles { get; set; }
}

/// <summary>Provides extension members for <see cref="StaffRoles"/>.</summary>
public static class StaffRolesExtensions
{
    /// <summary>The roles in the order they are offered.</summary>
    public static IReadOnlyList<StaffRoles> All { get; } =
    [
        StaffRoles.Dispatcher, StaffRoles.SwitchgearOperator, StaffRoles.YardManager, StaffRoles.Shuntman, StaffRoles.Trainee,
    ];

    /// <summary>The roles of a station manned by one person: all three of the station's own.</summary>
    public const StaffRoles SoleOperator = StaffRoles.Dispatcher | StaffRoles.SwitchgearOperator | StaffRoles.YardManager;

    extension(StaffPost post)
    {
        /// <summary>Whether the person fills <paramref name="role"/>.</summary>
        public bool Fills(StaffRoles role) => post.Roles.HasFlag(role);

        /// <summary>Makes the person fill, or stop filling, <paramref name="role"/>.</summary>
        public void Set(StaffRoles role, bool fills) =>
            post.Roles = fills ? post.Roles | role : post.Roles & ~role;
    }
}
