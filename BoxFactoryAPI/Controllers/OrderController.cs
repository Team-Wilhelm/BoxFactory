using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.DTOs;
using Models.Models;

namespace BoxFactoryAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController: ControllerBase
{
    private readonly OrderService _orderService;

    public OrderController(OrderService orderService)
    {
        _orderService = orderService;
    }
    
    [HttpPost]
    public async Task<ActionResult<Order>> Create([FromBody]OrderCreateDto orderCreateDto)
    {
        return await _orderService.Create(orderCreateDto);
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> Get()
    {
        return Ok(await _orderService.Get());
    }
    
    [HttpGet("{status}")]
    public async Task<ActionResult<IEnumerable<Order>>> GetByStatus(ShippingStatus status)
    {
        return Ok(await _orderService.GetByStatus(status));
    }
    
    [HttpGet("latest")]
    public async Task<ActionResult<IEnumerable<Order>>> GetRecent()
    {
        return Ok(await _orderService.GetLatest());
    }
    
    [HttpGet("orders-count")]
    public async Task<ActionResult<int>> GetTotalOrders()
    {
        return Ok(await _orderService.GetTotalOrders());
    }
    
    [HttpGet("revenue")]
    public async Task<ActionResult<decimal>> GetTotalRevenue()
    {
        return Ok(await _orderService.GetTotalRevenue());
    }
    
    [HttpGet("boxes-sold")]
    public async Task<ActionResult<int>> GetTotalBoxesSold()
    {
        return Ok(await _orderService.GetTotalBoxesSold());
    }
    
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult> UpdateStatus(Guid id, [FromBody]ShippingStatusUpdateDto status)
    {
        await _orderService.UpdateStatus(id, status);
        return Ok();
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _orderService.Delete(id);
        return Ok();
    }
}