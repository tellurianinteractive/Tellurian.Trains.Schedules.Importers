using System.Globalization;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Xpln.Extensions;

internal static class XplnTrainExtensions
{
    extension(Train train)
    {
        public (Maybe<StationCall> call, int index) FindBetweenArrivalAndDeparture(string stationSignature, Time time, int rowNumber)
        {
            if (train.TryFindCall(stationSignature, rowNumber, (c) => true, out (Maybe<StationCall> call, int index) result1))
                return result1;
            if (train.TryFindCall(stationSignature, rowNumber, (c) => c.Arrival == time, out (Maybe<StationCall> call, int index) result4))
                return result4;
            else if (train.TryFindCall(stationSignature, rowNumber, (c) => c.Departure == time, out (Maybe<StationCall> call, int index) result5))
            {
                return result5;
            }
            if (train.TryFindCall(stationSignature, rowNumber, (c) => time >= train.Calls.Last().Arrival && c.Equals(train.Calls.Last()), out (Maybe<StationCall> call, int index) result2))
                return result2;
            if (train.TryFindCall(stationSignature, rowNumber, (c) => time <= train.Calls.First().Departure && c.Equals(train.Calls.First()), out (Maybe<StationCall> call, int index) result3))
                return result3;
            else
            {
                train.TryFindCall(stationSignature, rowNumber, (c) => time > c.Arrival && time < c.Departure, out (Maybe<StationCall> call, int index) result6);
                return result6;
            }
        }
        private bool TryFindCall(string stationSignature, int rowNumber, Func<StationCall, bool> compare, out (Maybe<StationCall> call, int index) result)
        {
            var calls = train.Calls.Select((call, index) => (call, index))
                .Where(item => item.call.OperationLocation.Signature.Equals(stationSignature, StringComparison.OrdinalIgnoreCase) && compare(item.call))
                .ToArray();
            if (calls.Length == 1)
            {
                result = (new Maybe<StationCall>(calls.First().call), calls.First().index);
                return true;
            }
            else if (calls.Length == 0)
            {
                result = (new Maybe<StationCall>(string.Format(CultureInfo.CurrentCulture, Resources.Strings.TrainHasNoCallsAtStation, rowNumber, train, stationSignature)), -1);
                return false;
            }
            else
            {
                result = (new Maybe<StationCall>(string.Format(CultureInfo.CurrentCulture, Resources.Strings.TrainHasOverlappingTimesAtStation, rowNumber, train, stationSignature)), -1);
                return false;
            }
        }
    }
}
