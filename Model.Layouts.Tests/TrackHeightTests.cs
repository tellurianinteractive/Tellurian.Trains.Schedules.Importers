using Tellurian.Trains.Schedules.Model.Layouts.EndProfiles;

namespace Tellurian.Trains.Schedules.Model.Layouts.Tests;

/// <summary>
/// Track heights: each track's own or the end profile's, 4 mm when neither states one. Rail top stays at Y = 0, so the
/// track bed, the plate's top under a track, is at minus the height.
/// </summary>
[TestClass]
public sealed class TrackHeightTests
{
    private const double Gauge = 16.5;
    private const double HalfWidth = 200;
    private const double Bottom = -108;

    private static EndProfileTrack Track(double offset, double? height = null) => new() { Offset = offset, Gauges = [Gauge], Height = height };

    /// <summary>A plate with a level top at <paramref name="top"/> from side to side, as a template makes it.</summary>
    private static PlateShape LevelPlate(double top) => new([new(-HalfWidth, top), new(HalfWidth, top)], Bottom);

    private static EndProfile Profile(PlateShape plate, double? trackHeight, params EndProfileTrack[] tracks) => new()
    {
        Id = Guid.NewGuid(),
        Designation = "Test",
        Scale = "H0",
        Plate = plate,
        Outline = plate.Outline(),
        TrackHeight = trackHeight,
        Tracks = [.. tracks],
    };

    [TestMethod]
    public void AProfileWithoutATrackHeightHasTheDefault()
    {
        var profile = Profile(LevelPlate(-4), null, Track(0));

        Assert.AreEqual(TrackCrossSection.DefaultHeight, profile.DefaultTrackHeight, Precision);
        Assert.AreEqual(TrackCrossSection.DefaultHeight, profile.TrackHeightOf(profile.Tracks[0]), Precision);
    }

    [TestMethod]
    public void ATracksOwnHeightComesBeforeTheProfiles()
    {
        var profile = Profile(LevelPlate(-5), 5, Track(-50), Track(50, 3));

        Assert.AreEqual(5, profile.TrackHeightOf(profile.Tracks[0]), Precision);
        Assert.AreEqual(3, profile.TrackHeightOf(profile.Tracks[1]), Precision);
        Assert.AreEqual(-3, profile.TrackBounds(profile.Tracks[1]).Min.Y, Precision);
        Assert.AreEqual(0, profile.TrackBounds(profile.Tracks[1]).Max.Y, Precision);
    }

    [TestMethod]
    public void ATrackOnItsTrackBedIsNoProblem()
    {
        Assert.IsEmpty(EndProfileValidation.Validate(Profile(LevelPlate(-4), null, Track(0))));
    }

    [TestMethod]
    public void ATrackHigherOrLowerThanThePlateIsAProblem()
    {
        var problems = EndProfileValidation.Validate(Profile(LevelPlate(-4), 5, Track(0)));

        Assert.AreEqual(new EndProfileProblem(EndProfileProblemCode.TrackNotOnPlate, 0, 0), problems.Single());
    }

    [TestMethod]
    public void AnOutlineDrawnAroundTheTrackUpToRailTopIsNoProblem()
    {
        var aroundTheTrack = new PlateShape([new(-HalfWidth, -20), new(-24, -4), new(-20, 0), new(20, 0), new(24, -4), new(HalfWidth, -20)], Bottom);

        Assert.IsEmpty(EndProfileValidation.Validate(Profile(aroundTheTrack, 5, Track(0))));
    }

    [TestMethod]
    public void APlateBelowTheTrackBedIsAProblem()
    {
        Assert.AreEqual(EndProfileProblemCode.TrackNotOnPlate, EndProfileValidation.Validate(Profile(LevelPlate(-10), null, Track(0))).Single().Code);
    }

    [TestMethod]
    public void TheWholeLevelSegmentUnderATrackMoves()
    {
        var plate = LevelPlate(-4);

        var moved = plate.WithTrackBed(0, -4, -5, FitTolerances.Default.Joint)!;

        Assert.AreEqual(-5, moved.UpperEdge[0].Y, Precision);
        Assert.AreEqual(-5, moved.UpperEdge[^1].Y, Precision);
        Assert.AreEqual(Bottom, moved.Bottom, Precision);
    }

    [TestMethod]
    public void OnlyTheSegmentUnderTheTrackMovesTheRestOfTheEdgeStays()
    {
        // Landscape at -20 on both sides, the track bed at -4 between x -40 and 40, with slopes down to it.
        var plate = new PlateShape([new(-HalfWidth, -20), new(-60, -20), new(-40, -4), new(40, -4), new(60, -20), new(HalfWidth, -20)], Bottom);

        var moved = plate.WithTrackBed(10, -4, -6, FitTolerances.Default.Joint)!;

        Assert.HasCount(plate.UpperEdge.Count, moved.UpperEdge);
        Assert.AreEqual(new Geometry.Point(-40, -6), moved.UpperEdge[2]);
        Assert.AreEqual(new Geometry.Point(40, -6), moved.UpperEdge[3]);
        Assert.AreEqual(new Geometry.Point(-60, -20), moved.UpperEdge[1]);
        Assert.AreEqual(new Geometry.Point(60, -20), moved.UpperEdge[4]);
    }

    [TestMethod]
    public void AnEdgeThatIsNotAtTheTrackBedUnderTheTrackIsNotMoved()
    {
        Assert.IsNull(LevelPlate(-10).WithTrackBed(0, -4, -5, FitTolerances.Default.Joint));
    }

    [TestMethod]
    public void TheTemplateStatesTheTrackHeight()
    {
        var profile = EndProfileTemplate.Create(new() { Designation = "T", Scale = "H0", PlateWidth = 400, PlateTop = -5, PlateBottom = -109, Gauge = Gauge, TrackHeight = 5 });

        Assert.AreEqual(5, profile.TrackHeight);
        Assert.IsEmpty(EndProfileValidation.Validate(profile));
    }
}
