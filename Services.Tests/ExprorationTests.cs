using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Tellurian.Trains.Schedules.Importers.Model;

namespace Services.Tests;

[TestClass]
public sealed class ExprorationTests
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    [TestMethod] //, Ignore("This thest converted UIC data in CompanyCodes_20231117_1049.txt to JSON/CSV")]
    public async Task ConvertOperatingCompaniesToSimplerFormat()
    {
        List<OperatingCompany> _operatingCompanies = new(2000);
        var encoding = Encoding.GetEncoding("iso-8859-1");
        const string fileName = "..\\..\\..\\CompanyCodes_20231117_1049.txt";
        using var streamReader = new StreamReader(fileName, encoding, true);
        string? line;
        using var streamWriter = new StreamWriter("OperatingCompanies.csv", false, encoding);
        var lineCount = 0;

        string? number = null;
        string? signature = null;
        string? name = null;
        string? country = null;
        while ((line = streamReader.ReadLine()) != null)
        {
            if (LineStartsWithFourDigits(line))
            {
                number = line[..4];
                lineCount = 1;
            }
            else if (lineCount == 1)
            {
                signature = ValueOfMaxLengthOrNull(line, 10);
                lineCount++;
            }
            else if (lineCount == 2)
            {
                name = line.Trim();
                lineCount++;
            }
            else if (lineCount == 3)
            {
                var x = CountryName(line);
                country = CountryCode(x);
                lineCount++;
            }
            else if (lineCount == 4)
            {
                if (number.HasText() && signature.HasText() && name.HasText() && country.HasText())
                {
                    _operatingCompanies.Add(new(number.NumberOrZero, name, signature, country));
                }
                else
                {
                    Debugger.Break();
                }
                lineCount++;
            }
        }

        foreach (var oc in _operatingCompanies.OrderBy(oc => oc.CountryCode).ThenBy(oc => signature))
        {
            streamWriter.WriteLine(
                $""" 
                "{oc.Id:0000}"; "{oc.CountryCode}; "{oc.Signature}"; "{oc.Name}"" 
                """);

        }
        await using var jsonStream = File.Create("OperatingCompanies.json");
        await JsonSerializer.SerializeAsync<IEnumerable<OperatingCompany>>(jsonStream, _operatingCompanies, _jsonSerializerOptions, TestContext.CancellationToken);

        static string? ValueOfMaxLengthOrNull(string line, int maxLength)
        {
            var x = line.Trim().UntilOrEmpty(" ");
            var y = x.Length <= maxLength ? x.ToUpperInvariant() : line[..maxLength].ToUpperInvariant();
            if (y.All(c => char.IsLetter(c) || char.IsNumber(c) || c == '-')) return y;
            return null;
        }

        static bool LineStartsWithFourDigits(string line) =>
            line.Length >= 4 && line[..4].All(c => char.IsDigit(c));

        static string CountryName(string line) => line switch
        {
            "Bosnia and Herzegovina, Serb Republic of" => "Bosnia and Herzegovina",
            "Congo, the Democratic Republic of the" => "Congo",
            "Iran, Islamic Republic of" => "Iran",
            "Korea, Democratic People's Republic of" => "South Korea",
            "Korea, Republic of" => "North Korea",
            "Macedonia, The former Yugoslav Republic of" => "Macedonia",
            "Molødova" => "Moldova",
            "Moldova, Republic of" => "Molødova",
            "Russian Federation" => "Russia",
            "United Kingdom of Great Britain and Northern Ireland" => "United Kingdom",
            "Viet nam" => "Vietnam",
            _ => line.Trim(),
        };

        static string? CountryCode(string countryName) => countryName switch
        {
            "Austria" => "AT",
            "Belgium" => "BE",
            "Czech Republic" => "CZ",
            "Denmark" => "DK",
            "Finland" => "FI",
            "France" => "FR",
            "Germany" => "DE",
            "Hungary" => "HU",
            "Italy" => "IT",
            "Luxemburg" => "LU",
            "Netherlands" => "NL",
            "Norway" => "NO",
            "Poland" => "PL",
            "Portugal" => "PT",
            "Romanina" => "RO",
            "Slovakia" => "SK",
            "Slovenia" => "SI",
            "Spain" => "ES",
            "Sweden" => "SE",
            "Switzerland" => "CH",
            "United Kingdom" => "UK",

            _ => null, // Skip this country
        };
    }

    public TestContext TestContext { get; set; }
}
