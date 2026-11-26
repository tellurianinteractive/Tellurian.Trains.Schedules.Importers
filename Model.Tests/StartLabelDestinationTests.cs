using Tellurian.Trains.Schedules.Model.Schedules;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>Covers <see cref="StartLabel.Destination"/>: where the train on the label runs to.</summary>
[TestClass]
public class StartLabelDestinationTests
{
    [TestMethod]
    public void TheDestinationIsTheLastCallInRunOrder()
    {
        var category = new TrainCategory { Id = 1, Name = "Freight", Prefix = "G", Content = TrainContent.Cargo };
        var train = new Train(1, category, 100) { Category = category };
        var origin = new Station(1, "Ytterby", "Yb");
        origin.Add(new StationTrack(1, "1"));
        var destination = new Station(2, "Stenungsund", "Sts");
        destination.Add(new StationTrack(2, "1"));
        // Added destination first: the label must follow the run order, not the order the calls were added in.
        train.Add(new StationCall(2, destination.Tracks[0], Time.FromHourAndMinute(9, 0), Time.FromHourAndMinute(9, 5)));
        var departure = new StationCall(1, origin.Tracks[0], Time.FromHourAndMinute(8, 0), Time.FromHourAndMinute(8, 10));
        train.Add(departure);

        var label = new StartLabel(train, departure, [], []);

        Assert.AreEqual(destination, label.Destination);
    }
}
