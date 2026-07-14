using Tellurian.Trains.Schedules.Planning.Timetables;

namespace Tellurian.Trains.Schedules.Planning.Tests;

[TestClass]
public class WagonOrderingTests
{
    // A reversing layout: A and C both connect to B so that a train A->B->C reverses at B. The stretches
    // are oriented to END at B, so travelling B->C runs backward relative to the C-B stretch.
    //   A ──(1: A->B)──> B <──(2: C->B)── C
    private static (Plan Plan, Train Forward, Train Return) CreateReversingSchedule()
    {
        var a = Station(1, "A");
        var b = Station(2, "B");
        var c = Station(3, "C");

        var layout = new Layout { Name = "Reversing" };
        layout.Add(a);
        layout.Add(b);
        layout.Add(c);
        layout.Add(new TrackStretch(1, a, b, 10));
        layout.Add(new TrackStretch(2, c, b, 10));

        var category = new TrainCategory { Id = 1, Name = "G", Prefix = "G" };
        var timetable = new Timetable("Test", layout);

        // Train 1: A -> B (reverse) -> C
        var forward = new Train(1, category, 1) { Category = category };
        var t0 = Time.FromHourAndMinute(10, 0);
        _ = forward.Add(new StationCall(1, a["1"], t0, t0));
        _ = forward.Add(new StationCall(2, b["1"], t0.AddMinutes(20), t0.AddMinutes(25)));
        _ = forward.Add(new StationCall(3, c["1"], t0.AddMinutes(45), t0.AddMinutes(45)));
        timetable.Add(forward);

        // Train 2: C -> B (reverse) -> A
        var @return = new Train(2, category, 2) { Category = category };
        var t1 = Time.FromHourAndMinute(11, 0);
        _ = @return.Add(new StationCall(4, c["1"], t1, t1));
        _ = @return.Add(new StationCall(5, b["1"], t1.AddMinutes(20), t1.AddMinutes(25)));
        _ = @return.Add(new StationCall(6, a["1"], t1.AddMinutes(45), t1.AddMinutes(45)));
        timetable.Add(@return);

        return (new Plan("Test", timetable), forward, @return);
    }

    private static Station Station(int id, string signature)
    {
        var station = new Station(id, signature, signature);
        station.Add(new StationTrack(id * 10 + 1, "1"));
        return station;
    }

    [TestMethod]
    public void WagonOrderFlipsAtEachDirectionChangeAcrossTheWholeSchedule()
    {
        var (plan, forward, @return) = CreateReversingSchedule();
        var wagonset = new ScheduledObject(1, ScheduledObjectType.Wagonset, 1) { Plan = plan };
        wagonset.AddWagon("W1");
        wagonset.AddWagon("W2");
        wagonset.AddWagon("W3");

        var legs = wagonset.WagonOrderByLeg([forward.AsTrainPart, @return.AsTrainPart]);

        // Four legs A-B, B-C, C-B, B-A with the rake flipping at every direction change (at B, C and B).
        Assert.HasCount(4, legs);
        AssertLeg(legs[0], "A", "B", UnitOrder.AsArranged, [1, 2, 3]);
        AssertLeg(legs[1], "B", "C", UnitOrder.Reversed, [3, 2, 1]);
        AssertLeg(legs[2], "C", "B", UnitOrder.AsArranged, [1, 2, 3]);
        AssertLeg(legs[3], "B", "A", UnitOrder.Reversed, [3, 2, 1]);
    }

    private static void AssertLeg(ScheduleWagonLeg leg, string from, string to, UnitOrder order, int[] wagonOrder)
    {
        Assert.AreEqual(from, leg.From.OperationLocation.Signature, "from");
        Assert.AreEqual(to, leg.To.OperationLocation.Signature, "to");
        Assert.AreEqual(order, leg.Order, $"order on {from}-{to}");
        CollectionAssert.AreEqual(wagonOrder, leg.Wagons.Select(w => w.Position).ToArray(), $"wagons on {from}-{to}");
    }
}
