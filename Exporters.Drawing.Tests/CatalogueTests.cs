using System.Text.Json.Nodes;
using Tellurian.Trains.Schedules.Exporters.Interfaces;
using Tellurian.Trains.Schedules.Importers.Drawing;
using Tellurian.Trains.Schedules.Model.Files;
using Tellurian.Trains.Schedules.Model.Layouts;
using Tellurian.Trains.Schedules.Model.Layouts.Catalogues;

namespace Tellurian.Trains.Schedules.Exporters.Drawing.Tests;

/// <summary>Module Draw's catalogue read in the earlier shape and written in today's (decisions 11 to 13).</summary>
[TestClass]
public sealed class CatalogueTests
{
    private static string BuiltIn => File.ReadAllText(Path.Combine("TestData", "builtin.catalogue.json"));

    private static Catalogue Read(string json)
    {
        var result = new CatalogueImporter().Import(json);
        Assert.IsTrue(result.IsSuccess, string.Join(", ", result.Messages.Select(m => m.Text)));
        return result.Item;
    }

    [TestMethod]
    public void StandardsGetTheirGaugesAsAListAndTheirScaleAsTheSharedScale()
    {
        Assert.Contains("\"normalGauge\"", BuiltIn, "The test needs the earlier shape.");

        var catalogue = Read(BuiltIn);

        var fremo = catalogue.FindStandard("FREMO/H0-RE")!;
        Assert.AreEqual(16.5, fremo.NormalGauge);
        Assert.AreEqual("H0", fremo.Scale!.ShortName);
        Assert.IsNotNull(fremo.Id);
        Assert.IsTrue(catalogue.Standards.All(s => s.Gauges is { Count: > 0 } && s.Scale is not null));
        Assert.IsTrue(catalogue.EndProfiles.SelectMany(p => p.Tracks).All(t => t.Gauges.Count > 0));
    }

    [TestMethod]
    public void AWrittenCatalogueHasTodaysShapeAndReadsBackTheSame()
    {
        var exporter = new CatalogueExporter();
        var written = exporter.Write(Read(BuiltIn), ExportOptions.Default);

        Assert.DoesNotContain("\"normalGauge\"", written);
        Assert.DoesNotContain("\"gauge\":", written);
        var standard = JsonNode.Parse(written)!["content"]!["standards"]![0]!;
        Assert.AreEqual(System.Text.Json.JsonValueKind.Object, JsonValueKind(standard["scale"]));
        Assert.AreEqual(written, exporter.Write(Read(written), ExportOptions.Default), "Today's shape reads and writes unchanged.");
    }

    [TestMethod]
    public void TheExtendedStandardIsWrittenAndReadBack()
    {
        var catalogue = Read(BuiltIn);
        var (extension, extended) = (catalogue.Standards[1], catalogue.Standards[0]);
        catalogue = catalogue.With(extension.With(s => s.ExtendsStandardId = extended.Id));

        var written = new CatalogueExporter().Write(catalogue, ExportOptions.Default);

        Assert.AreEqual(extended.Id, Read(written).FindStandard(extension.Id!.Value)!.ExtendsStandardId);
        Assert.Contains("\"extendsStandardId\"", written);
        Assert.DoesNotContain("\"extendsStandardId\"", new CatalogueExporter().Write(Read(BuiltIn), ExportOptions.Default), "Not written when it extends none.");
    }

    [TestMethod]
    public void TheRegistryIdOfAStandardMovesToTheExternalReferences()
    {
        var file = JsonNode.Parse(BuiltIn)!.AsObject();
        file["content"]!["standards"]![0]!["registryId"] = 7;

        var standard = Read(file.ToJsonString()).Standards[0];

        Assert.AreEqual(7, standard.ExternalReferences!["moduleRegistry"].GetProperty("standardId").GetInt32());
        Assert.IsNull(standard.ExtensionData);
    }

    [TestMethod]
    public void AUserEntryReplacesTheServedOneWithTheSameId()
    {
        var served = Read(BuiltIn);
        var fremo = served.FindStandard("FREMO/H0-RE")!;
        var user = Catalogue.Empty.With(fremo.With(s => s.RailTopHeight = 1250));

        var merged = Catalogue.Merge(served, user);

        Assert.AreEqual(1250, merged.FindStandard(fremo.Id!.Value)!.RailTopHeight);
        Assert.HasCount(served.Standards.Count, merged.Standards);
    }

    [TestMethod]
    public void OnlyCatalogueFilesCanBeImported()
    {
        var importer = new CatalogueImporter();
        Assert.IsTrue(importer.CanImport("x.json", System.Text.Encoding.UTF8.GetBytes(BuiltIn)));
        Assert.IsFalse(importer.CanImport("x.json", """{ "format": "tellurian-drawing", "kind": "EndProfile" }"""u8));
        Assert.AreEqual(FileErrorCode.WrongKind, importer.Import("""{ "format": "tellurian-drawing", "kind": "Hall", "schemaVersion": 2, "content": {} }""").Messages.Single().Code);
    }

    private static System.Text.Json.JsonValueKind JsonValueKind(JsonNode? node) => node?.GetValueKind() ?? System.Text.Json.JsonValueKind.Null;

    [TestMethod]
    public void StandardsWithoutAnIdDontReplaceEachOther()
    {
        var served = Catalogue.Empty.With(new ModuleStandard { ShortName = "Served" });
        var user = Catalogue.Empty.With(new ModuleStandard { ShortName = "Mine" }).With(new ModuleStandard { ShortName = "Also mine" });

        var merged = Catalogue.Merge(served, user);

        CollectionAssert.AreEquivalent(new[] { "Served", "Mine", "Also mine" }, merged.Standards.Select(s => s.ShortName).ToArray());
    }
}
