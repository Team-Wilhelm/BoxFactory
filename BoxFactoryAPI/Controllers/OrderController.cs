using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Models;

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
}