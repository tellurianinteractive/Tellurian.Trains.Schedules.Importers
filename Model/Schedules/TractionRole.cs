namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Where a traction unit stands in the train it works, and so what it does there beside any other traction
/// unit working the same train on the same sessions.
/// </summary>
/// <remarks>
/// The role is a property of the unit's <see cref="ScheduleAssignment"/>, not of the unit: the same
/// locomotive may lead one working and assist in another. It is what lets a second locomotive be said to
/// work a train in addition to the first, even when the two are assigned to schedules of their own.
/// </remarks>
public enum TractionRole
{
    /// <summary>The unit leads the train. What every unit has unless it is said to assist.</summary>
    Front = 0,

    /// <summary>
    /// The unit stands at the other end of the train from the leading one, so the train has a locomotive
    /// at each end — which is also a cab at the far end, so the train is never run round.
    /// </summary>
    Back = 1,

    /// <summary>
    /// The unit is coupled to the leading one at the same end of the train, to add power. If it can turn
    /// the train without a runaround, so can the pair.
    /// </summary>
    Reinforcement = 2,
}

/// <summary>What the traction roles of a working decide.</summary>
public static class TractionRoleExtensions
{
    extension(IReadOnlyCollection<(ScheduledObject Vehicle, TractionRole Role)> units)
    {
        /// <summary>
        /// Whether the units are locomotives alone: no wagonset or cargo flow works with them, so there is
        /// no train to run round and a reversal never costs a runaround.
        /// </summary>
        /// <param name="carriesVehicles">Whether any wagonset or cargo flow works with the units.</param>
        public bool IsSingleLoco(bool carriesVehicles) =>
            !carriesVehicles && units.Count > 0 && units.All(u => u.Vehicle.ObjectType == ScheduledObjectType.Locomotive);

        /// <summary>
        /// Whether a reversal costs the locomotive its run round to the other end of the train. It does not
        /// where the working has a unit at the back, which is a cab at the far end; nor where every leading
        /// unit turns without a runaround; nor where the leading one cannot but a reinforcing unit at the
        /// same end can, the train then being push/pull all the same. A working with no traction is taken
        /// to need it, so what the planner asked for stands until the units say otherwise. Single locos are
        /// not decided here, since that needs to know what else works with them (see <see cref="IsSingleLoco"/>).
        /// </summary>
        public bool NeedRunaround
        {
            get
            {
                if (units.Count == 0) return true;
                if (units.Any(u => u.Role == TractionRole.Back)) return false;
                var leading = units.Where(u => u.Role == TractionRole.Front).ToList();
                if (leading.Count > 0 && leading.All(u => u.Vehicle.ChangesDirectionWithoutRunaround)) return false;
                return !units.Any(u => u.Role == TractionRole.Reinforcement && u.Vehicle.ChangesDirectionWithoutRunaround);
            }
        }
    }
}
