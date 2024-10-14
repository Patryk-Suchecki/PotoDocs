using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.X509;
using PotoDocs.API.Models;
using PotoDocs.API.Services;
using PotoDocs.Shared.Models;

namespace PotoDocs.API.Controllers;

[Route("api/transportorder")]
[ApiController]
[Authorize]
public class TransportOrderController : ControllerBase
{
    private readonly PotodocsDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IOpenAIService _openAIService;

    public TransportOrderController(PotodocsDbContext dbContext, IMapper mapper, IOpenAIService openAIService)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _openAIService = openAIService;
    }

    [HttpGet("all")]
    public ActionResult<IEnumerable<TransportOrderDto>> GetAll()
    {
        var orders = _dbContext.Orders.ToList();

        var adminOrderDtos = _mapper.Map<List<TransportOrderDto>>(orders);

        return Ok(adminOrderDtos);
    }

    [HttpGet("{id}")]
    public ActionResult<Order> GetById([FromRoute] int id)
    {
        var order = _dbContext.Orders.FirstOrDefault(o => o.Id == id);

        if (order is null)
        {
            return NotFound();
        }

        var transportOrderDto = _mapper.Map<TransportOrderDto>(order);

        return Ok(transportOrderDto);
    }

    [HttpPost]
    public ActionResult Create([FromBody] TransportOrderDto dto)
    {
        var order = _mapper.Map<Order>(dto);
        _dbContext.Orders.Add(order);
        _dbContext.SaveChanges();
        return Created($"api/order/{order.Id}", null);
    }

    [HttpDelete("{id}")]
    public ActionResult Delete([FromRoute] int id)
    {
        var order = _dbContext.Orders.FirstOrDefault(o => o.Id == id);
        if (order is null) return NotFound();
        _dbContext.Orders.Remove(order);
        _dbContext.SaveChanges();
        return NoContent();
    }

    [HttpPut("{id}")]
    public ActionResult Update([FromBody] Order dto, [FromRoute] int id)
    {
        var order = _dbContext.Orders.FirstOrDefault(o => o.Id == id);
        if (order is null) return NotFound();

        order.PDFUrl = dto.PDFUrl;
        order.InvoiceNumber = dto.InvoiceNumber;
        order.LoadingAddress = dto.LoadingAddress;
        order.UnloadingAddress = dto.UnloadingAddress;
        order.CMRFiles = dto.CMRFiles;
        order.CompanyOrderNumber = dto.CompanyOrderNumber;
        order.CompanyName = dto.CompanyName;
        order.CompanyNIP = dto.CompanyNIP;
        order.Driver = dto.Driver;
        order.DaysToPayment = dto.DaysToPayment;
        order.InvoiceIssueDate  = dto.InvoiceIssueDate;

        return Ok();
    }

    [HttpPost("datafromai")]
    public async Task<ActionResult<TransportOrderDto>> UploadPdf([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        if (file.ContentType != "application/pdf")
        {
            return BadRequest("Only PDF files are allowed.");
        }

        var transportOrder = await _openAIService.GetInfoFromText(file);

        return Ok(transportOrder);
    }
}


