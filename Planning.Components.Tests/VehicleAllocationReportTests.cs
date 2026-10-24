using Tellurian.Trains.Schedules.Model.Settings;
using Tellurian.Trains.Schedules.Planning.Components.Reporting.Allocation;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Covers the vehicle allocation report: the box a train part gets on a vehicle's line (the run in the train's colour,
/// the preparation and the finishing-up grey), the vehicles under a header for each company, the sheets cut at the graphical
/// timetable's break time, and a train that runs past midnight wrapping to the start of the axis.
/// </summary>
[TestClass]
public class VehicleAllocationReportTests
{
    private static readonly TrainCategory Category = new() { Id = 1, Name = "Passenger", Prefix = "P", Suffix = "x", Color = "#112233" };

    private static LayoutSettings Settings(TimeSpan? breakTime = null)
    {
        var settings = new LayoutSettings();
        settings.General.StartTime = TimeSpan.FromHours(6);
        settings.General.EndTime = TimeSpan.FromHours(20);
        settings.General.BreakTime = breakTime;
        return settings;
    }

    // Two stations and a plan with one train between them: ten minutes of preparation at the origin (arrival 10
    // minutes before the departure), thirty minutes of running, and ten minutes of finishing-up at the destination.
    private static (Plan Plan, Schedule Schedule) Arrange(Time departure, int runMinutes = 30)
    {
        var layout = new Layout { Name = "Test" };
        var first = layout.Add(Station(1, "Munkeröd", "Mkd"));
        var last = layout.Add(Station(2, "Stilkøbing", "Stk"));
        layout.Add(new TrackStretch(1, first, last, 10));
        var timetable = new Timetable("Test", layout);

        var train = new Train(1, Category, 1234) { Sessions = Sessions.All };
        train.Add(new StationCall(1, first["1"], departure.AddMinutes(-10), departure));
        var arrival = train.Add(new StationCall(2, last["1"], departure.AddMinutes(runMinutes), departure.AddMinutes(runMinutes + 10)));
        arrival.IsArrival = true;
        timetable.Add(train);

        var plan = Plan.Create("Test", timetable);
        var schedule = plan.CreateSchedule();
        schedule.Append(train.AsTrainPart);
        return (plan, schedule);
    }

    private static OperationLocation Station(int id, string name, string signature)
    {
        var station = new Station(id, name, signature);
        station.Add(new StationTrack(id * 10, "1"));
        return station;
    }

    private static ScheduledObject Vehicle(Plan plan, Schedule schedule, int number, Company? company)
    {
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", number, company);
        plan.AssignVehicle(schedule, vehicle);
        return vehicle;
    }

    [TestMethod]
    public void ABoxHoldsTheRunningTimeAndTheWorkingTimeAroundIt()
    {
        var (plan, schedule) = Arrange(Time.FromHourAndMinute(8, 5));
        Vehicle(plan, schedule, 1, null);

        var report = VehicleAllocation.Create(plan.ScheduledObjects, Settings(), AllocationGeometry.A4);

        var bar = report.Pages[0].Groups.Single().Rows.Single().Lanes.Single().Single();
        Assert.AreEqual("P 1234 x", bar.Identity, "The train is named as the Trains tab names it: prefix, number and suffix.");
        Assert.AreEqual(new TimeSpan(7, 55, 0), bar.WorkFrom, "The preparation starts at the origin arrival.");
        Assert.AreEqual(new TimeSpan(8, 5, 0), bar.RunFrom);
        Assert.AreEqual(new TimeSpan(8, 35, 0), bar.RunTo);
        Assert.AreEqual(new TimeSpan(8, 45, 0), bar.WorkTo, "The finishing-up ends at the destination departure.");
        Assert.AreEqual("05", bar.StartMinute);
        Assert.AreEqual("35", bar.EndMinute);
        Assert.AreEqual("#112233", bar.Color);
    }

    [TestMethod]
    public void VehiclesAreGroupedUnderTheirCompanyAndThoseWithoutComeLast()
    {
        var (plan, schedule) = Arrange(Time.FromHourAndMinute(8, 0));
        var beta = new Company(2, "Beta Rail", "BR");
        var alpha = new Company(1, "Alpha Rail", "AR");
        Vehicle(plan, schedule, 1, null);
        Vehicle(plan, schedule, 2, beta);
        Vehicle(plan, schedule, 3, alpha);
        Vehicle(plan, schedule, 4, alpha);

        var report = VehicleAllocation.Create(plan.ScheduledObjects, Settings(), AllocationGeometry.A4);

        var groups = report.Pages[0].Groups;
        Assert.IsTrue(report.HasCompanies);
        CollectionAssert.AreEqual(new[] { "Alpha Rail", "Beta Rail", null }, groups.Select(g => g.Company?.Name).ToArray());
        CollectionAssert.AreEqual(new[] { 3, 4 }, groups[0].Rows.Select(r => r.Vehicle.Number).ToArray());
    }

    [TestMethod]
    public void TheBreakTimeCutsTheSheets()
    {
        var (plan, schedule) = Arrange(Time.FromHourAndMinute(8, 0));
        Vehicle(plan, schedule, 1, null);
        var breakTime = TimeSpan.FromHours(13);

        var report = VehicleAllocation.Create(plan.ScheduledObjects, Settings(breakTime), AllocationGeometry.A4);

        Assert.AreEqual(2, report.Pages.Count);
        Assert.AreEqual(breakTime, report.Pages[0].To, "The first sheet ends at the break time.");
        Assert.AreEqual(breakTime, report.Pages[1].From, "The second begins at it: no sheet straddles the break.");
    }

    [TestMethod]
    public void ACompanyThatRunsOverTheSheetRepeatsItsHeaderOnTheNext()
    {
        var (plan, schedule) = Arrange(Time.FromHourAndMinute(8, 0));
        var company = new Company(1, "Alpha Rail", "AR");
        foreach (var number in Enumerable.Range(1, 40)) Vehicle(plan, schedule, number, company);

        var report = VehicleAllocation.Create(plan.ScheduledObjects, Settings(), AllocationGeometry.A4);

        var sheets = report.Pages.Where(page => page.From == report.Pages[0].From).ToList();
        Assert.IsGreaterThan(1, sheets.Count, "Forty lines do not fit one sheet.");
        Assert.IsTrue(sheets.All(sheet => sheet.Groups.Single().Company == company), "Every sheet says whose vehicles it shows.");
        Assert.AreEqual(40, sheets.Sum(sheet => sheet.Groups.Single().Rows.Count), "No line is lost between sheets.");
        var geometry = AllocationGeometry.A4;
        Assert.IsTrue(sheets.All(sheet => geometry.GroupHeaderHeightMm + sheet.Groups.Single().Rows.Sum(geometry.RowHeightMm) <= geometry.AvailableHeightMm),
            "Every sheet holds what it was given.");
    }

    [TestMethod]
    public void ATrainRunningPastMidnightIsDrawnAtBothEndsOfTheDay()
    {
        var (plan, schedule) = Arrange(Time.FromHourAndMinute(23, 30), runMinutes: 50);
        Vehicle(plan, schedule, 1, null);

        var report = VehicleAllocation.Create(plan.ScheduledObjects, Settings(), AllocationGeometry.A4);

        Assert.AreEqual(TimeSpan.Zero, report.Pages[0].From, "A train past midnight makes the axis the whole day.");
        Assert.AreEqual(TimeSpan.FromHours(24), report.Pages[^1].To);
        var bars = report.Pages[0].Groups.Single().Rows.Single().Lanes.Single();
        Assert.HasCount(2, bars, "The box is drawn where the day ends and again where it begins.");
        Assert.AreEqual(bars[1].RunFrom - TimeSpan.FromHours(24), bars[0].RunFrom);
        Assert.AreEqual("20", bars[0].EndMinute, "Wrapping keeps the minutes.");
    }

    [TestMethod]
    public void AnEmptyPlanHasNothingToPrint()
    {
        var report = VehicleAllocation.Create([], Settings(), AllocationGeometry.A4);

        Assert.IsEmpty(report.Pages);
    }
}
