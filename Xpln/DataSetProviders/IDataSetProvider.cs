using System.Data;

namespace Tellurian.Trains.Timetables.Importers.Xpln.DataSetProviders;
public interface IDataSetProvider
{
    DataSet? ImportSchedule(Stream stream, DataSetConfiguration configiration);
}
