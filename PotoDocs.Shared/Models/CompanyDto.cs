using System.ComponentModel.DataAnnotations;

namespace PotoDocs.Shared.Models;

public class CompanyDto
{
    [StringLength(15, ErrorMessage = "NIP firmy nie może mieć więcej niż 15 znaków.")]
    public string? NIP { get; set; }
    [StringLength(100, ErrorMessage = "Nazwa firmy nie może mieć więcej niż 100 znaków.")]
    public string? Name { get; set; }
    [StringLength(200, ErrorMessage = "Adres firmy nie może mieć więcej niż 200 znaków.")]
    public string? Address { get; set; }
    [StringLength(200, ErrorMessage = "Adres korespondencyjny firmy nie może mieć więcej niż 200 znaków.")]
    public string? CorrespondenceAddress { get; set; }
    [StringLength(50, ErrorMessage = "Kraj firmy nie może mieć więcej niż 50 znaków.")]
    public string? Country { get; set; }

    public bool AcceptsEInvoices { get; set; }
    public List<string> EmailAddresses { get; set; } = new();
}