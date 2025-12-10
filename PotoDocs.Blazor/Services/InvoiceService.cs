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
    Task<FileDownloadResult> Download(Guid id);
}

public class InvoiceService(IAuthService authService) : BaseService(authService), IInvoiceService
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
    public async Task<FileDownloadResult> Download(Guid id)
    {
        return await GetFileAsync($"api/invoice/{id}/pdf");
    }
}