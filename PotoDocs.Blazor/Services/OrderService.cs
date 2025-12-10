using PotoDocs.Blazor.Dialogs;
using PotoDocs.Shared.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PotoDocs.Blazor.Services;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAll();
    Task<OrderDto> GetById(Guid id);
    Task Delete(Guid id);
    Task<OrderDto> Create(OrderDto order, IEnumerable<FileUploadDto> orderfiles, IEnumerable<FileUploadDto> cmrfiles);
    Task Update(OrderDto order, IEnumerable<FileUploadDto> newOrderFiles, IEnumerable<FileUploadDto> newCmrFiles, IEnumerable<Guid> fileIdsToDelete);
    Task<FileDownloadResult> DownloadFile(Guid id);
    Task<OrderDto> ParseOrder(FileUploadDto file);
    Task<OrderDto> ParseExistingOrder(Guid id);
    Task SendDocuments(Guid id);
    Task MarkOrdersAsSentAsync(List<Guid> orderIds, DateTime dateSent);
}

public class OrderService(IAuthService authService) : BaseService(authService), IOrderService
{
    public async Task<IEnumerable<OrderDto>> GetAll()
    {
        return await GetAsync<IEnumerable<OrderDto>>("api/orders/all");
    }

    public async Task<OrderDto> GetById(Guid id)
    {
        return await GetAsync<OrderDto>($"api/orders/{id}");
    }

    public async Task Delete(Guid id)
    {
        await DeleteAsync($"api/orders/{id}");
    }

    public async Task<OrderDto> Create(OrderDto order, IEnumerable<FileUploadDto> orderfiles, IEnumerable<FileUploadDto> cmrfiles)
    {
        using var multipartFormContent = new MultipartFormDataContent();

        var orderJson = JsonSerializer.Serialize(order);
        multipartFormContent.Add(new StringContent(orderJson), "orderDtoJson");

        foreach (var file in orderfiles)
        {
            var fileContent = new ByteArrayContent(file.Data);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            multipartFormContent.Add(fileContent, "orderFiles", file.Name);
        }

        foreach (var file in cmrfiles)
        {
            var fileContent = new ByteArrayContent(file.Data);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            multipartFormContent.Add(fileContent, "cmrFiles", file.Name);
        }

        return await PostMultipartAsync<OrderDto>("api/orders", multipartFormContent);
    }

    public async Task<OrderDto> ParseOrder(FileUploadDto file)
    {
        using var multipartFormContent = new MultipartFormDataContent();

        var fileContent = new ByteArrayContent(file.Data);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        multipartFormContent.Add(fileContent, "file", file.Name);

        return await PostMultipartAsync<OrderDto>("api/orders/parse", multipartFormContent);
    }

    public async Task<OrderDto> ParseExistingOrder(Guid id)
    {
        return await GetAsync<OrderDto>($"api/orders/parse/{id}");
    }

    public async Task Update(OrderDto order, IEnumerable<FileUploadDto> newOrderFiles, IEnumerable<FileUploadDto> newCmrFiles, IEnumerable<Guid> fileIdsToDelete)
    {
        using var multipartFormContent = new MultipartFormDataContent();

        var orderJson = JsonSerializer.Serialize(order);
        multipartFormContent.Add(new StringContent(orderJson), "orderDtoJson");

        foreach (var file in newOrderFiles)
        {
            var fileContent = new ByteArrayContent(file.Data);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            multipartFormContent.Add(fileContent, "orderFiles", file.Name);
        }

        foreach (var file in newCmrFiles)
        {
            var fileContent = new ByteArrayContent(file.Data);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            multipartFormContent.Add(fileContent, "cmrFiles", file.Name);
        }

        foreach (var id in fileIdsToDelete)
        {
            multipartFormContent.Add(new StringContent(id.ToString()), "fileIdsToDelete");
        }

        await PutMultipartAsync($"api/orders/{order.Id}", multipartFormContent);
    }

    public async Task<FileDownloadResult> DownloadFile(Guid id)
    {
        return await GetFileAsync($"api/orders/files/{id}");
    }

    public async Task SendDocuments(Guid id)
    {
        await PostAsync($"api/orders/{id}/send-documents", null);
    }
    public async Task MarkOrdersAsSentAsync(List<Guid> orderIds, DateTime sentDate)
    {
        var formattedDate = sentDate.ToString("s");
        await PostAsync($"api/orders/mark-as-sent?sentDate={formattedDate}", orderIds);
    }
}