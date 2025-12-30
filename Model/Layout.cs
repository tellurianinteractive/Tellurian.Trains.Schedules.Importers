using System.Globalization;
using Tellurian.Trains.Schedules.Model.Resources;

namespace Tellurian.Trains.Schedules.Model;

public sealed class Layout : IEquatable<Layout>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Company> Companies { get; set; }
    public ICollection<OperationLocation> Stations { get; set; }
    public ICollection<TrackStretch> TrackStretches { get; set; }
    public ICollection<TimetableStretch> TimetableStretches { get; set; }

    public Layout()
    {
        Companies = [];
        Stations = [];
        TrackStretches = [];
        TimetableStretches = [];
    }

    public bool Equals(Layout? other) => other is not null && Id == other.Id;
    public override bool Equals(object? obj) => obj is Layout other && Equals(other);
    public override int GetHashCode() => Id.GetHashCode();
    public override string ToString() => Name;
}

public static class LayoutCompanyExtensions
{
    public static bool HasCompany(this Layout? layout, Company company) => layout?.Companies.Any(c => c.Equals(company)) ?? false;
    public static bool HasCompany(this Layout? layout, string signature) => layout?.Companies.Any(c => c.Signature.Equals(signature, StringComparison.OrdinalIgnoreCase)) ?? false;

    public static Maybe<Company> Company(this Layout me, string signature) =>
        new(me?.Companies.SingleOrDefault(c => c.Signature.Equals(signature, StringComparison.OrdinalIgnoreCase)),
            $"Company with signature '{signature}' not found.");

    public static Company Add(this Layout layout, Company company)
    {
        layout = layout.ValueOrException(nameof(layout));
        company = company.ValueOrException(nameof(company));
        if (!layout.HasCompany(company))
        {
            company.Layout = layout;
            company.LayoutId = layout.Id;
            layout.Companies.Add(company);
        }
        return company;
    }
}

public static class LayoutStationsExtensions
{
    public static bool HasStation(this Layout me, OperationLocation station) => me?.Stations.Any(s => s.Equals(station)) ?? false;
    public static bool HasTrack(this Layout me, StationTrack track) => me?.StationTracks().Any(t => t.Equals(track)) ?? false;



    public static Maybe<OperationLocation> Station(this Layout me, string nameOrSignature) =>
       new(me?.Stations.SingleOrDefault(s => s.Signature.Equals(nameOrSignature, StringComparison.OrdinalIgnoreCase) || s.Name.Equals(nameOrSignature, StringComparison.OrdinalIgnoreCase)),
           Strings.ThereIsNoStationWithNameOrSignature, nameOrSignature);

    public static IEnumerable<StationTrack> StationTracks(this Layout me) => me is null ? [] : me.Stations.SelectMany(s => s.Tracks);

    public static OperationLocation Add(this Layout layout, OperationLocation station)
    {
        layout = layout.ValueOrException(nameof(layout));
        station = station.ValueOrException(nameof(station));
        if (!layout.HasStation(station))
        {
            station.Layout = layout;
            station.LayoutId = layout.Id;
            layout.Stations.Add(station);
        }
        return station;
    }

    public static TrackStretch Add(this Layout layout, TrackStretch stretch)
    {
        layout = layout.ValueOrException(nameof(layout));
        stretch = stretch.ValueOrException(nameof(stretch));
        if (!layout.TrackStretches.Contains(stretch))
        {
            layout.TrackStretches.Add(stretch);
        }
        return stretch;
    }

    public static TrackStretch Add(this Layout layout, int id, string fromStationName, string toStationName, double distance, int tracksCount)
    {
        var fromStation = layout.Stations.Single(s => s.Name == fromStationName);
        var toStation = layout.Stations.Single(s => s.Name == toStationName);
        var trackStretch = new TrackStretch(id, fromStation, toStation, distance, tracksCount);
        layout.Add(trackStretch);
        return trackStretch;
    }

}

public static class LayoutExtensions
{
    public static bool HasTimetableStretch(this Layout me, string number) => me is not null && me.TimetableStretches.Any(ts => ts.Number.Equals(number, StringComparison.OrdinalIgnoreCase));
    public static Maybe<TimetableStretch> TimetableStretch(this Layout me, string number)
    {
        me = me.ValueOrException(nameof(me));
        return new Maybe<TimetableStretch>(me.TimetableStretches.SingleOrDefault(ts => ts.Number.Equals(number, StringComparison.OrdinalIgnoreCase)));
    }
    public static TimetableStretch Add(this Layout layout, TimetableStretch timetableStretch)
    {
        layout = layout.ValueOrException(nameof(layout));
        timetableStretch = timetableStretch.ValueOrException(nameof(timetableStretch));
        ArgumentNullException.ThrowIfNull(timetableStretch);
        if (!layout.TimetableStretches.Contains(timetableStretch))
        {
            layout.TimetableStretches.Add(timetableStretch);
        }
        return timetableStretch;
    }
}

public static class LayoutTracksExtensions
{
    public static Maybe<TrackStretch> Add(this Layout layout, int id, string fromStationName, string toStationName, double distance, int tracksCount, int speed, int time)
    {
        var from = layout.Station(fromStationName);
        var to = layout.Station(toStationName);
        if (from.HasValue && to.HasValue)
            return new Maybe<TrackStretch>(layout.Add(new TrackStretch(id, from.Value, to.Value, distance, tracksCount, speed, time)));
        return new Maybe<TrackStretch>($"From {from} to {to}");
    }

    public static Maybe<TrackStretch> TrackStretch(this Layout trackLayout, OperationLocation from, OperationLocation to)
        => new(trackLayout?.TrackStretches.SingleOrDefault(ts =>
            (ts.Start.Equals(from) && ts.End.Equals(to)) ||
            (ts.Start.Equals(to) && ts.End.Equals(from))),
            string.Format(CultureInfo.CurrentCulture, Strings.MoreThanOneStretchBetweenStations, from, to));

    public static Maybe<TrackStretch> TrackStretch(this Layout me, string fromStationNameOrSignature, string toStationNameOrSignature)
    {
        me = me.ValueOrException(nameof(me));
        return new Maybe<TrackStretch>(
            me.Between(fromStationNameOrSignature, toStationNameOrSignature).Concat(
            me.Between(toStationNameOrSignature, fromStationNameOrSignature)).SingleOrDefault(),
            string.Format(CultureInfo.CurrentCulture, Strings.ThereIsNoStretchBetweenStation1AndStation2, fromStationNameOrSignature, toStationNameOrSignature));
    }

    private static IEnumerable<TrackStretch> Between(this Layout me, string fromStationNameOrSignature, string? toStationNameOrSignature = null)
        => me.TrackStretches.Where(ts =>
            (ts.Start.Name.EqualsCaseInsensitive(fromStationNameOrSignature)
            || ts.Start.Signature.EqualsCaseInsensitive(fromStationNameOrSignature))
            && (ts.End.Name.EqualsCaseInsensitive(toStationNameOrSignature)
            || ts.End.Signature.EqualsCaseInsensitive(toStationNameOrSignature)));
}
