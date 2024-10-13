using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PotoDocs.API.Models;
using PotoDocs.Shared.Models;

namespace PotoDocs.API.Controllers;

public class OrderController : ControllerBase
{
    private readonly PotodocsDbContext _dbContext;
    private readonly IMapper _mapper;

    public OrderController(PotodocsDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    [HttpGet("admin")]
    public ActionResult<IEnumerable<Order>> GetAllForAdmin()
    {
        var orders = _dbContext.Orders.ToList();

        var adminOrderDtos = _mapper.Map<List<TransportOrderDto>>(orders);

        return Ok(adminOrderDtos);
    }

    [HttpGet("driver")]
    public ActionResult<IEnumerable<Order>> GetAllForDriver()
    {
        var orders = _dbContext.Orders.ToList();

        var transportOrdersDto = _mapper.Map<List<TransportOrderDto>>(orders);

        return Ok(transportOrdersDto);
    }

    [HttpGet("admin/{id}")]
    public ActionResult<Order> AdminGet([FromRoute] int id)
    {
        var order = _dbContext.Orders.FirstOrDefault(o => o.Id == id);

        if (order is null)
        {
            return NotFound();
        }

        var transportOrderDto = _mapper.Map<TransportOrderDto>(order);

        return Ok(transportOrderDto);
    }

    [HttpGet("driver/{id}")]
    public ActionResult<Order> DriverGet([FromRoute] int id)
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
    public ActionResult CreateOrder([FromBody] Order dto)
    {
        _dbContext.Orders.Add(dto);
        _dbContext.SaveChanges();
        return Created("api/order/{dto.Id}", null);
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
}


