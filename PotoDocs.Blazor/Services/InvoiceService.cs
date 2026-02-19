using PotoDocs.Shared.Models;

namespace PotoDocs.Blazor.Services;

public interface IInvoiceService
{
    Task<IEnumerable<InvoiceDto>> GetAll();
    Task<InvoiceDto> GetById(Guid id);
    Task Delete(Guid id);
    Task Create(InvoiceDto dto);
    Task<InvoiceDto> CreateFromOrder(Guid id);
    Task Update(InvoiceDto dto);
    Task<HttpResponseMessage> Download(Guid id);
    Task CreateCorrection(InvoiceCorrectionDto dto);
    Task UpdateCorrection(InvoiceCorrectionDto dto);
}

public class InvoiceService(HttpClient http) : BaseService(http), IInvoiceService
{
    public async Task<IEnumerable<InvoiceDto>> GetAll()
    {
        return await GetAsync<IEnumerable<InvoiceDto>>("api/invoice/all");
    }

    public async Task<InvoiceDto> GetById(Guid id)
    {
        return await GetAsync<InvoiceDto>($"api/invoice/{id}");
    }

    public async Task Delete(Guid id)
    {
        await DeleteAsync($"api/invoice/{id}");
    }

    public async Task Create(InvoiceDto dto)
    {
        await PostAsync("api/invoice", dto);
    }
    public async Task<InvoiceDto> CreateFromOrder(Guid id)
    {
        return await PostAsync<InvoiceDto>($"api/invoice/from-order/{id}", null);
    }

    public async Task Update(InvoiceDto dto)
    {
        await PutAsync("api/invoice", dto);
    }
    public async Task<HttpResponseMessage> Download(Guid id)
    {
        return await GetFileResponseAsync($"api/invoice/{id}/pdf");
    }
    public async Task CreateCorrection(InvoiceCorrectionDto dto)
    {
        await PostAsync("api/invoice/correction", dto);
    }
    public async Task UpdateCorrection(InvoiceCorrectionDto dto)
    {
        await PutAsync("api/invoice/correction", dto);
    }
}