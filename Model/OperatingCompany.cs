namespace Tellurian.Trains.Schedules.Importers.Model;

public record OperatingCompany(int Id, string Name, string Signature, string CountryCode);

public static class OperatingCompanyExtensions
{
    extension(OperatingCompany trainOperator)
    {
        public string DisplayName =>
             $"{trainOperator.Name} ({trainOperator.Signature}-{trainOperator.CountryCode})";


        public static OperatingCompany None =>
            new(0, "", "", "");
    }
}
