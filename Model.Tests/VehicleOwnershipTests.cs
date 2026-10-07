using System.Text.Json;
using Tellurian.Trains.Schedules.Model.Settings;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Verifies who brings the rolling stock of a plan: the participant catalogue and its name matching, the
/// contributors of each rolling stock item with the DCC address a traction unit needs, where each item is
/// to stand before the meeting, and that all of it survives a save.
/// </summary>
[TestClass]
public class VehicleOwnershipTests
{
    private static readonly JsonSerializerOptions Options = PlanJson.CreateOptions();

    // Forward 12:00 from G track 3; return 13:00 from Snu track 2.
    private static Plan CreatePlan()
    {
        TestDataFactory.Init();
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00)));
        timetable.Add(TestDataFactory.CreateTrainInOppositeDirection(category, 2, Time.FromHourAndMinute(13, 00)));
        return Plan.Create("Test", timetable);
    }

    private static Train Forward(Plan plan) => plan.Timetable.Trains.First(t => t.Number == 1);
    private static Train Return(Plan plan) => plan.Timetable.Trains.First(t => t.Number == 2);

    private static ScheduledObject Loco(Plan plan) => plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 1, null);
    private static ScheduledObject Wagons(Plan plan) => plan.CreateVehicle(ScheduledObjectType.Wagonset, "Bm", 2, null);

    // ---- Participant catalogue ------------------------------------------------------------------

    [TestMethod]
    public void AParticipantIsFoundByTheStartOfTheirNameOrOfALaterWord()
    {
        var plan = CreatePlan();
        var anna = plan.FindOrAddParticipant("Anna Berg")!;
        var bert = plan.FindOrAddParticipant("Bert Andersson")!;
        var carl = plan.FindOrAddParticipant("Carl-Anders Lind")!;

        CollectionAssert.AreEqual(new[] { anna, bert, carl }, plan.ParticipantsMatching("an").ToList(),
            "A name starting with it comes before the names with a later word starting with it.");
        CollectionAssert.AreEqual(new[] { bert, anna }, plan.ParticipantsMatching("BER").ToList(),
            "Case is ignored, and Bert's name starting with it puts him before Anna's surname.");
        CollectionAssert.AreEqual(new[] { carl }, plan.ParticipantsMatching("lind").ToList(), "A surname finds its owner.");
        Assert.HasCount(3, plan.ParticipantsMatching("  "), "Nothing typed offers everyone.");
        Assert.IsEmpty(plan.ParticipantsMatching("erg"), "The middle of a word is no match.");
    }

    [TestMethod]
    public void ANameAlreadyInTheCatalogueIsNotAddedAgain()
    {
        var plan = CreatePlan();
        var anna = plan.FindOrAddParticipant("  Anna   Berg ")!;

        Assert.AreEqual("Anna Berg", anna.Name, "White space around and inside the name is tidied away.");
        Assert.AreSame(anna, plan.FindOrAddParticipant("anna berg"), "Case and spacing do not make another person.");
        Assert.IsNull(plan.FindOrAddParticipant(" "), "A blank name names nobody.");
        var bert = plan.FindOrAddParticipant("Bert")!;
        Assert.AreNotEqual(anna.Id, bert.Id);
        Assert.HasCount(2, plan.Participants);
    }

    [TestMethod]
    public void AParticipantCannotBeRenamedToBlankOrToSomeoneElsesName()
    {
        var plan = CreatePlan();
        var anna = plan.FindOrAddParticipant("Anna Berg")!;
        plan.FindOrAddParticipant("Bert");

        Assert.IsFalse(plan.RenameParticipant(anna, ""));
        Assert.IsFalse(plan.RenameParticipant(anna, "bert"));
        Assert.AreEqual("Anna Berg", anna.Name, "A refused rename leaves the name as it was.");
        Assert.IsTrue(plan.RenameParticipant(anna, "anna berg"), "Their own name in another case is still theirs.");
        Assert.AreEqual("anna berg", anna.Name);
    }

    // ---- Contributors ---------------------------------------------------------------------------

    [TestMethod]
    public void TheFirstContributorIsThePrimaryOneAndTheRestBringSpares()
    {
        var plan = CreatePlan();
        var loco = Loco(plan);
        var anna = plan.AddContributor(loco, plan.FindOrAddParticipant("Anna")!, 3)!;
        var bert = plan.AddContributor(loco, plan.FindOrAddParticipant("Bert")!, 4, "Arrives on day two")!;

        var contribution = plan.ContributionFor(loco)!;
        Assert.AreSame(anna, contribution.PrimaryContributor);
        CollectionAssert.AreEqual(new[] { bert }, contribution.SpareContributors.ToList());
        Assert.AreEqual("Arrives on day two", bert.Note);

        Assert.IsTrue(plan.MakePrimary(loco, bert));
        CollectionAssert.AreEqual(new[] { bert, anna }, plan.ContributorsOf(loco).ToList(),
            "The one replaced becomes the first to bring a spare.");
    }

    [TestMethod]
    public void ATractionUnitIsNotBroughtWithoutADccAddressSparesIncluded()
    {
        var plan = CreatePlan();
        var loco = Loco(plan);
        var anna = plan.FindOrAddParticipant("Anna")!;
        var bert = plan.FindOrAddParticipant("Bert")!;

        Assert.IsNull(plan.AddContributor(loco, anna, null), "The primary contributor needs an address.");
        Assert.IsNull(plan.AddContributor(loco, anna, DccAddresses.Highest + 1), "An address out of range is refused.");
        var primary = plan.AddContributor(loco, anna, 218);
        Assert.IsNotNull(primary);
        Assert.IsNull(plan.AddContributor(loco, bert, null), "A spare needs an address of its own too.");
        var spare = plan.AddContributor(loco, bert, DccAddresses.ToBeProvided);
        Assert.IsNotNull(spare, "Zero is accepted: the owner is to provide the address.");

        Assert.IsFalse(plan.SetDccAddress(loco, primary, null), "An address can be changed but not cleared.");
        Assert.AreEqual(218, primary.DccAddress);
        Assert.IsTrue(plan.SetDccAddress(loco, primary, 3));
        Assert.AreEqual(3, primary.DccAddress);
    }

    [TestMethod]
    public void AWagonsetIsBroughtWithoutADccAddressAndACargoFlowIsNotBroughtAtAll()
    {
        var plan = CreatePlan();
        var anna = plan.FindOrAddParticipant("Anna")!;
        var wagons = Wagons(plan);
        var cargoFlow = plan.CreateVehicle(ScheduledObjectType.CargoFlow, null, 3, null);

        Assert.IsNull(plan.AddContributor(wagons, anna, 3), "A wagonset is not driven, so it has no address.");
        Assert.IsNotNull(plan.AddContributor(wagons, anna, null));
        Assert.IsNull(plan.AddContributor(cargoFlow, anna, null), "A cargo flow is not rolling stock anyone brings.");
        CollectionAssert.DoesNotContain(plan.RollingStock.ToList(), cargoFlow);
    }

    [TestMethod]
    public void RemovingThePrimaryContributorLetsTheFirstSpareTakeItsPlace()
    {
        var plan = CreatePlan();
        var loco = Loco(plan);
        var anna = plan.AddContributor(loco, plan.FindOrAddParticipant("Anna")!, 3)!;
        var bert = plan.AddContributor(loco, plan.FindOrAddParticipant("Bert")!, 4)!;

        Assert.IsTrue(plan.TryDelete(anna).IsSuccess);

        Assert.AreSame(bert, plan.ContributionFor(loco)!.PrimaryContributor);
        Assert.HasCount(2, plan.Participants, "The participant stays in the catalogue.");
    }

    [TestMethod]
    public void AnItemWithNoContributorAndNoNoteKeepsNoContribution()
    {
        var plan = CreatePlan();
        var loco = Loco(plan);

        plan.SetContributionNote(loco, " ");
        Assert.IsEmpty(plan.VehicleContributions, "A blank note on its own records nothing.");

        var anna = plan.AddContributor(loco, plan.FindOrAddParticipant("Anna")!, 3)!;
        plan.SetContributionNote(loco, "Needs a sound decoder update");
        plan.TryDelete(anna);
        Assert.HasCount(1, plan.VehicleContributions, "The note is still worth keeping.");

        plan.SetContributionNote(loco, "");
        Assert.IsEmpty(plan.VehicleContributions);
    }

    [TestMethod]
    public void AParticipantBringingRollingStockCannotBeDeleted()
    {
        var plan = CreatePlan();
        var loco = Loco(plan);
        var anna = plan.FindOrAddParticipant("Anna")!;
        var contributor = plan.AddContributor(loco, anna, 3)!;

        var deletion = plan.MayDelete(anna);
        Assert.IsInstanceOfType<DeletionResult.Failure>(deletion);
        Assert.AreEqual(loco.Designation, ((DeletionResult.Failure)deletion).References.Single().Name);
        Assert.IsTrue(plan.TryDelete(anna).IsDenied);

        plan.TryDelete(contributor);
        Assert.IsTrue(plan.TryDelete(anna).IsSuccess);
        Assert.IsEmpty(plan.Participants);
    }

    [TestMethod]
    public void TheRollingStockAParticipantBringsSaysWhereTheyAreThePrimaryOne()
    {
        var plan = CreatePlan();
        var loco = Loco(plan);
        var wagons = Wagons(plan);
        var anna = plan.FindOrAddParticipant("Anna")!;
        var bert = plan.FindOrAddParticipant("Bert")!;
        plan.AddContributor(loco, anna, 3);
        plan.AddContributor(wagons, bert, null);
        plan.AddContributor(wagons, anna, null);

        var brought = plan.RollingStockBroughtBy(anna);

        CollectionAssert.AreEqual(new[] { (loco, true), (wagons, false) }, brought.ToList());
    }

    // ---- Where an item starts -------------------------------------------------------------------

    [TestMethod]
    public void AVehicleStartsWhereItsFirstPartOnItsFirstSessionDeparts()
    {
        var plan = CreatePlan();
        var odd = plan.CreateSchedule();
        odd.Append(Forward(plan).AsTrainPart);
        var even = plan.CreateSchedule();
        even.Append(Return(plan).AsTrainPart);
        var loco = Loco(plan);
        plan.AssignVehicle(odd, loco, Sessions.FromSessionNumbers(3, 5));
        plan.AssignVehicle(even, loco, Sessions.FromSessionNumbers(2, 4));

        var start = loco.Start(useDays: false, maxSessions: 6);

        Assert.IsNotNull(start);
        Assert.AreEqual(2, start.FirstPosition, "Session 2 is the first it is in operation.");
        Assert.AreEqual(Return(plan), start.FirstPart.Train);
        Assert.AreEqual(Return(plan).CallsInRunOrder[0].Track, start.Track);
        Assert.AreEqual(Time.FromHourAndMinute(13, 00), start.Departure);
        Assert.IsFalse(start.IsEverySession, "Sessions 1 and 6 it works nothing.");
    }

    [TestMethod]
    public void AVehicleWorkingEverySessionOfThePeriodIsInOperationThroughout()
    {
        var plan = CreatePlan();
        var odd = plan.CreateSchedule();
        odd.Append(Forward(plan).AsTrainPart);
        var even = plan.CreateSchedule();
        even.Append(Return(plan).AsTrainPart);
        var loco = Loco(plan);
        plan.AssignVehicle(odd, loco, Sessions.FromSessionNumbers(1, 3));
        plan.AssignVehicle(even, loco, Sessions.FromSessionNumbers(2, 4));

        Assert.IsTrue(loco.Start(useDays: false, maxSessions: 4)!.IsEverySession, "Two schedules between them work all four.");
        Assert.IsFalse(loco.Start(useDays: false, maxSessions: 5)!.IsEverySession, "Nothing on session 5.");
    }

    [TestMethod]
    public void AVehicleStartsAtItsFirstPartFromWhenTheFirstSessionBegins()
    {
        var (plan, loco) = PlanWithRoundTrip();

        var start = loco.Start(useDays: false, maxSessions: 7, TimeSpan.FromHours(12.5));

        Assert.IsNotNull(start);
        Assert.AreEqual(1, start.FirstPosition);
        Assert.AreEqual(Return(plan), start.FirstPart.Train, "The 12:00 departure is before the meeting begins.");
    }

    [TestMethod]
    public void AVehicleWorkingNothingAfterTheFirstSessionBeginsStartsOnTheNextSession()
    {
        var (plan, loco) = PlanWithRoundTrip();

        var start = loco.Start(useDays: false, maxSessions: 7, TimeSpan.FromHours(14));

        Assert.IsNotNull(start);
        Assert.AreEqual(2, start.FirstPosition);
        Assert.AreEqual(Forward(plan), start.FirstPart.Train);
    }

    [TestMethod]
    public void TheFirstSessionStartTimeCountsOnlyWhenRunningOverMidnight()
    {
        var general = new GeneralSettings { FirstSessionStartTime = TimeSpan.FromHours(6) };
        Assert.AreEqual(TimeSpan.Zero, general.FirstSessionStart);

        general.RunsOverMidnight = true;
        Assert.AreEqual(TimeSpan.FromHours(6), general.FirstSessionStart);
    }

    // A loco working the 12:00 out and the 13:00 back every day of the week.
    private static (Plan Plan, ScheduledObject Loco) PlanWithRoundTrip()
    {
        var plan = CreatePlan();
        var schedule = plan.CreateSchedule();
        schedule.Append(Forward(plan).AsTrainPart);
        schedule.Append(Return(plan).AsTrainPart);
        var loco = Loco(plan);
        plan.AssignVehicle(schedule, loco, Sessions.FromSessionNumbers(1, 2, 3, 4, 5, 6, 7));
        return (plan, loco);
    }

    [TestMethod]
    public void AVehicleWorkingNoTrainHasNoStart()
    {
        var plan = CreatePlan();

        Assert.IsNull(Loco(plan).Start(useDays: false, maxSessions: 6));
    }

    // ---- Persistence ----------------------------------------------------------------------------

    [TestMethod]
    public void ContributorsTheirOrderAddressesAndNotesSurviveASave()
    {
        var plan = CreatePlan();
        var loco = Loco(plan);
        plan.AddContributor(loco, plan.FindOrAddParticipant("Anna Berg")!, 218, "Brings the sound decoder");
        plan.AddContributor(loco, plan.FindOrAddParticipant("Bert")!, DccAddresses.ToBeProvided);
        plan.SetContributionNote(loco, "Weathered");

        var restored = JsonSerializer.Deserialize<Plan>(JsonSerializer.Serialize(plan, Options), Options);

        Assert.IsNotNull(restored);
        CollectionAssert.AreEqual(new[] { "Anna Berg", "Bert" }, restored.Participants.Select(p => p.Name).ToList());
        var restoredLoco = restored.ScheduledObjects.Single(v => v.Id == loco.Id);
        var contribution = restored.ContributionFor(restoredLoco);
        Assert.IsNotNull(contribution);
        Assert.AreEqual("Weathered", contribution.Note);
        var contributors = contribution.Contributors;
        Assert.HasCount(2, contributors);
        Assert.AreEqual("Anna Berg", restored.ParticipantById(contributors[0].ParticipantId)!.Name, "The primary one stays first.");
        Assert.AreEqual(218, contributors[0].DccAddress);
        Assert.AreEqual("Brings the sound decoder", contributors[0].Note);
        Assert.AreEqual(DccAddresses.ToBeProvided, contributors[1].DccAddress);
    }

    [TestMethod]
    public void APlanSavedBeforeVehicleOwnersExistedReadsWithNone()
    {
        var plan = CreatePlan();
        var json = JsonSerializer.Serialize(plan, Options)
            .Replace(",\"Participants\":{\"$id\":", ",\"IgnoredParticipants\":{\"$id\":", StringComparison.Ordinal)
            .Replace(",\"VehicleContributions\":{\"$id\":", ",\"IgnoredVehicleContributions\":{\"$id\":", StringComparison.Ordinal);
        Assert.DoesNotContain("\"Participants\":", json, StringComparison.Ordinal, "Precondition: the properties are gone.");

        var restored = JsonSerializer.Deserialize<Plan>(json, Options);

        Assert.IsNotNull(restored);
        Assert.IsEmpty(restored.Participants);
        Assert.IsEmpty(restored.VehicleContributions);
    }
}
