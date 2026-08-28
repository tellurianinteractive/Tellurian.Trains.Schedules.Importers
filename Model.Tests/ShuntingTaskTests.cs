using System.Globalization;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Verifies the shunting task: a train of a shunting category worked at one operating location, whose
/// single call carries the span of the work, and whose cargo flows say which wagons to shunt and in
/// which direction.
/// </summary>
[TestClass]
public class ShuntingTaskTests
{
    private static readonly Time Start = Time.FromHourAndMinute(14, 0);
    private static readonly Time End = Time.FromHourAndMinute(14, 45);

    private static TrainCategory ShuntingCategory => new()
    {
        Id = 3,
        Name = "Shunting",
        Prefix = "V",
        Content = TrainContent.Cargo,
        IsShunting = true,
        StartNumber = 9000,
    };

    // A task at Göteborg from 14:00 to 14:45, in a timetable that also holds the ordinary test trains.
    private static (Timetable Timetable, Train Task, Station Station) Arrange()
    {
        TestDataFactory.Init();
        var timetable = TestDataFactory.CreateTimetable();
        var station = timetable.Layout.OperationLocations.OfType<Station>().First(s => s.Signature == "G");
        return (timetable, AddTask(timetable, station, Start, End), station);
    }

    private static Train AddTask(Timetable timetable, Station station, Time start, Time end)
    {
        var number = 9000 + timetable.Trains.Count;
        var task = new Train(90 + timetable.Trains.Count, ShuntingCategory, number) { Sessions = Sessions.All };
        timetable.Add(task);
        var call = task.Add(new StationCall(90 + timetable.Trains.Count, station["1"], start, end));
        call.IsArrival = true;
        call.IsDeparture = true;
        return task;
    }

    private static CargoFlowOptions FlowTo(Timetable timetable, OperationLocation destination, params OperationLocation[] origins)
    {
        var options = timetable.Add(new CargoFlowOptions());
        options.Destinations.Add(new Destination { Location = destination });
        foreach (var origin in origins) options.Origins.Add(new Origin { Location = origin });
        return options;
    }

    [TestMethod]
    public void ShuntingTaskIsRecognisedFromItsCategory()
    {
        var (_, task, _) = Arrange();

        Assert.IsTrue(task.IsShuntingTask);
        Assert.IsNotNull(task.ShuntingCall);
        Assert.IsFalse(TestDataFactory.CreateTrain1().IsShuntingTask, "An ordinary freight train is no shunting task.");
    }

    [TestMethod]
    public void ShuntingTaskSpansItsSingleCallFromStartToEnd()
    {
        var (_, task, _) = Arrange();

        var part = task.AsTrainPart;

        Assert.AreSame(task.Calls[0], part.From);
        Assert.AreSame(task.Calls[0], part.To);
        // The call is both the task's origin and its destination, so the working span is the whole of it:
        // the arrival is when the work starts, the departure when it ends.
        Assert.AreEqual((Start, End), part.WorkingSpan);
        Assert.AreEqual(Start, task.DriverStartTime);
        Assert.AreEqual(End, task.DriverEndTime);
    }

    [TestMethod]
    public void TheListLabelNamesTheStationOnceAndGivesTheSpanOfTheWork()
    {
        var (timetable, task, _) = Arrange();

        // "V 9002  G 14:00-14:45": one place, and the times the right way round. A travelling train reads
        // the other way — departure, arrow, arrival — because it goes somewhere.
        StringAssert.EndsWith(task.ListLabel, "G 14:00-14:45");
        StringAssert.Contains(timetable.Trains.First(t => !t.IsShuntingTask).ListLabel, "→");
    }

    [TestMethod]
    public void ShuntingTaskCanHostACargoFlowOverItsOneCall()
    {
        var (timetable, task, station) = Arrange();

        Assert.IsTrue(task.CanHostCargoFlow);
        var from = task.CargoFlowDepartureCalls.Single();
        Assert.AreSame(task.Calls[0], from);
        Assert.AreSame(from, task.CargoFlowArrivalCalls(from).Single());
        Assert.AreEqual(1, timetable.Trains.Count(t => t.IsShuntingTask));
        Assert.AreSame(station, from.OperationLocation);
    }

    [TestMethod]
    public void FlowBoundForTheTasksOwnStationIsWorkedOutToTheCargoCustomers()
    {
        var (timetable, task, station) = Arrange();
        var elsewhere = timetable.Layout.OperationLocations.First(l => !l.Equals(station));
        var call = task.Calls[0];

        var flow = task.CreateCargoFlow(1, call, call, FlowTo(timetable, station, elsewhere));

        Assert.AreEqual(ShuntingWork.ToCargoCustomers, flow.ShuntingWork);
        var note = flow.ShuntingNotes.OfType<ShuntArrivingWagonsNote>().Single();
        // The origins are what the driver sorts the arrived wagons by, so they are what the note names.
        StringAssert.Contains(note.ToText, elsewhere.Name);
    }

    [TestMethod]
    public void FlowBoundElsewhereIsFetchedInFromTheCargoCustomers()
    {
        var (timetable, task, station) = Arrange();
        var elsewhere = timetable.Layout.OperationLocations.First(l => !l.Equals(station));
        var call = task.Calls[0];

        var flow = task.CreateCargoFlow(1, call, call, FlowTo(timetable, elsewhere));

        Assert.AreEqual(ShuntingWork.FromCargoCustomers, flow.ShuntingWork);
        var note = flow.ShuntingNotes.OfType<FetchDepartingWagonsNote>().Single();
        StringAssert.Contains(note.ToText, elsewhere.Name);
    }

    [TestMethod]
    public void FlowToAllDestinationsIsFetchedInEvenAtItsOwnStation()
    {
        var (timetable, task, station) = Arrange();
        var call = task.Calls[0];
        var options = FlowTo(timetable, station);
        options.ToAllDestinations = true;

        var flow = task.CreateCargoFlow(1, call, call, options);

        // "All destinations" reaches beyond this station, so the wagons are leaving it rather than
        // arriving at it.
        Assert.AreEqual(ShuntingWork.FromCargoCustomers, flow.ShuntingWork);
    }

    [TestMethod]
    public void TheInstructionReachesTheCallsDriverAndStationNotes()
    {
        var (timetable, task, station) = Arrange();
        var call = task.Calls[0];
        task.CreateCargoFlow(1, call, call, FlowTo(timetable, station));
        var settings = SessionsSettings.UseSessions(14);

        var driverNotes = call.DriverNotes(task.Sessions, settings);
        var stationNotes = call.StationNotes(task.Sessions, settings);

        Assert.HasCount(1, driverNotes.OfType<ShuntArrivingWagonsNote>());
        Assert.HasCount(1, stationNotes.OfType<ShuntArrivingWagonsNote>());
        // It belongs to the half showing the start time: the driver reads it when they begin.
        Assert.IsTrue(driverNotes.OfType<ShuntArrivingWagonsNote>().Single().IsForArrival);
    }

    [TestMethod]
    public void AFlowOnATravellingTrainGivesNoShuntingInstruction()
    {
        var (timetable, _, station) = Arrange();
        var train = timetable.Trains.First(t => !t.IsShuntingTask);
        var calls = train.CallsInRunOrder;

        var flow = train.CreateCargoFlow(1, calls[0], calls[^1], FlowTo(timetable, station));

        Assert.IsEmpty(flow.ShuntingNotes);
        Assert.IsEmpty(calls[0].CargoFlowNotes);
    }

    [TestMethod]
    public void AFlowOnATaskConnectsWhenTheWorkStartsAndDisconnectsWhenItEnds()
    {
        var (timetable, task, station) = Arrange();
        var call = task.Calls[0];

        var flow = task.CreateCargoFlow(1, call, call, FlowTo(timetable, station));

        // Both ends of the flow are the one call, so its times are that call's two halves the way the
        // work reads them: the arrival starts it, the departure ends it. Read them the way a travelling
        // train is read and the planner is shown the end of the work where the start belongs.
        Assert.AreEqual(Start, task.CargoFlowConnectTime(call));
        Assert.AreEqual(End, task.CargoFlowDisconnectTime(call));
        Assert.AreEqual(Start, flow.ConnectTime);
        Assert.AreEqual(End, flow.DisconnectTime);
    }

    [TestMethod]
    public void AFlowOnATravellingTrainConnectsAtTheDepartureAndDisconnectsAtTheArrival()
    {
        var (timetable, _, station) = Arrange();
        var train = timetable.Trains.First(t => !t.IsShuntingTask);
        var calls = train.CallsInRunOrder;

        var flow = train.CreateCargoFlow(1, calls[0], calls[^1], FlowTo(timetable, station));

        Assert.AreEqual(calls[0].Departure, flow.ConnectTime);
        Assert.AreEqual(calls[^1].Arrival, flow.DisconnectTime);
    }

    [TestMethod]
    public void AShuntingTaskContinuesAVehicleWorkingThatLeavesItStandingThere()
    {
        var (timetable, taskAtGöteborg, _) = Arrange();
        // The test train runs Göteborg → Ytterby → Stenungsund, arriving 12:55, so a task at Stenungsund
        // afterwards is what the locomotive goes on to; the one back at Göteborg is not.
        var terminus = timetable.Layout.OperationLocations.OfType<Station>().First(s => s.Signature == "Snu");
        var taskAtTerminus = AddTask(timetable, terminus, Time.FromHourAndMinute(13, 0), Time.FromHourAndMinute(13, 30));
        var inbound = timetable.Trains.First(t => !t.IsShuntingTask);
        var plan = Plan.Create("Test", timetable);
        var schedule = plan.CreateSchedule();
        schedule.Append(inbound.AsTrainPart);

        var candidates = plan.CandidateTrainsFor(schedule);

        Assert.Contains(taskAtTerminus, candidates, "The task stands where the working leaves the locomotive.");
        Assert.DoesNotContain(taskAtGöteborg, candidates, "The task back at the origin is not reachable.");
        Assert.IsTrue(schedule.Append(taskAtTerminus.AsTrainPart).HasValue);
        Assert.AreSame(taskAtTerminus, schedule.LastPart!.Train);
    }

    [TestMethod]
    public void TheInstructionIsWrittenInEveryLanguageTheAppIsRead()
    {
        var (timetable, task, station) = Arrange();
        var call = task.Calls[0];
        var flow = task.CreateCargoFlow(1, call, call, FlowTo(timetable, station));
        var original = CultureInfo.CurrentUICulture;
        try
        {
            // Each language has its own wording; what is checked is that a translation exists at all —
            // a missing key would leave the note reading in English, or empty.
            foreach (var (language, word) in new[] { ("sv", "godskund"), ("da", "godskund"), ("nb", "godskund"), ("de", "Güterkunden"), ("en", "cargo customers") })
            {
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(language);
                var text = flow.ShuntingNotes.Single().ToText;
                StringAssert.Contains(text, word, StringComparison.CurrentCultureIgnoreCase, $"Note text in {language}: '{text}'.");
            }
        }
        finally
        {
            CultureInfo.CurrentUICulture = original;
        }
    }

    [TestMethod]
    public void AShuntingTaskWithMoreThanOneCallIsReported()
    {
        var (timetable, task, _) = Arrange();
        var other = timetable.Layout.OperationLocations.OfType<Station>().First(s => s.Signature == "Yb");
        task.Add(new StationCall(98, other["1"], End.AddMinutes(10), End.AddMinutes(20)));

        var errors = task.CheckShuntingTaskCalls().ToList();

        Assert.HasCount(1, errors);
        Assert.AreEqual(ValidationErrorType.ShuntingTaskCallCount, errors[0].ErrorType);
    }

    [TestMethod]
    public void AShuntingTaskWithItsOneCallIsNotReported()
    {
        var (_, task, _) = Arrange();

        Assert.IsEmpty(task.CheckShuntingTaskCalls());
    }
}
