using Tellurian.Trains.Schedules.Planning.Timetables;

namespace Tellurian.Trains.Schedules.Planning.Tests;

/// <summary>
/// A train's <see cref="Train.Calls"/> is in insertion order, which on a hand-edited train is not the order
/// it runs them. Everything here reasons about the route — which legs the train travels, which way it runs
/// them, where it starts — so it must read the calls in run order. Imported plans hide the difference,
/// since there the two orders coincide.
/// </summary>
[TestClass]
public class PlanRunOrderTests
{
    private static Plan SimplePlan() => new("Test", new Timetable("Test", TestLayoutFactory.CreateSimpleLayout()));

    private static TrainCategory Passenger => new() { Id = 1, Name = "Passenger", Prefix = "P", IsPassenger = true, DefaultSpeed = 100 };

    // Malmö 07:50/08:00 → Lund 08:10/08:12 → Eslöv 08:25/08:35, with the calls added in another order than
    // the train runs them: the intermediate Lund stop first, then the origin, then the terminus.
    private static Train CreateTrainWithCallsAddedOutOfRunOrder(Plan plan)
    {
        var category = Passenger;
        var train = new Train(1, category, 2) { Category = category, Sessions = Sessions.All };
        plan.Timetable.Add(train);
        AddCall(train, plan, "Lu", Time.FromHourAndMinute(8, 10), Time.FromHourAndMinute(8, 12), isArrival: true, isDeparture: true);
        AddCall(train, plan, "M2", Time.FromHourAndMinute(7, 50), Time.FromHourAndMinute(8, 00), isArrival: false, isDeparture: true);
        AddCall(train, plan, "E", Time.FromHourAndMinute(8, 25), Time.FromHourAndMinute(8, 35), isArrival: true, isDeparture: false);
        return train;
    }

    private static void AddCall(Train train, Plan plan, string signature, Time arrival, Time departure, bool isArrival, bool isDeparture)
    {
        var location = plan.Layout.OperationLocations.First(l => l.Signature == signature);
        var call = new StationCall(train.Calls.Count + 1, location.Tracks.First(), arrival, departure);
        train.Add(call);
        // Set after adding: Train.Add marks the first call added as departure-only.
        call.IsArrival = isArrival;
        call.IsDeparture = isDeparture;
    }

    private static TimetableStretch MainLine(Plan plan) =>
        plan.Layout.TimetableStretches.First(s => s.Number == "S1");

    [TestMethod]
    public void TheGraphDirectionIsInferredFromTheOrderTheTrainRunsItsCalls()
    {
        var plan = SimplePlan();
        var train = CreateTrainWithCallsAddedOutOfRunOrder(plan);
        var stretch = MainLine(plan);

        var upward = stretch.SortedTrainSegments(plan.Timetable.Trains, TrainGraphDirection.Upward);
        var downward = stretch.SortedTrainSegments(plan.Timetable.Trains, TrainGraphDirection.Downward);

        Assert.Contains(train, upward.Select(s => s.Train).ToList(),
            "The train runs Malmö → Lund → Eslöv, which is upward, whatever order its calls were added in.");
        Assert.IsEmpty(downward, "It does not also belong to the opposite direction's column.");
    }

    [TestMethod]
    public void TimingsAreRecomputedAlongTheLegsTheTrainRuns()
    {
        var plan = SimplePlan();
        var train = CreateTrainWithCallsAddedOutOfRunOrder(plan);
        var origin = train.Calls.Single(c => c.OperationLocation.Signature == "M2");

        var result = plan.UpdateTimings(train);

        Assert.IsNotNull(result, "The legs are Malmö–Lund and Lund–Eslöv, both track stretches of the layout.");
        Assert.AreEqual(Time.FromHourAndMinute(8, 00), origin.Departure, "The origin's departure is the fixed anchor.");
        var lund = train.Calls.Single(c => c.OperationLocation.Signature == "Lu");
        var eslöv = train.Calls.Single(c => c.OperationLocation.Signature == "E");
        Assert.IsTrue(origin.Departure < lund.Arrival, "Lund is reached after the train leaves Malmö.");
        Assert.IsTrue(lund.Departure < eslöv.Arrival, "Eslöv is reached after the train leaves Lund.");
    }

    [TestMethod]
    public void RepeatedClonesAreMeasuredFromTheTrainsOwnDeparture()
    {
        var plan = SimplePlan();
        var train = CreateTrainWithCallsAddedOutOfRunOrder(plan);

        // Departures 08:00 + n·60 up to 10:00 give clones at 09:00 and 10:00.
        var clones = plan.CloneMany(train, Time.FromHourAndMinute(10, 00), 60);

        Assert.HasCount(2, clones, "The sequence is measured from the train's departure from Malmö, not from Lund.");
    }

    [TestMethod]
    public void TheDriverServiceWindowSpansTheWholeRun()
    {
        var plan = SimplePlan();
        var train = CreateTrainWithCallsAddedOutOfRunOrder(plan);

        Assert.AreEqual(Time.FromHourAndMinute(7, 50), train.DriverStartTime, "Service starts when the driver reports at the origin.");
        Assert.AreEqual(Time.FromHourAndMinute(8, 35), train.DriverEndTime, "Service ends when the driver stands down at the terminus.");
        Assert.IsTrue(plan.FitsWithinOperatingWindow(train));
    }
}
