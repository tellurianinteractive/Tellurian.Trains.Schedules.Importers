using System.Globalization;
using System.Text.RegularExpressions;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Xpln.Extensions
{
    public static partial class StringExtensions
    {
        [GeneratedRegex("\\d+")]
        private static partial Regex TrainCategoryRegex();

        extension(string value)
        {
            public string LocoOperatingCompanySignature =>
                value.UntilOrEmpty(['_', '-', '.', ' ']);

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

            public Time AsTime() =>
                TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var timespan) ? Time.FromTimeSpan(timespan) :
                DateTime.TryParse(value, CultureInfo.InvariantCulture, out var dateTime) ? Time.FromTimeSpan(dateTime.TimeOfDay) :
                Time.FromDays(double.Parse(value.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture));
        }

        extension(string? value)
        {
            public bool IsTime(bool isZeroInvalid = false) =>
                (isZeroInvalid && value.HasValueExcept("0") || !isZeroInvalid && value.HasValue) &&
                    (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var _) ||
                    DateTime.TryParse(value, CultureInfo.InvariantCulture, out var _) ||
                    double.TryParse(value.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out var t) && t >= 0.0 && t <= 1.0);

            public bool IsTrackNumber() =>
                value.IsNumber;
        }

        extension(string? filename)
        {
            //public bool HasFileExtension(params string[] extensions) =>
            //    filename is not null &&
            //    extensions.Any(e => Path.GetExtension(filename).Equals(e, StringComparison.OrdinalIgnoreCase));
        }
    }
}
