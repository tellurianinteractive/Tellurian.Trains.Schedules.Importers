using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Model.Tests;

internal static class TestDataFactory
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    public static StationTrack CreateStationTrack()
    {
        var result = StationTrack.Example;
        result.Station = new Station(2, "Ytterby", "Yb");
        return result;
    }

    public static void Init()
    {
        Stations = [CreateStation1(), CreateStation2(), CreateStation3()];
    }

    public static IEnumerable<OperationLocation> Stations;

    internal static OperationLocation CreateStation1()
    {
        var station = new Station(3, "Göteborg", "G");
        station.Add(new StationTrack(1, "1"));
        station.Add(new StationTrack(2, "2"));
        station.Add(new StationTrack(3, "3"));
        station.Add(new StationTrack(4, "4"));
        return station;
    }

    private static OperationLocation CreateStation2()
    {
        var station = new Station(2, "Ytterby", "Yb");
        station.Add(new StationTrack(1, "1"));
        station.Add(new StationTrack(2, "2"));
        return station;
    }

    private static OperationLocation CreateStation3()
    {
        var station = new Station(1, "Stenungsund", "Snu");
        station.Add(new StationTrack(1, "1"));
        station.Add(new StationTrack(2, "2"));
        return station;
    }

    public static IEnumerable<Train> CreateTrains(string prefix, Time startTime)
    {
        var category = new TrainCategory() { Id = 1, Prefix = prefix, Name = prefix };
        return [
            CreateTrainInForwardDirection(category, 1, startTime)
        ];
    }

    public static Train CreateTrainInForwardDirection(TrainCategory category, int number, Time startTime)
    {
        var stations = Stations.ToArray();
        var train = new Train(number, category, number) { Category = category };
        _ = train.Add(new StationCall(1, stations[0]["3"], startTime, startTime));
        _ = train.Add(new StationCall(2, stations[1]["2"], startTime.AddMinutes(25), startTime.AddMinutes(30)));
        _ = train.Add(new StationCall(3, stations[2]["1"], startTime.AddMinutes(55), startTime.AddMinutes(55)));
        return train;
    }

    public static Train CreateTrainInOppositeDirection(TrainCategory category, int number, Time startTime)
    {
        var stations = Stations.ToArray();
        var train = new Train(number, category, number) { Category = category };
        _ = train.Add(new StationCall(1, stations[2]["2"], startTime, startTime));
        _ = train.Add(new StationCall(2, stations[1]["1"], startTime.AddMinutes(25), startTime.AddMinutes(30)));
        _ = train.Add(new StationCall(3, stations[0]["3"], startTime.AddMinutes(55), startTime.AddMinutes(55)));
        return train;
    }

    public static Train CreateTrain1()
    {
        return CreateTrainInForwardDirection(new() { Id = 21, Name = "FreightTrain", Prefix = "G" }, 4321, Time.FromHourAndMinute(12, 00));
    }

    public static Train CreateTrain2()
    {
        return CreateTrainInForwardDirection(new() { Id = 22, Name = "PassengerTrain", Prefix = "G" }, 1234, Time.FromHourAndMinute(12, 00));
    }

    public static Timetable CreateTimetable()
    {
        var timetable = NewTimetable("Test", Layout());
        timetable.Add(CreateTrain1());
        timetable.Add(CreateTrain2());
        return timetable;
    }

    public static Layout Layout()
    {
        var layout = new Layout { Name = "Test" };
        foreach (var s in Stations) layout.Add(s);
        var stations = layout.OperationLocations.ToArray();
        for (var i = 0; i < stations.Length - 1; i++) layout.Add(new TrackStretch(1, stations[i], stations[i + 1], 10));
        var stretch = new TimetableStretch(1, "1");
        foreach (var ts in layout.TrackStretches) stretch.AddLast(ts);
        PendingStretches.GetOrCreateValue(layout).Add(stretch);
        return layout;
    }

    // The timetable stretches made with a layout, until a timetable is made for it: they are the timetable's.
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Layout, List<TimetableStretch>> PendingStretches = [];

    /// <summary>A timetable for <paramref name="layout"/>, with the timetable stretches made with it here.</summary>
    public static Timetable NewTimetable(string name, Layout layout)
    {
        var timetable = new Timetable(name, layout);
        foreach (var stretch in PendingStretches.GetOrCreateValue(layout)) timetable.Add(stretch);
        return timetable;
    }
}
