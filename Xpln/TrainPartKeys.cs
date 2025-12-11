using Tellurian.Trains.Schedules.Importers.Model;

namespace Tellurian.Trains.Schedules.Importers.Xpln;

public sealed partial class XplnDataImporter
{
    internal record TrainPartKeys(Maybe<StationCall> FromCall, Maybe<StationCall> ToCall, IEnumerable<Message> Messages);
}
