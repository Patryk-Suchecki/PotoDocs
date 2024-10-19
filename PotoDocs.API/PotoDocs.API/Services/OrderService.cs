using AutoMapper;
using PotoDocs.API.Entities;
using PotoDocs.API.Models;
using PotoDocs.Shared.Models;

namespace PotoDocs.API.Services
{
    public interface IOrderService
    {
        IEnumerable<OrderDto> GetAll();
        OrderDto GetById(int id);
        void Delete(int id);
        void Update(int id, OrderDto dto);
        Task<OrderDto> ProcessAndCreateOrderFromPdf(IFormFile file);
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

        public IEnumerable<OrderDto> GetAll()
        {
            var orders = _dbContext.Orders.ToList();
            var ordersDto = _mapper.Map<List<OrderDto>>(orders);
            return ordersDto;
        }

        public OrderDto GetById(int id)
        {
            var order = _dbContext.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) return null;

            return _mapper.Map<OrderDto>(order);
        }

        public void Delete(int id)
        {
            var order = _dbContext.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) return;

            _dbContext.Orders.Remove(order);
            _dbContext.SaveChanges();
        }

        public void Update(int id, OrderDto dto)
        {
            var order = _dbContext.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) return;

            // Mapowanie dto na istniejące zamówienie
            _mapper.Map(dto, order);
            _dbContext.SaveChanges();
        }

        public async Task<OrderDto> ProcessAndCreateOrderFromPdf(IFormFile file)
        {
            if (file == null || file.Length == 0 || file.ContentType != "application/pdf")
            {
                return null;
            }

            // Ekstrakcja danych z pliku PDF za pomocą usługi OpenAI
            var extractedData = await _openAIService.GetInfoFromText(file);
            extractedData.InvoiceNumber = GetInvoiceNumber(extractedData.UnloadingDate);
            var order = _mapper.Map<Order>(extractedData);
            _dbContext.Orders.Add(order);
            _dbContext.SaveChanges();


            return extractedData;
        }
        private int GetInvoiceNumber(DateTime date)
        {
            return _dbContext.Orders
                .Where(o => date.Month == date.Month && o.InvoiceIssueDate.Year == date.Year)
                .OrderByDescending(o => o.InvoiceNumber)
                .ToList()
                .FirstOrDefault()?.InvoiceNumber ?? 1;
        }

        public void AddCMRFile(CMRFile cmrFile)
        {
            _dbContext.CMRFiles.Add(cmrFile);
            _dbContext.SaveChanges();
        }
    }
}
