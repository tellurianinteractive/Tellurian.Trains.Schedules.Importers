namespace Tellurian.Trains.Schedules.Importers.Services.Tests;

[TestClass]
public class TrainCategoriesServiceTests
{
    [TestMethod]
    public async Task ReadsCsvFile()
    {
        var target = new TrainCategoriesFromCsvService();
        var actual = await target.GetAllTrainCategoriesAsync();
        Assert.IsNotNull(actual);
        Assert.HasCount(14, actual);
    }
}
