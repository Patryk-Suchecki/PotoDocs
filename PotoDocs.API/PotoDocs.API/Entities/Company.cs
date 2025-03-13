namespace PotoDocs.API.Entities;
public class Company
{
    public int Id { get; set; }
    public string? NIP { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? CorrespondenceAddress { get; set; }
    public string? Country { get; set; }

    public bool AcceptsEInvoices { get; set; }
    public List<string> EmailAddresses { get; set; } = new();
}
