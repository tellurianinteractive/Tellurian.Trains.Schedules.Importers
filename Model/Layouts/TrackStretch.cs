using System.Globalization;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Resources;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// Provides extension methods for <see cref="TrackStretch"/>.
/// </summary>
public static class TrackStretchExtensions
{
    extension(TrackStretch stretch)
    {
        /// <summary>
        /// All train passings on this track stretch. Derived from the trains calling at both ends, so it is
        /// computed on demand and never stored.
        /// </summary>
        public IEnumerable<StretchPassing> Passings => [.. stretch.GetStretchPassings()];

        /// <summary>The stretch in the current language, e.g. "Tellby to Ytterby".</summary>
        public string Description => string.Format(CultureInfo.CurrentCulture, Strings.StretchToString, stretch.Start, stretch.End);
    }

    /// <summary>
    /// Gets all train passings on the track stretch: a passing is a train running from one end to the
    /// other, so the calls are read in the order the train runs them (<c>Train.CallsInRunOrder</c>) rather
    /// than the order they were added.
    /// </summary>
    /// <param name="me">The track stretch.</param>
    /// <returns>A collection of stretch passings.</returns>
    internal static IEnumerable<StretchPassing> GetStretchPassings(this TrackStretch me)
    {
        var trains = me.Start.Trains().Intersect(me.End.Trains());
        var result = new List<StretchPassing>(trains.Count());
        foreach (var train in trains)
        {
            var calls = train.CallsInRunOrder;
            for (int i = 0; i < calls.Count - 1; i++)
            {
                if (calls[i].OperationLocation.Equals(me.Start) && calls[i + 1].OperationLocation.Equals(me.End)) result.Add(new StretchPassing(train, calls[i], calls[i + 1]));
                if (calls[i].OperationLocation.Equals(me.End) && calls[i + 1].OperationLocation.Equals(me.Start)) result.Add(new StretchPassing(train, calls[i], calls[i + 1]));
            }
        }
        return result;
    }
}
