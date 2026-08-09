namespace Tellurian.Trains.Schedules.Model.Validations;

/// <summary>
/// Rules for where a train may call: the operating locations a call can be moved to, and the ones a
/// call added at the end of the run can be at, without breaking the train's route.
/// </summary>
/// <remarks>
/// <para>
/// A route is continuous when every leg the train runs — each pair of calls in run order — is a track
/// stretch of the layout, or stays at one location, which is a train changing track there. That is rule
/// T5 (<c>CheckRouteContinuity</c>), and these rules are the same rule offered forwards: the planner is
/// given the locations that keep the route whole, rather than told afterwards that it broke.
/// </para>
/// <para>
/// A call in the middle of a route is bound on both sides, so it is offered only the locations joined to
/// the call before it <em>and</em> the call after it. That is why re-pointing a middle call cannot open a
/// gap, just as <c>DeletionRules.MayDelete(StationCall)</c> refuses to delete one for the same reason.
/// </para>
/// <para>
/// Two locations are left out. One that would send the train out over a stretch and straight back over it,
/// because the first leg sets the direction and the following legs carry on from it; and one with no track
/// at all, which a train has nowhere to call at. The call's own location is always offered whatever the
/// rules say of it, so a route that already reverses — from an import, or from a stretch taken out of the
/// layout under a train that used it — still shows where the train is instead of appearing to be elsewhere.
/// </para>
/// <para>
/// Nothing here mutates or persists. Moving a call to one of these locations is the caller's to do, and
/// obliges it to save (in the app, <c>ScheduleState.SaveAndNotify()</c>).
/// </para>
/// </remarks>
public static class RouteRules
{
    extension(Train train)
    {
        /// <summary>
        /// The operating locations this call may be moved to, in name order. Never empty for a call the
        /// train holds; a call belonging to another train is offered nothing but where it already is.
        /// </summary>
        /// <param name="call">The call being re-pointed.</param>
        public IReadOnlyList<OperationLocation> LocationChoicesFor(StationCall call)
        {
            if (train is null || call is null) return [];
            var current = call.OperationLocation;
            // Connectivity is a question only the layout can answer. A call always has one, through the
            // track it stands on, so this is a fragment under construction rather than a normal case.
            if (current.Layout is not { } layout) return [current];
            var calls = train.CallsInRunOrder;
            var index = IndexOf(calls, call);
            if (index < 0) return [current];
            return Choices(layout,
                before: LocationAt(calls, index - 1), beyondBefore: LocationAt(calls, index - 2),
                after: LocationAt(calls, index + 1), beyondAfter: LocationAt(calls, index + 2),
                current);
        }

        /// <summary>
        /// The operating locations a call added at the end of the run may be at, in name order. Every
        /// location with a track when the train has no calls yet, since nothing constrains the first one.
        /// </summary>
        /// <param name="layout">
        /// The layout to choose from. Passed in rather than read from the train, which belongs to no
        /// layout until it has its first call.
        /// </param>
        public IReadOnlyList<OperationLocation> LocationChoicesForNextCall(Layout layout)
        {
            if (train is null || layout is null) return [];
            var calls = train.CallsInRunOrder;
            return Choices(layout,
                before: LocationAt(calls, calls.Count - 1), beyondBefore: LocationAt(calls, calls.Count - 2),
                after: null, beyondAfter: null,
                current: null);
        }
    }

    private static IReadOnlyList<OperationLocation> Choices(
        Layout layout,
        OperationLocation? before, OperationLocation? beyondBefore,
        OperationLocation? after, OperationLocation? beyondAfter,
        OperationLocation? current)
    {
        var result = layout.OperationLocations
            .Where(location => location.Tracks.Count > 0)
            .Where(location => CanRun(layout, before, location) && CanRun(layout, location, after))
            .Where(location => !IsReversal(location, before, beyondBefore) && !IsReversal(location, after, beyondAfter))
            .ToList();
        if (current is not null && !result.Any(location => location.Equals(current))) result.Add(current);
        return [.. result.OrderBy(location => location.Name, StringComparer.CurrentCulture)];
    }

    // Rule T5 read forwards. A leg is runnable when it stays where it is — two calls at one location
    // travel no stretch — or when a stretch joins the two, in either direction, a stretch being
    // bidirectional. An open end, where the train has no call on that side, constrains nothing.
    private static bool CanRun(Layout layout, OperationLocation? from, OperationLocation? to) =>
        from is null || to is null || from.Equals(to) || layout.IsConnected(from, to);

    // Out over a stretch and straight back over it: the candidate is where the train stood two calls
    // away, with somewhere else in between. Where that somewhere else is the same location, no stretch
    // was travelled and there is nothing to reverse.
    private static bool IsReversal(OperationLocation candidate, OperationLocation? next, OperationLocation? beyond) =>
        next is not null && beyond is not null && candidate.Equals(beyond) && !next.Equals(beyond);

    private static OperationLocation? LocationAt(IReadOnlyList<StationCall> calls, int index) =>
        index >= 0 && index < calls.Count ? calls[index].OperationLocation : null;

    // By identity, never by value: two calls of one train can compare equal (see StationCall.Equals),
    // and it is the very call being re-pointed whose place in the run order is wanted.
    private static int IndexOf(IReadOnlyList<StationCall> calls, StationCall call)
    {
        for (var i = 0; i < calls.Count; i++)
        {
            if (ReferenceEquals(calls[i], call)) return i;
        }
        return -1;
    }
}
