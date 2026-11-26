using Tellurian.Trains.Schedules.Model.Notes;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Covers <see cref="TrainCategory.ShowNameOnDispatchList"/>: the category name as the first station note at
/// the calls where the train stops, and nowhere else.
/// </summary>
[TestClass]
public class TrainCategoryNoteTests
{
    private static readonly SessionsSettings Settings = new();

    // A train from Ytterby through Kode, where it passes, to Stenungsund.
    private static Train CreateTrain(bool showName)
    {
        var category = new TrainCategory { Id = 1, Name = "Regional", Prefix = "R", Content = TrainContent.Passenger, ShowNameOnDispatchList = showName };
        var layout = new Layout { Id = 1, Name = "Test" };
        var timetable = new Timetable("Test", layout);
        timetable.TrainCategories.Add(category);
        var train = new Train(1, category, 100) { Category = category, Sessions = Sessions.All };
        var time = Time.FromHourAndMinute(8, 0);
        var id = 1;
        foreach (var (name, signature) in new[] { ("Ytterby", "Yb"), ("Kode", "Kd"), ("Stenungsund", "Sts") })
        {
            var station = new Station(id, name, signature);
            var track = new StationTrack(id, "1");
            station.Add(track);
            layout.OperationLocations.Add(station);
            var call = new StationCall(id, track, time, time.AddMinutes(signature == "Kd" ? 0 : 2));
            train.Add(call);
            call.IsArrival = signature == "Sts";
            call.IsDeparture = signature == "Yb";
            time = time.AddMinutes(20);
            id++;
        }
        timetable.Add(train);
        return train;
    }

    private static List<ICallNote> NotesAt(Train train, int index) =>
        [.. train.CallsInRunOrder[index].StationNotes(train.Sessions, Settings)];

    [TestMethod]
    public void TheNameIsTheNoteAtEveryStop()
    {
        var train = CreateTrain(showName: true);

        foreach (var index in new[] { 0, 2 })
        {
            var note = NotesAt(train, index).OfType<TrainCategoryNote>().Single();
            Assert.AreEqual("Regional", note.ToText);
            Assert.IsTrue(note.IsForArrival && note.IsForDeparture, "it goes on both the arrival and the departure row");
        }
    }

    [TestMethod]
    public void ThereIsNoNameWhereTheTrainPasses() =>
        Assert.IsEmpty(NotesAt(CreateTrain(showName: true), 1).OfType<TrainCategoryNote>());

    [TestMethod]
    public void ThereIsNoNameUnlessTheCategoryAsks() =>
        Assert.IsEmpty(CreateTrain(showName: false).Calls.SelectMany(c => c.StationNotes(Sessions.All, Settings)).OfType<TrainCategoryNote>());

    [TestMethod]
    public void TheNameComesBeforeEveryOtherNote()
    {
        var train = CreateTrain(showName: true);
        train.CallsInRunOrder[0].Notes.Add(new TextCallNote("Ring the dispatcher.", "en", displayOrder: -100) { IsForDeparture = true });

        var notes = NotesAt(train, 0);

        Assert.IsInstanceOfType<TrainCategoryNote>(notes[0]);
        Assert.HasCount(2, notes);
    }

    [TestMethod]
    public void TheNameIsNotADriverNote() =>
        Assert.IsFalse(NotesAt(CreateTrain(showName: true), 0).OfType<TrainCategoryNote>().Single().IsDriverNote);
}
