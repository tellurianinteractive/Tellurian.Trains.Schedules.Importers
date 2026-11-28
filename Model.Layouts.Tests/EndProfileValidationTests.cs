using Tellurian.Trains.Schedules.Model.Layouts.Drawings;
using Tellurian.Trains.Schedules.Model.Layouts.EndProfiles;

namespace Tellurian.Trains.Schedules.Model.Layouts.Tests;

/// <summary>Tracks and screw holes of an end profile may not overlap, except a dual-gauge track sharing a rail.</summary>
[TestClass]
public sealed class EndProfileValidationTests
{
    private const double Standard = 16.5;
    private const double Narrow = 12;
    private const double HoleDiameter = 6;

    private static EndProfile Profile(IEnumerable<EndProfileTrack>? tracks = null, IEnumerable<Circle>? holes = null) => new()
    {
        Id = Guid.NewGuid(),
        Designation = "Test",
        Scale = "H0",
        Tracks = [.. tracks ?? []],
        ScrewHoles = [.. holes ?? []],
    };

    private static EndProfileTrack Track(double offset, double gauge = Standard) => new() { Offset = offset, Gauges = [gauge] };

    private static Circle Hole(double x, double y) => new(new(x, y), HoleDiameter);

    [TestMethod]
    public void ADoubleTrackProfileWithHolesBelowHasNoProblems()
    {
        var profile = Profile([Track(-22.5), Track(22.5)], [Hole(-100, -40), Hole(0, -40), Hole(100, -40)]);

        Assert.IsEmpty(EndProfileValidation.Validate(profile));
    }

    [TestMethod]
    public void TracksWhoseSleepersOverlapAreAProblem()
    {
        var profile = Profile([Track(0), Track(20)]);

        Assert.AreEqual(new EndProfileProblem(EndProfileProblemCode.TracksOverlap, 0, 1), EndProfileValidation.Validate(profile).Single());
    }

    [TestMethod]
    public void TracksThatOnlyTouchAreNoProblem()
    {
        var profile = Profile([Track(0), Track(TrackCrossSection.SleeperLength(Standard))]);

        Assert.IsEmpty(EndProfileValidation.Validate(profile));
    }

    [TestMethod]
    [DataRow(-1, DisplayName = "Left rail common")]
    [DataRow(1, DisplayName = "Right rail common")]
    public void ANarrowGaugeTrackSharingARailWithAStandardGaugeTrackIsNoProblem(int side)
    {
        var narrowOffset = side * (Standard - Narrow) / 2;
        var profile = Profile([Track(0), Track(narrowOffset, Narrow)]);

        Assert.IsEmpty(EndProfileValidation.Validate(profile));
    }

    [TestMethod]
    public void ANarrowGaugeTrackCentredInAStandardGaugeTrackIsAProblem()
    {
        var profile = Profile([Track(0), Track(0, Narrow)]);

        Assert.AreEqual(EndProfileProblemCode.TracksOverlap, EndProfileValidation.Validate(profile).Single().Code);
    }

    [TestMethod]
    public void TwoTracksOfTheSameGaugeAtTheSamePlaceAreAProblem()
    {
        var profile = Profile([Track(0), Track(0)]);

        Assert.AreEqual(EndProfileProblemCode.TracksOverlap, EndProfileValidation.Validate(profile).Single().Code);
    }

    [TestMethod]
    public void OverlappingScrewHolesAreAProblem()
    {
        var profile = Profile(holes: [Hole(0, -40), Hole(HoleDiameter / 2, -40), Hole(50, -40)]);

        Assert.AreEqual(new EndProfileProblem(EndProfileProblemCode.ScrewHolesOverlap, 0, 1), EndProfileValidation.Validate(profile).Single());
    }

    [TestMethod]
    public void ScrewHolesThatOnlyTouchAreNoProblem()
    {
        var profile = Profile(holes: [Hole(0, -40), Hole(HoleDiameter, -40)]);

        Assert.IsEmpty(EndProfileValidation.Validate(profile));
    }

    [TestMethod]
    public void AScrewHoleOverlappingATrackIsAProblem()
    {
        var profile = Profile([Track(0)], [Hole(0, -TrackCrossSection.DefaultHeight - HoleDiameter / 4)]);

        Assert.AreEqual(new EndProfileProblem(EndProfileProblemCode.ScrewHoleOverlapsTrack, 0, 0), EndProfileValidation.Validate(profile).Single());
    }

    [TestMethod]
    public void AScrewHoleJustBelowATrackIsNoProblem()
    {
        var profile = Profile([Track(0)], [Hole(0, -TrackCrossSection.DefaultHeight - HoleDiameter / 2)]);

        Assert.IsEmpty(EndProfileValidation.Validate(profile));
    }
}
