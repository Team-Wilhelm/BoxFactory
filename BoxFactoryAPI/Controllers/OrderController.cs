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
    public async Task<Order> Create(OrderCreateDto orderCreateDto)
    {
        return await _orderService.Create(orderCreateDto);
    }
}