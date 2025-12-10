namespace PotoDocs.API.Options;

public class OrganizationSettings
{
    public required LegalInfo LegalInfo { get; set; }
    public required Bank Bank { get; set; }
}

public class LegalInfo
{
    public required string Name { get; set; }
    public required string Address { get; set; }
    public required string NIP { get; set; }
    public required string PlaceOfIssue { get; set; }
}

public class Bank
{
    public required string BankName { get; set; }
    public string AccountPLN { get; set; } = string.Empty;
    public string AccountEUR { get; set; } = string.Empty;
    public string IBAN { get; set; } = string.Empty;
    public string SWIFT { get; set; } = string.Empty;
}