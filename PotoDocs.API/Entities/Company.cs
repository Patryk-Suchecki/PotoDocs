namespace PotoDocs.API.Entities;
public class Company
{
    public Guid Id { get; set; }
    public required string NIP { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public string? CorrespondenceAddress { get; set; }
    public string Country { get; set; } = string.Empty;

    public bool AcceptsEInvoices { get; set; }
    public string EmailAddress { get; set; } = string.Empty;
}