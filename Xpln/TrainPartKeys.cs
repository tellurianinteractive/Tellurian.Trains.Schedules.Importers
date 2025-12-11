using Tellurian.Trains.Timetables.Importers.Model;

namespace Tellurian.Trains.Timetables.Importers.Xpln;

public sealed partial class XplnDataImporter
{
    internal record TrainPartKeys(Maybe<StationCall> FromCall, Maybe<StationCall> ToCall, IEnumerable<Message> Messages);
}
