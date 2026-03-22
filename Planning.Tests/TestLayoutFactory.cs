using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Planning.Timetables;

namespace Tellurian.Trains.Schedules.Planning.Tests;

/// <summary>
/// Creates a test layout based on an exported model railway network.
/// <code>
///                                           Bjørkåsen
///                                               │
/// Lysekil ── Göransberg ── Furudalen             Rise ─── Ørmen ── Steinsnes
///                              │                /
///                          Kristineberg     Froland
///                           /       \          \
///                   Kbg västra    Munkeröd    Gården ── Mohult ── Loudden ── Växjö
///                       │           / \        /            \
///                    Sura by  Ryr t.  Kkö ── Kys            Krutträsk ── Bergsgården ── Wilma ── Hammarlind
///                             │       │
///                         Tosselilla  Devsjö ── Lekby ── Rotebro ── Malmö
/// </code>
/// </summary>
internal static class TestLayoutFactory
{
    /// <summary>
    /// Creates a simple linear layout for basic testing:
    /// <code>
    /// Malmö (M) ──10m── Lund (Lu) ──8m── Eslöv (E) ──12m── Hässleholm (Hm)
    ///                        │
    ///                       6km
    ///                        │
    ///                    Kävlinge (Kä)
    /// </code>
    /// </summary>
    public static Layout CreateSimpleLayout()
    {
        var layout = new Layout { Name = "Södra stambanan" };

        var malmö = AddStation(layout, 101, "Malmö", "M2", ["1", "2", "3", "4"]);
        var lund = AddStation(layout, 102, "Lund", "Lu", ["1", "2", "3"]);
        var eslöv = AddStation(layout, 103, "Eslöv", "E", ["1", "2"]);
        var hässleholm = AddStation(layout, 104, "Hässleholm", "Hm", ["1", "2", "3"]);
        var kävlinge = AddStation(layout, 105, "Kävlinge", "Kä", ["1", "2"]);

        layout.Add(new TrackStretch(101, malmö, lund, 10, 2));
        layout.Add(new TrackStretch(102, lund, eslöv, 8, 2));
        layout.Add(new TrackStretch(103, eslöv, hässleholm, 12, 1));
        layout.Add(new TrackStretch(104, lund, kävlinge, 6, 1));

        var mainLine = new TimetableStretch(101, "S1", "Södra stambanan");
        mainLine.AddLast(Stretch(layout, "M2", "Lu"));
        mainLine.AddLast(Stretch(layout, "Lu", "E"));
        mainLine.AddLast(Stretch(layout, "E", "Hm"));
        layout.Add(mainLine);

        var branch = new TimetableStretch(102, "S2", "Lund–Kävlinge");
        branch.AddLast(Stretch(layout, "Lu", "Kä"));
        layout.Add(branch);

        return layout;
    }

    /// <summary>
    /// Creates the full test layout based on an exported model railway network.
    /// </summary>
    public static Layout CreateLayout()
    {
        var layout = new Layout { Name = "Testbanan" };

        // Manned stations (appear as from/to in dispatch stretches)
        var lysekil = AddStation(layout, 1, "Lysekil", "Lys", ["1", "2"]);
        var göransberg = AddStation(layout, 2, "Göransberg", "Gbg", ["1", "2"]);
        var furudalen = AddStation(layout, 3, "Furudalen", "Fda", ["1", "2"]);
        var tosselilla = AddStation(layout, 4, "Tosselilla", "Tsl", ["1", "2"]);
        var munkeröd = AddStation(layout, 5, "Munkeröd", "Mkd", ["1", "2", "3", "4"], canChangeDirection: true);
        var devsjö = AddStation(layout, 6, "Devsjö", "Djö", ["1", "2"]);
        var lekby = AddStation(layout, 7, "Lekby", "Lek", ["1", "2"]);
        var rotebro = AddStation(layout, 8, "Rotebro", "R", ["1", "2", "3"]);
        var malmö = AddStation(layout, 9, "Malmö", "M", ["1", "2", "3", "4"]);
        var mohult = AddStation(layout, 10, "Mohult", "Mot", ["1", "2"], canChangeDirection: true);
        var loudden = AddStation(layout, 11, "Loudden", "Lou", ["1", "2"]);
        var växjö = AddStation(layout, 12, "Växjö", "Vö", ["1", "2", "3"]);
        var rise = AddStation(layout, 13, "Rise", "Ris", ["1", "2"], canChangeDirection: true);
        var froland = AddStation(layout, 14, "Froland", "Fro", ["1", "2"]);
        var bjørkåsen = AddStation(layout, 15, "Bjørkåsen", "Bjå", ["1", "2"]);
        var steinsnes = AddStation(layout, 16, "Steinsnes", "Sns", ["1", "2"]);

        // Unmanned locations along dispatch stretches (signal-controlled junctions/passing points)
        var kristineberg = AddSignalControlled(layout, 17, "Kristineberg", "Kbg", ["1", "2"]);
        var ryrTimmer = AddSignalControlled(layout, 18, "Ryr timmer", "Ryt", ["1"]);
        var kyrkebyÖstra = AddSignalControlled(layout, 19, "Kyrkeby Östra", "Kkö", ["1", "2"]);
        var kyrkebyStrand = AddOther(layout, 20, "Kyrkeby Strand", "Kys", ["1"]);
        var gården = AddSignalControlled(layout, 21, "Gården", "Gdn", ["1", "2"]);
        var ørmen = AddSignalControlled(layout, 22, "Ørmen", "Ømn", ["1"]);

        // Unmanned end-of-line locations (other)
        var kristinebergVästra = AddOther(layout, 23, "Kristineberg västra", "Kbv", ["1"]);
        var suraBy = AddOther(layout, 24, "Sura by", "Sub", ["1"]);
        var krutträsk = AddOther(layout, 25, "Krutträsk", "Krt", ["1"]);
        var bergsgården = AddOther(layout, 26, "Bergsgården", "Bgd", ["1"]);
        var wilmaLp = AddOther(layout, 27, "Wilma lp", "Wil", ["1"]);
        var hammarlind = AddOther(layout, 28, "Hammarlind", "Hld", ["1"]);

        // Track stretches (from exported data)
        layout.Add(new TrackStretch(1, lysekil, göransberg, 4, 1));
        layout.Add(new TrackStretch(2, göransberg, furudalen, 9, 1));
        layout.Add(new TrackStretch(3, tosselilla, ryrTimmer, 5, 1));
        layout.Add(new TrackStretch(4, furudalen, kristineberg, 8, 1));
        layout.Add(new TrackStretch(5, kristineberg, kristinebergVästra, 5, 1));
        layout.Add(new TrackStretch(6, kristinebergVästra, suraBy, 3, 1));
        layout.Add(new TrackStretch(7, ryrTimmer, munkeröd, 17, 1));
        layout.Add(new TrackStretch(8, kristineberg, munkeröd, 9, 1));
        layout.Add(new TrackStretch(9, munkeröd, kyrkebyÖstra, 7, 2));
        layout.Add(new TrackStretch(10, kyrkebyÖstra, devsjö, 12, 2));
        layout.Add(new TrackStretch(11, devsjö, lekby, 7, 2));
        layout.Add(new TrackStretch(12, lekby, rotebro, 14, 2));
        layout.Add(new TrackStretch(13, rotebro, malmö, 8, 2));
        layout.Add(new TrackStretch(14, kyrkebyÖstra, kyrkebyStrand, 2, 1));
        layout.Add(new TrackStretch(15, kyrkebyStrand, gården, 3, 1));
        layout.Add(new TrackStretch(16, gården, mohult, 11, 1));
        layout.Add(new TrackStretch(17, mohult, loudden, 8, 1));
        layout.Add(new TrackStretch(18, loudden, växjö, 6, 1));
        layout.Add(new TrackStretch(19, mohult, krutträsk, 7, 1));
        layout.Add(new TrackStretch(20, krutträsk, bergsgården, 2, 1));
        layout.Add(new TrackStretch(21, bergsgården, wilmaLp, 5, 1));
        layout.Add(new TrackStretch(22, wilmaLp, hammarlind, 2, 1));
        layout.Add(new TrackStretch(23, bjørkåsen, rise, 8, 1));
        layout.Add(new TrackStretch(24, rise, froland, 7, 1));
        layout.Add(new TrackStretch(25, rise, ørmen, 7, 1));
        layout.Add(new TrackStretch(26, ørmen, steinsnes, 9, 1));
        layout.Add(new TrackStretch(27, froland, gården, 4, 1));

        // Timetable stretches (logical railway lines)
        // Some track stretches appear on multiple timetable stretches.

        var huvudbanan = new TimetableStretch(1, "1", "Lysekil–Malmö");
        huvudbanan.AddLast(Stretch(layout, "Lys", "Gbg"));
        huvudbanan.AddLast(Stretch(layout, "Gbg", "Fda"));
        huvudbanan.AddLast(Stretch(layout, "Fda", "Kbg"));
        huvudbanan.AddLast(Stretch(layout, "Kbg", "Mkd"));
        huvudbanan.AddLast(Stretch(layout, "Mkd", "Kkö"));
        huvudbanan.AddLast(Stretch(layout, "Kkö", "Djö"));
        huvudbanan.AddLast(Stretch(layout, "Djö", "Lek"));
        huvudbanan.AddLast(Stretch(layout, "Lek", "R"));
        huvudbanan.AddLast(Stretch(layout, "R", "M"));
        layout.Add(huvudbanan);

        var tosselillabanan = new TimetableStretch(2, "2", "Tosselilla–Munkeröd");
        tosselillabanan.AddLast(Stretch(layout, "Tsl", "Ryt"));
        tosselillabanan.AddLast(Stretch(layout, "Ryt", "Mkd"));
        layout.Add(tosselillabanan);

        var kustBanan = new TimetableStretch(3, "3", "Munkeröd–Växjö");
        kustBanan.AddLast(Stretch(layout, "Mkd", "Kkö"));
        kustBanan.AddLast(Stretch(layout, "Kkö", "Kys"));
        kustBanan.AddLast(Stretch(layout, "Kys", "Gdn"));
        kustBanan.AddLast(Stretch(layout, "Gdn", "Mot"));
        kustBanan.AddLast(Stretch(layout, "Mot", "Lou"));
        kustBanan.AddLast(Stretch(layout, "Lou", "Vö"));
        layout.Add(kustBanan);

        var norgebanan = new TimetableStretch(4, "4", "Bjørkåsen–Mohult");
        norgebanan.AddLast(Stretch(layout, "Bjå", "Ris"));
        norgebanan.AddLast(Stretch(layout, "Ris", "Fro"));
        norgebanan.AddLast(Stretch(layout, "Fro", "Gdn"));
        norgebanan.AddLast(Stretch(layout, "Gdn", "Mot"));
        layout.Add(norgebanan);

        var steinsnesbanan = new TimetableStretch(5, "5", "Bjørkåsen–Steinsnes");
        steinsnesbanan.AddLast(Stretch(layout, "Bjå", "Ris"));
        steinsnesbanan.AddLast(Stretch(layout, "Ris", "Ømn"));
        steinsnesbanan.AddLast(Stretch(layout, "Ømn", "Sns"));
        layout.Add(steinsnesbanan);

        return layout;
    }

    public static Schedule CreateSchedule()
    {
        var layout = CreateLayout();
        var timetable = new Timetable("Vinter 2026", layout);
        return new Schedule("Test", timetable);
    }

    public static OperationsPlanner CreatePlanner() =>
        new(CreateSchedule(), TimetableSettings.Default);

    private static Station AddStation(Layout layout, int id, string name, string signature, string[] trackNumbers, bool canChangeDirection = false)
    {
        var station = new Station(id, name, signature) { IsChangingTrainDirectionPossible = canChangeDirection };
        AddTracks(station, trackNumbers);
        layout.Add(station);
        return station;
    }

    private static SignalControlledLocation AddSignalControlled(Layout layout, int id, string name, string signature, string[] trackNumbers)
    {
        var location = new SignalControlledLocation(id, name, signature);
        AddTracks(location, trackNumbers);
        layout.Add(location);
        return location;
    }

    private static OtherLocation AddOther(Layout layout, int id, string name, string signature, string[] trackNumbers)
    {
        var location = new OtherLocation(id, name, signature);
        AddTracks(location, trackNumbers);
        layout.Add(location);
        return location;
    }

    private static void AddTracks(OperationLocation location, string[] trackNumbers)
    {
        for (var i = 0; i < trackNumbers.Length; i++)
        {
            location.Add(new StationTrack(location.Id * 10 + i + 1, trackNumbers[i]));
        }
    }

    private static TrackStretch Stretch(Layout layout, string fromSignature, string toSignature) =>
        layout.TrackStretches.First(ts =>
            ts.Start.Signature == fromSignature && ts.End.Signature == toSignature);
}
