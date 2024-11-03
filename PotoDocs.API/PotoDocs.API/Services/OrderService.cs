using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PotoDocs.API.Entities;
using PotoDocs.API.Models;
using PotoDocs.Shared.Models;

namespace PotoDocs.API.Services
{
    public interface IOrderService
    {
        ApiResponse<IEnumerable<OrderDto>> GetAll();
        ApiResponse<OrderDto> GetById(int id);
        void Delete(int id);
        void Update(int invoiceNumber, OrderDto dto);
        Task<ApiResponse<OrderDto>> ProcessAndCreateOrderFromPdf(IFormFile file);
        public void AddCMRFile(CMRFile cmrFile);
    }

    public class OrderService : IOrderService
    {
        private readonly PotodocsDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IOpenAIService _openAIService;

        public OrderService(PotodocsDbContext dbContext, IMapper mapper, IOpenAIService openAIService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _openAIService = openAIService;
        }

        public ApiResponse<IEnumerable<OrderDto>> GetAll()
        {
            var orders = _dbContext.Orders.Include(o => o.Driver).ToList();
            var ordersDto = _mapper.Map<List<OrderDto>>(orders);
            return ApiResponse<IEnumerable<OrderDto>>.Success(ordersDto);
        }

        public ApiResponse<OrderDto> GetById(int id)
        {
            var order = _dbContext.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) return null;

            return ApiResponse<OrderDto>.Success(_mapper.Map<OrderDto>(order));
        }

        public void Delete(int id)
        {
            var order = _dbContext.Orders.FirstOrDefault(o => o.Id == id);
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
                return null;
            }

            // Ekstrakcja danych z pliku PDF za pomocą usługi OpenAI
            var extractedData = await _openAIService.GetInfoFromText(file);
            extractedData.InvoiceIssueDate = DateTime.Now;
            extractedData.InvoiceNumber = GetInvoiceNumber(extractedData.UnloadingDate);
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

        public void AddCMRFile(CMRFile cmrFile)
        {
            _dbContext.CMRFiles.Add(cmrFile);
            _dbContext.SaveChanges();
        }
    }
}
