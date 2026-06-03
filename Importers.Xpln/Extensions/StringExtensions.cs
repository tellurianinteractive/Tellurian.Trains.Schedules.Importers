using System.Globalization;
using System.Text.RegularExpressions;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Xpln.Extensions
{
    /// <summary>
    /// Extension methods for string manipulation specific to XPLN data import.
    /// Provides utilities for parsing train identifiers, company signatures, and time values.
    /// </summary>
    public static partial class StringExtensions
    {
        [GeneratedRegex("\\d+")]
        private static partial Regex TrainCategoryRegex();

        extension(string value)
        {
            /// <summary>
            /// Extracts the operating company signature from a locomotive identifier.
            /// The signature is the portion before the first delimiter (underscore, hyphen, period, or space).
            /// </summary>
            /// <example>
            /// "DB_218" returns "DB", "SJ-Rc6" returns "SJ".
            /// </example>
            public string LocoOperatingCompanySignature =>
                value.UntilOrEmpty(['_', '-', '.', ' ']);

            /// <summary>
            /// Extracts the vehicle number from a locomotive or trainset identifier as the trailing run of digits.
            /// Unlike <c>NumberOrZero</c>, this handles identifiers where the number follows a non-letter separator.
            /// </summary>
            /// <example>
            /// "DB_GLok2" returns 2, "GT CL5814" returns 5814, "X2000" returns 2000; an identifier without
            /// trailing digits (e.g. "DB_GLok") returns 0.
            /// </example>
            public int LocoNumber =>
                int.TryParse(
                    new string([.. value.Reverse().TakeWhile(char.IsDigit).Reverse()]),
                    NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) ? number : 0;

            /// <summary>
            /// Extracts the train category prefix from a train identifier.
            /// The prefix is the alphabetic portion after the period and before any numbers or spaces.
            /// </summary>
            /// <example>
            /// "1.IC123" returns "IC", "2.RE 456" returns "RE".
            /// </example>
            public string TrainCategoryPrefix
            {
                get
                {
                    var start = value.IndexOf('.') + 1;
                    var length = 0;
                    var end = value.IndexOf(' ');
                    if (end >= 0)
                        length = end - start;
                    else
                    {
                        var re = TrainCategoryRegex();
                        Match m = re.Match(value[start..]);
                        if (m.Success) length = m.Index;
                    }
                    return value.Substring(start, length);
                }
            }

            /// <summary>
            /// Parses the string value as a time.
            /// Supports multiple formats: TimeSpan ("HH:mm"), DateTime, or fractional days (0.0 to 1.0).
            /// </summary>
            /// <returns>A <see cref="Time"/> value representing the parsed time.</returns>
            /// <exception cref="FormatException">Thrown when the value cannot be parsed as a valid time.</exception>
            public Time AsTime() =>
                TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var timespan) ? Time.FromTimeSpan(timespan) :
                DateTime.TryParse(value, CultureInfo.InvariantCulture, out var dateTime) ? Time.FromTimeSpan(dateTime.TimeOfDay) :
                Time.FromDays(double.Parse(value.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture));
        }

        extension(string? value)
        {
            /// <summary>
            /// Determines whether the string represents a valid time value.
            /// </summary>
            /// <param name="isZeroInvalid">If true, a value of "0" is considered invalid.</param>
            /// <returns>
            /// <c>true</c> if the value can be parsed as a TimeSpan, DateTime, or fractional day (0.0 to 1.0);
            /// otherwise, <c>false</c>.
            /// </returns>
            public bool IsTime(bool isZeroInvalid = false) =>
                (isZeroInvalid && value.HasValueExcept("0") || !isZeroInvalid && value.HasValue) &&
                    (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var _) ||
                    DateTime.TryParse(value, CultureInfo.InvariantCulture, out var _) ||
                    double.TryParse(value.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out var t) && t >= 0.0 && t <= 1.0);

            /// <summary>
            /// Determines whether the string represents a valid track number.
            /// </summary>
            /// <returns><c>true</c> if the value is a valid number; otherwise, <c>false</c>.</returns>
            public bool IsTrackNumber() =>
                value.IsNumber;
        }
    }
}
