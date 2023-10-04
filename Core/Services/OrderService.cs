using AutoMapper;
using Infrastructure;
using Models;

namespace Core.Services;

public class OrderService
{
    private readonly OrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public OrderService(OrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }
    
    public async Task<Order> Create(OrderCreateDto orderCreateDto)
    {
        var order = _mapper.Map<Order>(orderCreateDto);
        order.CreatedAt = DateTime.Now;
        order.UpdatedAt = DateTime.Now;
        if (order.Boxes != null && order.Boxes.Count != 0)
        {
            return await _orderRepository.Create(order);
        }
        throw new Exception("No boxes in order");
    }
}