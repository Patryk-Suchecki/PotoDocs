using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PotoDocs.API.Entities;
using PotoDocs.API.Models;
using PotoDocs.Shared.Models;
using System.Net;

namespace PotoDocs.API.Services
{
    public interface IOrderService
    {
        ApiResponse<IEnumerable<OrderDto>> GetAll();
        ApiResponse<OrderDto> GetById(int id);
        void Delete(int invoiceNumber);
        void Update(int invoiceNumber, OrderDto dto);
        Task<ApiResponse<OrderDto>> ProcessAndCreateOrderFromPdf(IFormFile file);
        Task<ApiResponse<OrderDto>> AddCMRFileAsync(List<IFormFile> files, int invoiceNumber);
        void DeleteCMR(string fileName);
        Task<byte[]> CreateInvoicePDF(int invoiceNumber);
        Task<byte[]> CreateInvoices(DownloadDto dto);
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

        public ApiResponse<IEnumerable<OrderDto>> GetAll()
        {
            var orders = _dbContext.Orders.Include(o => o.Driver)
                                          .Include(c => c.CMRFiles)
                                          .ToList();

            var ordersDto = _mapper.Map<List<OrderDto>>(orders);
            return ApiResponse<IEnumerable<OrderDto>>.Success(ordersDto);
        }

        public ApiResponse<OrderDto> GetById(int id)
        {
            var order = _dbContext.Orders.Include(o => o.Driver)
                                         .Include(c => c.CMRFiles)
                                         .FirstOrDefault(o => o.InvoiceNumber == id);
            if (order == null) return ApiResponse<OrderDto>.Failure("Nie znaleziono zlecenia.", HttpStatusCode.BadRequest);

            return ApiResponse<OrderDto>.Success(_mapper.Map<OrderDto>(order));
        }

        public void Delete(int invoiceNumber)
        {
            var order = _dbContext.Orders.FirstOrDefault(o => o.InvoiceNumber == invoiceNumber);
            if (order == null) return;

            _dbContext.Orders.Remove(order);
            _dbContext.SaveChanges();
        }

        public void Update(int invoiceNumber, OrderDto dto)
        {
            var order = _dbContext.Orders.FirstOrDefault(o => o.InvoiceNumber == invoiceNumber);
            if (order == null) return;

            // Mapowanie dto na istniejące zamówienie
            _mapper.Map(dto, order);
            if(dto.Driver != null)
                order.Driver = _dbContext.Users.FirstOrDefault(u => u.Email == dto.Driver.Email);
            _dbContext.SaveChanges();
        }

        public async Task<ApiResponse<OrderDto>> ProcessAndCreateOrderFromPdf(IFormFile file)
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

            var fileName = $"{Guid.NewGuid()}.pdf";
            var filePath = Path.Combine(uploadsFolderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var extractedData = await _openAIService.GetInfoFromText(file);
            extractedData.InvoiceIssueDate = DateTime.Now;
            extractedData.InvoiceNumber = GetInvoiceNumber(extractedData.UnloadingDate);
            extractedData.PDFUrl = fileName;

            var order = _mapper.Map<Order>(extractedData);
            _dbContext.Orders.Add(order);
            _dbContext.SaveChanges();
            
            return ApiResponse<OrderDto>.Success(extractedData);
        }

        private int GetInvoiceNumber(DateTime date)
        {
            int invoiceNumber = _dbContext.Orders.Where(o => o.InvoiceIssueDate.Value.Month == date.Month
                                                          && o.InvoiceIssueDate.Value.Year == date.Year).Count() + 1;
            return int.Parse($"{invoiceNumber}{date.Month}{date.Year}");
        }

        public async Task<ApiResponse<OrderDto>> AddCMRFileAsync(List<IFormFile> files, int invoiceNumber)
        {
            var order = _dbContext.Orders.FirstOrDefault(o => o.InvoiceNumber == invoiceNumber);
            if (order == null) return ApiResponse<OrderDto>.Failure("Nie znaleziono zlecenia.", HttpStatusCode.BadRequest);

            if (files == null || files.Count == 0) return ApiResponse<OrderDto>.Failure("Nie przesłano pliku", HttpStatusCode.BadRequest);

            var cmrFileUrls = new List<string>();
            foreach (var file in files)
            {
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/pdfs", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = Path.Combine("/pdfs", fileName);
                cmrFileUrls.Add(relativePath);

                var cmrFile = new CMRFile
                {
                    Url = fileName,
                    OrderId = order.Id,
                    Order = order
                };
                _dbContext.CMRFiles.Add(cmrFile);
                _dbContext.SaveChanges();
            }
            return GetById(invoiceNumber);

        }

        public void DeleteCMR(string fileName)
        {
            var cmrFile = _dbContext.CMRFiles.FirstOrDefault(c => c.Url == fileName);
            if (cmrFile == null) return;

            _dbContext.CMRFiles.Remove(cmrFile);
            _dbContext.SaveChanges();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cmrFile.Url.TrimStart('/'));

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        public async Task<byte[]> CreateInvoicePDF(int invoiceNumber)
        {
            var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.InvoiceNumber == invoiceNumber);
            if (order == null)
            {
                throw new Exception("Zamówienie o podanym numerze faktury nie zostało znalezione.");
            }

            return await _invoiceService.GenerateInvoicePdf(order);
        }
        public async Task<byte[]> CreateInvoices(DownloadDto dto)
        {
            var orders = await _dbContext.Orders.FirstOrDefaultAsync(o => o.InvoiceIssueDate.Value.Month == dto.Month && o.InvoiceIssueDate.Value.Year == dto.Year);
            if (orders == null)
            {
                throw new Exception("Zamówienie o podanym numerze faktury nie zostało znalezione.");
            }

            return await _invoiceService.GenerateInvoicePdf(orders);
        }
    }
}
