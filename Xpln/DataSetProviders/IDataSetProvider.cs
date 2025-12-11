using System.Data;

namespace Tellurian.Trains.Schedules.Importers.Xpln.DataSetProviders;
public interface IDataSetProvider
{
    DataSet? ImportSchedule(Stream stream, DataSetConfiguration configiration);
}
