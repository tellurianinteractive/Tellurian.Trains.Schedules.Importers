using Microsoft.Extensions.Logging.Abstractions;

namespace Tellurian.Trains.Schedules.Importers.Access.Tests
{
    [TestClass]
    public class ReadDatabaseTests
    {
        [TestMethod]
        public async Task ReadsLayoutStations()
        {
            var file = new FileInfo(Path.Combine("Test data", "Timetable.accdb"));
            var repository = new AccessRepository(file, NullLogger<AccessRepository>.Instance);
            var schedule = await repository.ImportScheduleAsync("Grimslöv H0");
            Assert.IsTrue(schedule.IsSuccess);
            Assert.AreEqual(16, schedule.Item.Timetable.Layout.Stations.Count);
            Assert.AreEqual(62, schedule.Item.Timetable.Layout.Stations.Sum(s => s.Tracks.Count));
        }
    }
}
