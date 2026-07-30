using System.Globalization;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Xpln.Extensions;

internal static class XplnTrainExtensions
{
    extension(Train train)
    {
        /// <summary>
        /// Fixes a train that has only one call by duplicating it as arrival and departure.
        /// </summary>
        /// <returns>The train with fixed calls.</returns>
        public Train WithFixedSingleCallTrain()
        {
            if (train.Calls.Count == 1)
            {
                var departure = train.Calls[0];
                departure.Track.Calls.Remove(departure);
                var arrival = new StationCall(departure.Id, departure.Track, departure.Arrival, departure.Arrival);
                departure = new StationCall(departure.Id + 1, departure.Track, departure.Departure, departure.Departure);
                train.Calls.Clear();
                train.Add(arrival);
                train.Add(departure);
            }
            return train;
        }

        /// <summary>
        /// Ensures the origin and terminus have a visible dwell so they remain proper stops.
        /// </summary>
        /// <remarks>
        /// The first call of a train is always its departure (origin) and the last call is always its arrival
        /// (terminus), but XPLN sometimes gives these calls equal arrival and departure times. Equal times
        /// otherwise mark a pass-through (see <see cref="WithPassthroughCalls"/>), so to keep the origin and
        /// terminus as stops a synthetic dwell is added: when the first call's times are equal its arrival is
        /// moved 10 minutes earlier, and when the last call's times are equal its departure is moved 10 minutes
        /// later. Run this before <see cref="WithPassthroughCalls"/> so those calls are no longer equal-times
        /// and are not cleared to pass-throughs.
        /// </remarks>
        /// <returns>The train with origin/terminus dwells ensured.</returns>
        public Train WithOriginAndTerminusDwell()
        {
            const int dwellMinutes = 10;
            var first = train.Calls.First();
            if (first.Arrival.Equals(first.Departure))
                first.Arrival = first.Departure.AddMinutes(-dwellMinutes);
            var last = train.Calls.Last();
            if (last.Arrival.Equals(last.Departure))
                last.Departure = last.Arrival.AddMinutes(dwellMinutes);
            return train;
        }

        /// <summary>
        /// Sets the first call as departure only and the last call as arrival only.
        /// </summary>
        /// <remarks>
        /// This is an XPLN import convention: the first call is where the train originates (departure only)
        /// and the last call is where it terminates (arrival only). It does not look at the times.
        /// </remarks>
        /// <returns>The train with adjusted call flags.</returns>
        public Train WithFirstCallDepartureOnlyAndLastCallArrivalOnly()
        {
            train.Calls.First().IsArrival = false;
            train.Calls.Last().IsDeparture = false;
            return train;
        }

        /// <summary>
        /// Marks every intermediate call where the train does not stop as a pass-through.
        /// </summary>
        /// <remarks>
        /// This is an XPLN import convention used to derive the stop flags from the times: a call whose
        /// arrival equals its departure means the train passes without stopping, so both
        /// <see cref="StationCall.IsArrival"/> and <see cref="StationCall.IsDeparture"/> are cleared and
        /// <see cref="StationCall.IsStop"/> becomes <c>false</c>. The origin and terminus are exempt: they are
        /// always a departure and an arrival respectively, and <see cref="WithOriginAndTerminusDwell"/> has
        /// already given them a dwell so their times are no longer equal. After import, consumers detect a
        /// pass-through from <see cref="StationCall.IsStop"/> alone and never re-compare the times.
        /// </remarks>
        /// <returns>The train with pass-through calls marked.</returns>
        public Train WithPassthroughCalls()
        {
            foreach (var call in train.Calls)
            {
                if (call.Arrival.Equals(call.Departure))
                {
                    call.IsArrival = false;
                    call.IsDeparture = false;
                }
            }
            return train;
        }

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
