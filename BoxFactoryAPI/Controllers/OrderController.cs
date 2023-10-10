using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.DTOs;

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