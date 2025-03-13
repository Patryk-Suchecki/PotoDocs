using System.ComponentModel.DataAnnotations;

namespace PotoDocs.Shared.Models;

public class OrderStopDto
{
    public StopType Type { get; set; }
    public DateTime Date { get; set; }
    [StringLength(200, ErrorMessage = "Adres rozładunku nie może mieć więcej niż 200 znaków.")]
    public string Address { get; set; }
}
public enum StopType
{
    Loading,
    Unloading
}