namespace PotoDocs.Blazor.Services.Strategies;

public class InvoiceStrategyFactory(IEnumerable<IInvoiceActionStrategy> strategies)
{
    private readonly IEnumerable<IInvoiceActionStrategy> _strategies = strategies;

    public IInvoiceActionStrategy GetStrategy(InvoiceType type)
    {
        return _strategies.FirstOrDefault(s => s.Type == type)
               ?? throw new NotImplementedException($"Brak strategii dla typu {type}");
    }
}
