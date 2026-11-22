namespace Tellurian.Trains.Schedules.Importers.Interfaces.Tests;

[TestClass]
public sealed class ImporterExtensionsTests
{
    private sealed class TextImporter(string extension, string start) : IImporter<string>
    {
        public string ResourceKey => start;
        public IReadOnlyList<string> FileExtensions { get; } = [extension];
        public bool CanImport(string fileName, ReadOnlySpan<byte> content) => content.StartsWith(System.Text.Encoding.UTF8.GetBytes(start));
        public Task<ImportResult<string>> ImportAsync(Stream stream, string fileName, CancellationToken cancellationToken = default) =>
            Task.FromResult(ImportResult<string>.Success(start));
    }

    private static readonly IImporter<string>[] Importers = [new TextImporter(".txt", "A"), new TextImporter(".txt", "B"), new TextImporter(".dat", "A")];

    [TestMethod]
    public void TheFirstImporterOfTheExtensionThatRecognisesTheContentIsChosen()
    {
        Assert.AreEqual("B", Importers.For("file.TXT", "B..."u8)!.ResourceKey);
        Assert.AreSame(Importers[0], Importers.For("file.txt", "A..."u8));
        Assert.AreSame(Importers[2], Importers.For("file.dat", "A..."u8));
        Assert.IsNull(Importers.For("file.txt", "C..."u8));
        Assert.IsNull(Importers.For("file.csv", "A..."u8));
    }

    [TestMethod]
    public void ReadableExtensionsAreListedOnce()
    {
        CollectionAssert.AreEqual(new[] { ".txt", ".dat" }, Importers.ReadableFileExtensions().ToArray());
    }
}
