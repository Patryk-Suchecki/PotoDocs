using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PotoDocs.API.Entities;
using PotoDocs.API.Models;
using PotoDocs.Shared.Models;
using System.Formats.Tar;
using System.IO.Compression;
using System.Net;

namespace PotoDocs.API.Services;

public interface IOrderService
{
    ApiResponse<IEnumerable<OrderDto>> GetAll(int page = 1, int pageSize = 5, string? driverEmail = null);
    ApiResponse<OrderDto> GetById(Guid id);
    void Delete(Guid id);
    void Update(Guid id, OrderDto dto);
    Task<ApiResponse<OrderDto>> CreateFromPdf(IFormFile file);
    Task<ApiResponse<OrderDto>> AddCmr(List<IFormFile> files, Guid id);
    void DeleteCmr(string fileName);
    Task<byte[]> GetPdf(Guid id);
    Task<byte[]> GetZip(int year, int month);
    Task<Dictionary<int, List<int>>> GetAvailableYearsAndMonthsAsync();
    void DeleteFile(string filePath);
}

public class OrderService : IOrderService
{
    private readonly PotodocsDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IOpenAIService _openAIService;
    private readonly IInvoiceService _invoiceService;

    public OrderService(PotodocsDbContext dbContext, IMapper mapper, IOpenAIService openAIService, IInvoiceService invoiceService)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _openAIService = openAIService;
        _invoiceService = invoiceService;
    }

    public ApiResponse<IEnumerable<OrderDto>> GetAll(int page = 1, int pageSize = 5, string? driverEmail = null)
    {
        var query = _dbContext.Orders
            .Include(o => o.Driver)
            .Include(o => o.CMRFiles)
            .Include(o => o.Company)
            .Include(o => o.Stops)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(driverEmail))
        {
            query = query.Where(o => o.Driver != null && o.Driver.Email == driverEmail);
        }

        query = query.Where(o => o.IssueDate.HasValue)
                     .OrderByDescending(o => o.IssueDate.Value.Year)
                     .ThenByDescending(o => o.IssueDate.Value.Month)
                     .ThenByDescending(o => o.InvoiceNumber);

        var orders = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var ordersDto = _mapper.Map<List<OrderDto>>(orders);

        return ApiResponse<IEnumerable<OrderDto>>.Success(ordersDto);
    }


    public ApiResponse<OrderDto> GetById(Guid id)
    {
        var order = _dbContext.Orders.Include(o => o.Driver)
                                     .Include(c => c.CMRFiles)
                                     .Include(o => o.Company)
                                     .Include(o => o.Stops)
                                     .FirstOrDefault(o => o.Id == id);
        if (order == null) return ApiResponse<OrderDto>.Failure("Nie znaleziono zlecenia.", HttpStatusCode.BadRequest);

        return ApiResponse<OrderDto>.Success(_mapper.Map<OrderDto>(order));
    }

    public void Delete(Guid id)
    {
        var order = _dbContext.Orders
            .Include(o => o.CMRFiles)
            .FirstOrDefault(o => o.Id == id);

        if (order == null)
        {
            Console.WriteLine($"Zamówienie o ID {id} nie istnieje.");
            return;
        }

        DeleteFile(order.PDFUrl);

        if (order.CMRFiles != null)
        {
            foreach (var cmr in order.CMRFiles)
            {
                DeleteFile(cmr.Url);
            }
        }

        _dbContext.Orders.Remove(order);
        _dbContext.SaveChanges();
    }

    public void DeleteFile(string filePath)
    {
        if (!string.IsNullOrWhiteSpace(filePath))
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/pdfs", filePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }

    public void Update(Guid id, OrderDto dto)
    {
        var order = _dbContext.Orders
            .Include(o => o.Driver)
            .Include(o => o.Company)
            .Include(o => o.Stops)
            .Include(o => o.CMRFiles)
            .FirstOrDefault(o => o.Id == id);

        if (order == null) return;

        _mapper.Map(dto, order);

        if (dto.Driver?.Email != null)
        {
            order.Driver = _dbContext.Users.SingleOrDefault(u => u.Email == dto.Driver.Email);
        }

        _dbContext.SaveChanges();
    }


    public async Task<ApiResponse<OrderDto>> CreateFromPdf(IFormFile file)
    {
        if (file == null || file.Length == 0 || file.ContentType != "application/pdf")
        {
            return ApiResponse<OrderDto>.Failure("Plik jest nieprawidłowy lub ma niepoprawny format", HttpStatusCode.BadRequest);
        }

        var uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdfs");
        if (!Directory.Exists(uploadsFolderPath))
        {
            Directory.CreateDirectory(uploadsFolderPath);
        }

        var fileName = GetUniqueFileName(uploadsFolderPath, file.FileName);
        var filePath = Path.Combine(uploadsFolderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var orderDto = await _openAIService.GetInfoFromText(file);

        var lastUnloadingStop = orderDto.Stops
            .Where(stop => stop.Type == StopType.Unloading)
            .OrderByDescending(stop => stop.Date)
            .FirstOrDefault();

        var issueDate = lastUnloadingStop?.Date ?? DateTime.Now;
        orderDto.IssueDate = issueDate;

        int lastInvoiceNumber = _dbContext.Orders
            .Where(o => o.IssueDate.HasValue &&
                        o.IssueDate.Value.Year == issueDate.Year &&
                        o.IssueDate.Value.Month == issueDate.Month)
            .OrderByDescending(o => o.InvoiceNumber)
            .Select(o => o.InvoiceNumber ?? 0)
            .FirstOrDefault();

        orderDto.InvoiceNumber = lastInvoiceNumber + 1;
        orderDto.PDFUrl = fileName;

        var order = _mapper.Map<Order>(orderDto);
        _dbContext.Orders.Add(order);
        _dbContext.SaveChanges();

        return GetById(order.Id);
    }


    public async Task<ApiResponse<OrderDto>> AddCmr(List<IFormFile> files, Guid id)
    {
        if (files == null || files.Count == 0)
            return ApiResponse<OrderDto>.Failure("Nie przesłano pliku", HttpStatusCode.BadRequest);

        var order = _dbContext.Orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
            return ApiResponse<OrderDto>.Failure("Nie znaleziono zlecenia.", HttpStatusCode.BadRequest);

        var uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdfs");
        if (!Directory.Exists(uploadsFolderPath))
        {
            Directory.CreateDirectory(uploadsFolderPath);
        }

        var cmrFileUrls = new List<string>();

        foreach (var file in files)
        {
            var uniqueFileName = GetUniqueFileName(uploadsFolderPath, file.FileName);
            var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = Path.Combine("/pdfs", uniqueFileName);
            cmrFileUrls.Add(relativePath);

            var cmrFile = new CMRFile
            {
                Url = uniqueFileName,
                OrderId = order.Id,
                Order = order
            };
            _dbContext.CMRFiles.Add(cmrFile);
            _dbContext.SaveChanges();
        }

        return GetById(id);
    }


    public void DeleteCmr(string fileName)
    {
        var cmrFile = _dbContext.CMRFiles.FirstOrDefault(c => c.Url == fileName);
        if (cmrFile == null) return;

        _dbContext.CMRFiles.Remove(cmrFile);
        _dbContext.SaveChanges();

        DeleteFile(cmrFile.Url);
    }
    public async Task<byte[]> GetPdf(Guid id)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Company)
            .Include(o => o.Driver)
            .Include(o => o.Stops)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return null;

        return await _invoiceService.GenerateInvoicePdf(order);
    }
    public async Task<byte[]> GetZip(int year, int month)
    {
        var orders = await _dbContext.Orders
            .Where(o => o.IssueDate.Value.Month == month && o.IssueDate.Value.Year == year)
            .ToListAsync();

        if (orders == null || orders.Count == 0) return null;

        string tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDirectory);

        try
        {
            foreach (var order in orders)
            {
                var pdfData = await _invoiceService.GenerateInvoicePdf(order);

                string fileName = $"FAKTURA {order.InvoiceNumber}-{order.IssueDate:MM-yyyy}.pdf";
                string filePath = Path.Combine(tempDirectory, fileName);

                await File.WriteAllBytesAsync(filePath, pdfData);
            }

            string zipFilePath = Path.Combine(Path.GetTempPath(), $"Faktury {month:D2}-{year}.zip");

            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }

            ZipFile.CreateFromDirectory(tempDirectory, zipFilePath);

            return await File.ReadAllBytesAsync(zipFilePath);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }
    }
    public async Task<Dictionary<int, List<int>>> GetAvailableYearsAndMonthsAsync()
    {
        var data = await _dbContext.Orders
            .Where(o => o.IssueDate.HasValue)
            .Select(o => new { o.IssueDate!.Value.Year, o.IssueDate!.Value.Month })
            .ToListAsync();

        return data
            .GroupBy(o => o.Year)
            .OrderByDescending(g => g.Key)
            .ToDictionary(
                g => g.Key,
                g => g.Select(o => o.Month).Distinct().OrderBy(m => m).ToList()
            );
    }
    private string GetUniqueFileName(string directory, string originalFileName)
    {
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        string extension = Path.GetExtension(originalFileName);
        string uniqueFileName = originalFileName;
        int counter = 1;

        while (File.Exists(Path.Combine(directory, uniqueFileName)))
        {
            uniqueFileName = $"{fileNameWithoutExtension}_{counter}{extension}";
            counter++;
        }

        return uniqueFileName;
    }

}
