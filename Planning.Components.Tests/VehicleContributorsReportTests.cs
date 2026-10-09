using System.Globalization;
using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Planning.App.Translations;
using Tellurian.Trains.Schedules.Planning.Components.Reporting.Contributors;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Covers the Vehicle contributors report: every rolling stock item arranged for a station owner, for a
/// participant and by DCC address — none left out — and a station or participant always starting a page.
/// </summary>
[TestClass]
public class VehicleContributorsReportTests
{
    private static readonly SessionsSettings Settings = SessionsSettings.UseSessions(4);
    // The labels a row prints, in English.
    private static readonly Dictionary<string, string> English = new()
    {
        ["PrimaryContributor"] = "Primary",
        ["SpareContributor"] = "Spare",
        ["Missing"] = "Missing",
        ["ElectricInText"] = "electric",
        ["DieselInText"] = "diesel",
    };
    private static readonly VehicleContributorCells Cells = new(key => English.GetValueOrDefault(key ?? string.Empty, key ?? string.Empty), Settings);

    // Room for exactly four one-line rows, so a page break is reached without a hundred vehicles.
    private static readonly VehicleContributorsPageGeometry Geometry = VehicleContributorsPageGeometry.A4Landscape with
    {
        PrintableHeightMm = 50,
        HeadingHeightMm = 20,
        ColumnHeaderHeightMm = 10,
        RowHeightMm = 5,
        LineHeightMm = 4,
    };

    // Two stations and four vehicles: an Rc working the 09:00 from Munkeröd on every session, a Da the 08:00 from
    // Stenungsund on sessions 1 and 2, an X2 the 07:00 from Munkeröd on session 2 only, and a wagonset given no work. Bert brings the Rc, with
    // Anna bringing a spare, and the X2; Anna brings the Da; nobody the wagonset; Cecilia nothing at all.
    private static Plan CreatePlan()
    {
        var plan = PlanFactory.CreatePlan("Vehicle contributors", "en");
        var timetable = plan.Timetable!;
        var layout = timetable.Layout;
        var stenungsund = layout.Add(NewStation(1, "Stenungsund", "Snu"));
        var munkerod = layout.Add(NewStation(2, "Munkeröd", "Mkd"));
        layout.Add(new TrackStretch(1, stenungsund, munkerod, 10));

        var early = AddTrain(timetable, 1, stenungsund, munkerod, Time.FromHourAndMinute(8, 0));
        var late = AddTrain(timetable, 2, munkerod, stenungsund, Time.FromHourAndMinute(9, 0));
        var dawn = AddTrain(timetable, 3, munkerod, stenungsund, Time.FromHourAndMinute(7, 0));
        plan.Reconcile();

        var rc = AssignedVehicle(plan, ScheduledObjectType.Locomotive, "Rc", 1, late, null);
        var da = AssignedVehicle(plan, ScheduledObjectType.Locomotive, "Da", 2, early, Sessions.FromSessionNumbers(1, 2));
        var x2 = AssignedVehicle(plan, ScheduledObjectType.Trainset, "X2", 3, dawn, Sessions.FromSessionNumbers(2));
        var wagons = plan.CreateVehicle(ScheduledObjectType.Wagonset, "B", 4, null);
        wagons.NumberOfUnits = 3;

        var bert = plan.FindOrAddParticipant("Bert")!;
        var anna = plan.FindOrAddParticipant("Anna")!;
        plan.FindOrAddParticipant("Cecilia");
        plan.AddContributor(rc, bert, 1234);
        plan.AddContributor(rc, anna, DccAddresses.ToBeProvided, "Arrives on Saturday");
        plan.AddContributor(da, anna, 3);
        plan.AddContributor(x2, bert, 7);
        plan.SetContributionNote(rc, "Needs a sound decoder");
        return plan;
    }

    private static OperationLocation NewStation(int id, string name, string signature)
    {
        var station = new Station(id, name, signature) { IsManned = true };
        station.Add(new StationTrack(id * 10, "1"));
        return station;
    }

    private static Train AddTrain(Timetable timetable, int number, OperationLocation from, OperationLocation to, Time departure)
    {
        var train = new Train(number, 1000 + number) { Category = timetable.TrainCategories.First() };
        train.Add(new StationCall(number * 10 + 1, from["1"], departure.AddMinutes(-5), departure));
        var arrival = departure.AddMinutes(20);
        train.Add(new StationCall(number * 10 + 2, to["1"], arrival, arrival.AddMinutes(5)));
        timetable.Add(train);
        return train;
    }

    private static ScheduledObject AssignedVehicle(
        Plan plan, ScheduledObjectType type, string @class, int number, Train train, Sessions? sessions)
    {
        var schedule = plan.CreateSchedule();
        schedule.Append(train.AsTrainPart);
        var vehicle = plan.CreateVehicle(type, @class, number, null);
        plan.AssignVehicle(schedule, vehicle, sessions);
        return vehicle;
    }

    private static IReadOnlyList<VehicleContributorsGroup> Groups(Plan plan, VehicleContributorsGrouping grouping) =>
        VehicleContributorsList.Create(plan, grouping, Settings);

    private static string[] Classes(VehicleContributorsGroup group) => [.. group.Lines.Select(line => line.Vehicle.Class)];

    [TestMethod]
    public void A_station_lists_the_vehicles_set_up_there_by_track_then_departure()
    {
        var groups = Groups(CreatePlan(), VehicleContributorsGrouping.OperationLocation);

        CollectionAssert.AreEqual(new[] { "Munkeröd", "Stenungsund", null }, groups.Select(group => group.Name).ToArray());
        // Both on track 1: the X2 leaving at 07:00, though only from session 2, before the Rc leaving at 09:00.
        CollectionAssert.AreEqual(new[] { "X2", "Rc", "Rc" }, Classes(groups[0]));
        var rc = groups[0].Lines[1];
        Assert.AreEqual("Bert", rc.OwnerName);
        Assert.AreEqual("Primary", Cells.TextOf(rc, VehicleContributorsColumn.Role));
        Assert.AreEqual(1234, rc.Contributor!.DccAddress, "A station page gives the address of the unit set up there.");
        CollectionAssert.AreEqual(new[] { "Needs a sound decoder" }, rc.Notes.ToArray());
    }

    [TestMethod]
    public void A_station_orders_its_tracks_by_number_as_read_track_2_before_track_10()
    {
        var plan = CreatePlan();
        var timetable = plan.Timetable!;
        var munkerod = timetable.Layout.OperationLocations.Single(location => location.Name == "Munkeröd");
        var stenungsund = timetable.Layout.OperationLocations.Single(location => location.Name == "Stenungsund");
        munkerod.Add(new StationTrack(210, "10"));
        munkerod.Add(new StationTrack(202, "2"));
        AssignedVehicle(plan, ScheduledObjectType.Locomotive, "T43", 10, AddTrainFrom(timetable, 4, munkerod["10"], stenungsund, Time.FromHourAndMinute(6, 0)), null);
        AssignedVehicle(plan, ScheduledObjectType.Locomotive, "Z65", 11, AddTrainFrom(timetable, 5, munkerod["2"], stenungsund, Time.FromHourAndMinute(10, 0)), null);
        plan.Reconcile();

        var lines = Groups(plan, VehicleContributorsGrouping.OperationLocation)[0].Lines;

        CollectionAssert.AreEqual(new[] { "1", "1", "1", "2", "10" }, lines.Select(line => line.Start!.Track.Number).ToArray());
    }

    private static Train AddTrainFrom(Timetable timetable, int number, StationTrack from, OperationLocation to, Time departure)
    {
        var train = new Train(number, 1000 + number) { Category = timetable.TrainCategories.First() };
        train.Add(new StationCall(number * 10 + 1, from, departure.AddMinutes(-5), departure));
        var arrival = departure.AddMinutes(20);
        train.Add(new StationCall(number * 10 + 2, to["1"], arrival, arrival.AddMinutes(5)));
        timetable.Add(train);
        return train;
    }

    [TestMethod]
    public void A_station_lists_each_spare_on_a_row_of_its_own_below_its_primary_shaded_as_a_spare()
    {
        var munkerod = Groups(CreatePlan(), VehicleContributorsGrouping.OperationLocation)[0];

        var spare = munkerod.Lines[2];
        Assert.AreEqual("Anna", spare.OwnerName);
        Assert.IsFalse(spare.IsPrimary);
        Assert.AreEqual("Spare", Cells.TextOf(spare, VehicleContributorsColumn.Role));
        Assert.AreEqual("Missing", Cells.TextOf(spare, VehicleContributorsColumn.DccAddress), "Her address is still to be provided.");
        Assert.AreEqual(VehicleContributorShade.Grey, spare.Shade, "As on the owner's page.");
        Assert.AreEqual(VehicleContributorShade.None, munkerod.Lines[1].Shade, "The primary Rc is in operation on every session.");
        CollectionAssert.AreEqual(new[] { "Arrives on Saturday", "Needs a sound decoder" }, spare.Notes.ToArray());
        CollectionAssert.Contains(munkerod.Kind.Columns.ToArray(), VehicleContributorsColumn.Role);
    }

    [TestMethod]
    public void A_shunter_is_listed_last_on_the_page_of_the_station_it_is_stationed_at()
    {
        var plan = CreatePlan();
        var munkerod = plan.Layout.OperationLocations.Single(location => location.Name == "Munkeröd");
        var shunter = plan.AddShunter(munkerod, "Z65", 5, null, TractionType.Diesel)!;
        plan.AddContributor(shunter, plan.ParticipantNamed("Bert")!, 65);

        var groups = Groups(plan, VehicleContributorsGrouping.OperationLocation);

        var line = groups[0].Lines[^1];
        Assert.AreEqual("Munkeröd", groups[0].Name);
        Assert.AreSame(shunter, line.Vehicle);
        Assert.IsNull(line.Start, "A shunter starts no train part.");
        Assert.AreEqual("Munkeröd", Cells.TextOf(line, VehicleContributorsColumn.Station));
        Assert.AreEqual("Shunter, diesel", Cells.TextOf(line, VehicleContributorsColumn.Type));
        Assert.AreEqual("65", Cells.TextOf(line, VehicleContributorsColumn.DccAddress));
        CollectionAssert.DoesNotContain(groups[^1].Lines.Select(l => l.Vehicle).ToList(), shunter, "It is not 'not in operation'.");
    }

    [TestMethod]
    public void A_vehicle_not_in_operation_is_listed_last_under_a_heading_of_its_own()
    {
        var groups = Groups(CreatePlan(), VehicleContributorsGrouping.OperationLocation);

        var last = groups[^1];
        Assert.AreEqual(VehicleContributorsGroupKind.NotInOperation, last.Kind);
        CollectionAssert.AreEqual(new[] { "B" }, Classes(last));
        Assert.IsNull(last.Lines[0].OwnerName);
        Assert.AreEqual(VehicleContributorCells.Nobody, Cells.TextOf(last.Lines[0], VehicleContributorsColumn.Owner));
        Assert.AreEqual("3", Cells.TextOf(last.Lines[0], VehicleContributorsColumn.Count));
        CollectionAssert.DoesNotContain(last.Kind.Columns.ToArray(), VehicleContributorsColumn.Departure);
    }

    [TestMethod]
    public void An_owner_is_given_what_they_bring_by_first_session_then_station_departure_and_track()
    {
        var groups = Groups(CreatePlan(), VehicleContributorsGrouping.Contributor);

        // The items nobody has booked come first; Cecilia brings nothing, so she gets no page.
        CollectionAssert.AreEqual(new[] { null, "Anna", "Bert" }, groups.Select(group => group.Name).ToArray());
        Assert.AreEqual(VehicleContributorsGroupKind.NotYetBooked, groups[0].Kind);
        CollectionAssert.AreEqual(new[] { "B" }, Classes(groups[0]));

        // Both of Anna's from session 1: Munkeröd before Stenungsund, though the Da leaves earlier.
        CollectionAssert.AreEqual(new[] { "Rc", "Da" }, Classes(groups[1]));
        // Bert's X2 leaves at 07:00, but only from session 2.
        CollectionAssert.AreEqual(new[] { "Rc", "X2" }, Classes(groups[2]));
        var spare = groups[1].Lines[0];
        Assert.IsFalse(spare.IsPrimary);
        Assert.AreEqual("Spare", Cells.TextOf(spare, VehicleContributorsColumn.Role));
        Assert.AreEqual("Missing", Cells.TextOf(spare, VehicleContributorsColumn.DccAddress), "Her address is still to be provided.");
        Assert.AreEqual("Munkeröd", Cells.TextOf(spare, VehicleContributorsColumn.Station));
        CollectionAssert.AreEqual(new[] { "Arrives on Saturday", "Needs a sound decoder" }, spare.Notes.ToArray());
        CollectionAssert.DoesNotContain(groups[1].Kind.Columns.ToArray(), VehicleContributorsColumn.Owner);
    }

    [TestMethod]
    public void An_item_is_described_by_type_with_traction_type_and_class_before_its_turnus_number()
    {
        var plan = CreatePlan();
        var company = new Company(1, "Statens Järnvägar", "SJ");
        var locomotive = plan.RollingStock.Single(v => v.Class == "Rc");
        locomotive.TractionType = TractionType.Electric;
        locomotive.Company = company;
        var munkerod = Groups(plan, VehicleContributorsGrouping.OperationLocation)[0];
        var (rc, x2) = (munkerod.Lines.First(line => line.Vehicle.Class == "Rc"), munkerod.Lines.Single(line => line.Vehicle.Class == "X2"));
        var wagons = Groups(plan, VehicleContributorsGrouping.OperationLocation)[^1].Lines[0];

        CollectionAssert.AreEqual(
            new[] { VehicleContributorsColumn.Type, VehicleContributorsColumn.Turnus, VehicleContributorsColumn.VehicleNumber, VehicleContributorsColumn.Count },
            munkerod.Kind.Columns.SkipWhile(column => column != VehicleContributorsColumn.Type).Take(4).ToArray());
        Assert.AreEqual("Locomotive, electric", Cells.TextOf(rc, VehicleContributorsColumn.Type));
        Assert.AreEqual("Trainset", Cells.TextOf(x2, VehicleContributorsColumn.Type), "Not stated: left out rather than Any.");
        Assert.AreEqual("Wagonset", Cells.TextOf(wagons, VehicleContributorsColumn.Type), "A wagonset is not powered.");
        Assert.AreEqual("SJ 01 Rc", Cells.TextOf(rc, VehicleContributorsColumn.Turnus), "As the Schedules tab names it.");
        Assert.AreEqual("Turnus", Cells.HeadingOf(VehicleContributorsColumn.Turnus));

        x2.Vehicle.TractionType = TractionType.Diesel;
        Assert.AreEqual("Trainset, diesel", Cells.TextOf(x2, VehicleContributorsColumn.Type));
    }

    [TestMethod]
    public void Each_unit_brought_prints_its_own_vehicle_number_after_the_turnus()
    {
        var plan = CreatePlan();
        var rc = plan.RollingStock.Single(v => v.Class == "Rc");
        var contributors = plan.ContributorsOf(rc);
        contributors[0].VehicleNumber = "1328";

        var lines = Groups(plan, VehicleContributorsGrouping.OperationLocation)[0].Lines.Where(line => line.Vehicle == rc).ToList();

        Assert.AreEqual("1328", Cells.TextOf(lines[0], VehicleContributorsColumn.VehicleNumber), "The primary unit's number.");
        Assert.AreEqual("", Cells.TextOf(lines[1], VehicleContributorsColumn.VehicleNumber), "The spare has been given none.");
        Assert.AreEqual("VehicleNumber", Cells.HeadingOf(VehicleContributorsColumn.VehicleNumber), "Its own label.");
    }

    [TestMethod]
    public void A_wagonset_listing_its_wagons_counts_the_wagons()
    {
        var plan = CreatePlan();
        var wagonset = plan.RollingStock.Single(v => v.Class == "B");
        foreach (var @class in new[] { "A", "B", "A", "Fv", "B" }) wagonset.AddWagon(@class);

        var line = Groups(plan, VehicleContributorsGrouping.OperationLocation)[^1].Lines.Single();

        Assert.AreEqual("5", Cells.TextOf(line, VehicleContributorsColumn.Count));
    }

    [TestMethod]
    public void The_first_day_is_printed_by_its_full_name()
    {
        var days = SessionsSettings.UseWeekdays(7, useShortDayNames: true, DayOfWeek.Monday);
        var cells = new VehicleContributorCells(key => key ?? string.Empty, days);
        var plan = CreatePlan();
        var da = VehicleContributorsList.Create(plan, VehicleContributorsGrouping.OperationLocation, days)
            .SelectMany(group => group.Lines)
            .Single(line => line.Vehicle.Class == "Da");

        var original = (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture);
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-GB");
            Assert.AreEqual("Monday", cells.TextOf(da, VehicleContributorsColumn.FirstSession), "Even where the layout names days short.");
            Assert.AreEqual("1", Cells.TextOf(da, VehicleContributorsColumn.FirstSession), "A session is its number.");
        }
        finally
        {
            (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture) = original;
        }
    }

    [TestMethod]
    public void The_address_list_has_every_traction_unit_known_addresses_first_and_no_wagonsets()
    {
        var plan = CreatePlan();
        plan.CreateVehicle(ScheduledObjectType.Locomotive, "Ma", 7, null);
        var bert = plan.ParticipantNamed("Bert")!;
        plan.AddContributor(plan.RollingStock.Single(v => v.Class == "B"), bert, null);

        var all = Groups(plan, VehicleContributorsGrouping.DccAddress).Single();

        Assert.AreEqual(VehicleContributorsGroupKind.AllUnits, all.Kind);
        CollectionAssert.AreEqual(
            new[] { "3", "7", "1234", "Missing", "" },
            all.Lines.Select(line => Cells.TextOf(line, VehicleContributorsColumn.DccAddress)).ToArray());
        CollectionAssert.AreEqual(new[] { "Anna", "Bert", "Bert", "Anna", null }, all.Lines.Select(line => line.OwnerName).ToArray());
        Assert.AreEqual("Ma", all.Lines[^1].Vehicle.Class, "The locomotive nobody brings comes last.");
        Assert.IsTrue(all.Lines.All(line => line.Vehicle.ObjectType != ScheduledObjectType.Wagonset), "A wagonset takes no address, booked or not.");
    }

    [TestMethod]
    public void A_plan_with_only_wagonsets_has_no_address_list()
    {
        var plan = PlanFactory.CreatePlan("Wagons", "en");
        plan.AddContributor(plan.CreateVehicle(ScheduledObjectType.Wagonset, "B", 1, null), plan.FindOrAddParticipant("Anna")!, null);

        Assert.AreEqual(0, Groups(plan, VehicleContributorsGrouping.DccAddress).Count);
        Assert.AreEqual(1, Groups(plan, VehicleContributorsGrouping.Contributor).Count, "Anna's page, and none for the unbooked.");
    }

    [TestMethod]
    public void An_owners_row_is_shaded_yellow_only_when_not_needed_from_the_first_session_and_a_spare_grey()
    {
        var plan = CreatePlan();
        var dawn = plan.Timetable!.Trains.Single(t => t.Id == 3);
        AssignedVehicle(plan, ScheduledObjectType.Wagonset, "Bm", 5, dawn, Sessions.FromSessionNumbers(3));

        var shades = Groups(plan, VehicleContributorsGrouping.Contributor)
            .SelectMany(group => group.Lines)
            .ToDictionary(line => $"{line.Vehicle.Class} {line.OwnerName}", line => line.Shade);

        Assert.AreEqual(VehicleContributorShade.None, shades["Rc Bert"], "In operation on every session.");
        Assert.AreEqual(VehicleContributorShade.Grey, shades["Rc Anna"]);
        Assert.AreEqual(VehicleContributorShade.None, shades["Da Anna"], "Sessions 1 and 2 of 4: needed from the start.");
        Assert.AreEqual(VehicleContributorShade.Yellow, shades["X2 Bert"]);
        Assert.AreEqual(VehicleContributorShade.Yellow, shades["Bm "], "Nobody brings it; the item itself is shaded.");
        Assert.AreEqual(VehicleContributorShade.None, shades["B "], "Not in operation.");
    }

    [TestMethod]
    public void A_station_row_not_in_operation_throughout_is_yellow_from_an_odd_session_and_blue_from_an_even_one()
    {
        var plan = CreatePlan();
        var dawn = plan.Timetable!.Trains.Single(t => t.Id == 3);
        AssignedVehicle(plan, ScheduledObjectType.Wagonset, "Bm", 5, dawn, Sessions.FromSessionNumbers(3));

        var shades = Groups(plan, VehicleContributorsGrouping.OperationLocation)
            .SelectMany(group => group.Lines)
            .ToDictionary(line => $"{line.Vehicle.Class} {line.OwnerName}", line => line.Shade);

        Assert.AreEqual(VehicleContributorShade.None, shades["Rc Bert"], "In operation on every session.");
        Assert.AreEqual(VehicleContributorShade.Grey, shades["Rc Anna"]);
        Assert.AreEqual(VehicleContributorShade.Yellow, shades["Da Anna"], "Sessions 1 and 2 of 4.");
        Assert.AreEqual(VehicleContributorShade.Blue, shades["X2 Bert"]);
        Assert.AreEqual(VehicleContributorShade.Yellow, shades["Bm "], "Nobody brings it; the item itself is shaded.");
        Assert.AreEqual(VehicleContributorShade.None, shades["B "], "Not in operation.");
    }

    [TestMethod]
    public void An_owner_of_a_traction_unit_without_an_address_or_with_zero_is_missing_one()
    {
        var plan = CreatePlan();
        var bert = plan.ParticipantNamed("Bert")!;
        var wagons = plan.RollingStock.Single(v => v.Class == "B");
        plan.AddContributor(wagons, bert, null);
        // A wagonset its owner has since made a locomotive keeps the owner, with no address.
        var goods = plan.CreateVehicle(ScheduledObjectType.Wagonset, "Gbs", 6, null);
        plan.AddContributor(goods, bert, null);
        goods.ObjectType = ScheduledObjectType.Locomotive;

        var lines = Groups(plan, VehicleContributorsGrouping.Contributor)
            .SelectMany(group => group.Lines)
            .ToDictionary(line => $"{line.Vehicle.Class} {line.OwnerName}");

        Assert.IsTrue(lines["Rc Anna"].IsDccAddressMissing, "0: still to be provided.");
        Assert.IsTrue(lines["Gbs Bert"].IsDccAddressMissing, "No address at all.");
        Assert.AreEqual("Missing", Cells.TextOf(lines["Gbs Bert"], VehicleContributorsColumn.DccAddress));
        Assert.IsFalse(lines["B Bert"].IsDccAddressMissing, "A wagonset is not driven, so it needs none.");
        Assert.AreEqual("", Cells.TextOf(lines["B Bert"], VehicleContributorsColumn.DccAddress));
        Assert.IsFalse(lines["Rc Bert"].IsDccAddressMissing);
        CollectionAssert.AreEqual(
            new[] { "Da Anna", "X2 Bert", "Rc Bert", "Rc Anna", "Gbs Bert" },
            Groups(plan, VehicleContributorsGrouping.DccAddress).Single().Lines.Select(line => $"{line.Vehicle.Class} {line.OwnerName}").ToArray(),
            "The missing addresses after the known ones.");
    }

    [TestMethod]
    public void A_plan_without_rolling_stock_prints_nothing()
    {
        var plan = PlanFactory.CreatePlan("Empty", "en");

        foreach (var grouping in Enum.GetValues<VehicleContributorsGrouping>())
            Assert.AreEqual(0, Groups(plan, grouping).Count, grouping.ToString());
    }

    [TestMethod]
    public void Every_group_starts_a_page_and_one_too_long_continues_under_its_heading()
    {
        var plan = CreatePlan();
        for (var number = 10; number < 15; number++)
            plan.AddContributor(plan.CreateVehicle(ScheduledObjectType.Wagonset, $"G{number}", number, null), plan.ParticipantNamed("Anna")!, null);

        var pages = VehicleContributorsPaginator.BuildPages(Groups(plan, VehicleContributorsGrouping.Contributor), Geometry);

        // Anna: the spare Rc, the Da and five wagonsets — seven rows; the Rc's two notes leave room for three.
        CollectionAssert.AreEqual(new[] { null, "Anna", "Anna", "Bert" }, pages.Select(page => page.Group.Name).ToArray());
        CollectionAssert.AreEqual(new[] { 1, 3, 4, 2 }, pages.Select(page => page.Lines.Count).ToArray());
        CollectionAssert.AreEqual(new[] { false, false, true, false }, pages.Select(page => page.IsContinued).ToArray());
    }

    [TestMethod]
    public void Only_the_notes_make_a_row_taller_a_line_for_each_note()
    {
        var plan = CreatePlan();
        var anna = Groups(plan, VehicleContributorsGrouping.Contributor)[1];

        Assert.AreEqual(9, VehicleContributorsPaginator.HeightMmOf(anna.Lines[0], Geometry), "The spare Rc has two notes.");
        Assert.AreEqual(5, VehicleContributorsPaginator.HeightMmOf(anna.Lines[1], Geometry), "The Da has no notes.");
    }

    [TestMethod]
    public void A_wagonset_lists_its_wagons_in_train_order()
    {
        var plan = CreatePlan();
        var wagonset = plan.RollingStock.Single(v => v.Class == "B");
        wagonset.AddWagon("A", "5101");
        wagonset.AddWagon("B");
        wagonset.AddWagon("Fv", "3301");

        var line = Groups(plan, VehicleContributorsGrouping.OperationLocation)[^1].Lines.Single();

        CollectionAssert.AreEqual(new[] { "A", "B", "Fv" }, line.Wagons.Select(wagon => wagon.Class).ToArray());
        CollectionAssert.AreEqual(new[] { "5101", null, "3301" }, line.Wagons.Select(wagon => wagon.Number).ToArray());
        Assert.AreEqual(1, VehicleContributorsPaginator.WagonLinesOf(line, Geometry));
        Assert.AreEqual(5, VehicleContributorsPaginator.HeightMmOf(line, Geometry), "One line of wagons and no notes.");
    }

    [TestMethod]
    public void A_long_rake_wraps_and_makes_the_row_taller()
    {
        var plan = CreatePlan();
        var wagonset = plan.RollingStock.Single(v => v.Class == "B");
        // Each wagon 3 + 2.4 + 1 + 4 x 1.5 = 12.4 mm wide; six fit in 80 mm with the gaps between them.
        for (var i = 0; i < 8; i++) wagonset.AddWagon("B", $"{5100 + i}");

        var line = Groups(plan, VehicleContributorsGrouping.OperationLocation)[^1].Lines.Single();

        Assert.AreEqual(2, VehicleContributorsPaginator.WagonLinesOf(line, Geometry));
        Assert.AreEqual(9, VehicleContributorsPaginator.HeightMmOf(line, Geometry));
    }

    [TestMethod]
    public void A_vehicle_that_is_not_a_wagonset_lists_no_wagons()
    {
        var plan = CreatePlan();
        var anna = Groups(plan, VehicleContributorsGrouping.Contributor)[1];

        Assert.IsEmpty(anna.Lines[0].Wagons);
        Assert.AreEqual(0, VehicleContributorsPaginator.WagonLinesOf(anna.Lines[0], Geometry));
    }
}
