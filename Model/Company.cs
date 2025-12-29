namespace Tellurian.Trains.Schedules.Model;

public record Company(int Id, string Name, string Signature, string CountryCode);

public static class CompanyExtensions
{
    extension(Company company)
    {
        public string DisplayName =>
             $"{company.Name} ({company.Signature}-{company.CountryCode})";


        public static Company None =>
            new(0, "", "", "");

        public static Company FromSignature(string signature) =>
            new(0, signature, signature, "");
    }
}
