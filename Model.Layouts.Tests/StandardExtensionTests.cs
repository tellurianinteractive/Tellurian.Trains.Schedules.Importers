using Tellurian.Trains.Schedules.Model.Layouts.Catalogues;
using Tellurian.Trains.Schedules.Model.Layouts.EndProfiles;

namespace Tellurian.Trains.Schedules.Model.Layouts.Tests;

/// <summary>
/// A standard that extends another (<see cref="ModuleStandard.ExtendsStandardId"/>) has the end profiles of the standards it extends,
/// directly or through others, as in the Module Registry, where e.g. MMM-2R extends MMM and MMM-2R/SSM extends MMM-2R.
/// </summary>
[TestClass]
public sealed class StandardExtensionTests
{
    private static readonly Guid MmmId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid Mmm2RId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly Guid SummerId = Guid.Parse("00000000-0000-0000-0000-000000000003");
    private static readonly Guid FremoId = Guid.Parse("00000000-0000-0000-0000-000000000004");

    private static ModuleStandard Standard(Guid id, string shortName, Guid? extends = null) =>
        new() { Id = id, ShortName = shortName, Scale = new() { ShortName = "H0", Denominator = 87 }, Gauges = [new(16.5)], ExtendsStandardId = extends };

    private static EndProfile Profile(string designation, ModuleStandard standard) =>
        new() { Id = Guid.NewGuid(), Designation = designation, Scale = "H0", Standards = [new(standard.Id!.Value, standard.ShortName)] };

    private static readonly ModuleStandard Mmm = Standard(MmmId, "MMM");
    private static readonly ModuleStandard Mmm2R = Standard(Mmm2RId, "MMM-2R", MmmId);
    private static readonly ModuleStandard Summer = Standard(SummerId, "MMM-2R/SSM", Mmm2RId);
    private static readonly ModuleStandard Fremo = Standard(FremoId, "FREMO/H0-RE");
    private static readonly EndProfile Mmm480 = Profile("MMM480", Mmm);
    private static readonly EndProfile Summer1 = Profile("SSM1", Summer);
    private static readonly EndProfile B96 = Profile("B96", Fremo);

    private static readonly Catalogue Catalogue = new()
    {
        Scales = [new() { ShortName = "H0", Denominator = 87 }],
        Standards = [Mmm, Mmm2R, Summer, Fremo],
        EndProfiles = [Mmm480, Summer1, B96],
    };

    [TestMethod]
    public void AnExtensionHasTheEndProfilesOfTheStandardsItExtends()
    {
        CollectionAssert.AreEquivalent(new[] { Mmm480 }, Catalogue.EndProfilesOf(Mmm2RId).ToArray());
        CollectionAssert.AreEquivalent(new[] { Mmm480, Summer1 }, Catalogue.EndProfilesOf(SummerId).ToArray(), "Through MMM-2R to MMM.");
    }

    [TestMethod]
    public void AStandardDoesNotHaveTheEndProfilesOfItsExtensions()
    {
        CollectionAssert.AreEquivalent(new[] { Mmm480 }, Catalogue.EndProfilesOf(MmmId).ToArray());
        CollectionAssert.AreEquivalent(new[] { B96 }, Catalogue.EndProfilesOf(FremoId).ToArray());
    }

    [TestMethod]
    public void TheChainIsTheStandardAndThoseItExtendsNearestFirst()
    {
        CollectionAssert.AreEqual(new[] { "MMM-2R/SSM", "MMM-2R", "MMM" }, Catalogue.StandardAndExtended(SummerId).Select(s => s.ShortName).ToArray());
        Assert.IsEmpty(Catalogue.StandardAndExtended(Guid.NewGuid()));
    }

    [TestMethod]
    public void ALoopOrAnUnknownExtendedStandardEndsTheChain()
    {
        var first = Standard(Guid.NewGuid(), "First");
        var second = Standard(Guid.NewGuid(), "Second", first.Id);
        first.ExtendsStandardId = second.Id;
        var orphan = Standard(Guid.NewGuid(), "Orphan", Guid.NewGuid());
        var catalogue = Catalogue with { Standards = [.. Catalogue.Standards, first, second, orphan], EndProfiles = [.. Catalogue.EndProfiles, Profile("Own", orphan)] };

        CollectionAssert.AreEqual(new[] { "First", "Second" }, catalogue.StandardAndExtended(first.Id!.Value).Select(s => s.ShortName).ToArray());
        Assert.AreEqual("Own", catalogue.EndProfilesOf(orphan.Id!.Value).Single().Designation);
    }

    [TestMethod]
    public void TheEndProfilesOfAStandardTheCatalogueLacksAreThoseThatNameIt()
    {
        var catalogue = Catalogue with { Standards = [Fremo] };

        CollectionAssert.AreEquivalent(new[] { Mmm480 }, catalogue.EndProfilesOf(MmmId).ToArray());
    }

    [TestMethod]
    public void ADefaultEndProfileMayBeOfAnExtendedStandard()
    {
        var extension = Mmm2R.With(s => s.DefaultSingleTrackEndProfileId = Mmm480.Id);
        var other = Fremo.With(s => s.DefaultSingleTrackEndProfileId = Mmm480.Id);
        var newExtension = Standard(Guid.Empty, "MMM-3R", MmmId).With(s => { s.Id = null; s.DefaultSingleTrackEndProfileId = Mmm480.Id; });

        Assert.IsEmpty(CatalogueValidation.Validate(extension, Catalogue));
        Assert.IsEmpty(CatalogueValidation.Validate(newExtension, Catalogue), "A new standard without id yet.");
        Assert.AreEqual(CatalogueProblemCode.EndProfileNotOfStandard, CatalogueValidation.Validate(other, Catalogue).Single().Code);
    }

    [TestMethod]
    public void ACopyHasTheExtendedStandard()
    {
        Assert.AreEqual(MmmId, Mmm2R.Copy().ExtendsStandardId);
    }
}