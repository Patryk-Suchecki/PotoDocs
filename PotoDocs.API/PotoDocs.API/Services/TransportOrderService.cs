using AutoMapper;
using PotoDocs.API.Entities;
using PotoDocs.API.Models;
using PotoDocs.Shared.Models;

namespace PotoDocs.API.Services
{
    public interface ITransportOrderService
    {
        IEnumerable<TransportOrderDto> GetAll();
        TransportOrderDto GetById(int id);
        void Create(TransportOrderDto dto);
        void Delete(int id);
        void Update(int id, TransportOrderDto dto);
        Task<TransportOrderDto> ProcessAndCreateOrderFromPdf(IFormFile file);
    }

    public class TransportOrderService : ITransportOrderService
    {
        private readonly PotodocsDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IOpenAIService _openAIService;

        public TransportOrderService(PotodocsDbContext dbContext, IMapper mapper, IOpenAIService openAIService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _openAIService = openAIService;
        }

        public IEnumerable<TransportOrderDto> GetAll()
        {
            var orders = _dbContext.Orders.ToList();
            return _mapper.Map<List<TransportOrderDto>>(orders);
        }

        public TransportOrderDto GetById(int id)
        {
            var order = _dbContext.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) return null;

            return _mapper.Map<TransportOrderDto>(order);
        }

        public void Create(TransportOrderDto dto)
        {
            var order = _mapper.Map<Order>(dto);
            _dbContext.Orders.Add(order);
            _dbContext.SaveChanges();
        }

        public void Delete(int id)
        {
            var order = _dbContext.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) return;

            _dbContext.Orders.Remove(order);
            _dbContext.SaveChanges();
        }

        public void Update(int id, TransportOrderDto dto)
        {
            var order = _dbContext.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) return;

            // Mapowanie dto na istniejące zamówienie
            _mapper.Map(dto, order);
            _dbContext.SaveChanges();
        }

        public async Task<TransportOrderDto> ProcessAndCreateOrderFromPdf(IFormFile file)
        {
            if (file == null || file.Length == 0 || file.ContentType != "application/pdf")
            {
                return null;
            }

            // Ekstrakcja danych z pliku PDF za pomocą usługi OpenAI
            var extractedData = await _openAIService.GetInfoFromText(file);
            if (extractedData == null)
            {
                return null;
            }

            // Stworzenie encji zamówienia na podstawie wyciągniętych danych
            Create(extractedData);

            return extractedData;
        }
    }
}
