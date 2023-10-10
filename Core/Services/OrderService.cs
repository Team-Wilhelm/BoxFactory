using Infrastructure;
using Models;
using Models.DTOs;
using Models.Models;

namespace Core.Services;

public class OrderService
{
    private readonly OrderRepository _orderRepository;

    public OrderService(OrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }
    
    public async Task<Order> Create(OrderCreateDto orderCreateDto)
    {
        if (orderCreateDto.Boxes.Count == 0) throw new Exception("No boxes in order.");
        try
        {
            return await _orderRepository.Create(orderCreateDto);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message, e.InnerException);
            throw new Exception("Something went wrong while creating order.");
        }
    }
    
    public async Task<IEnumerable<Order>> Get()
    {
        try
        {
            return await _orderRepository.Get();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message, e.InnerException);
            throw new Exception("Something went wrong while fetching orders.");
        }
    }

    public async Task<IEnumerable<Order>> GetByStatus(ShippingStatus status)
    {
        try
        {
            return await _orderRepository.GetByStatus(status);
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message, e.InnerException);
            throw new Exception("Something went wrong while fetching these orders.");
        }
    }
    
    public async Task UpdateStatus(Guid id, ShippingStatusUpdateDto status)
    {
            try
            {
                await _orderRepository.UpdateStatus(id, status);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message, e.InnerException);
                throw new Exception("Something went wrong while updating order status.");
            }
    }
    
    public async Task Delete(Guid id)
    {
        try
        {
            await _orderRepository.Delete(id);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message, e.InnerException);
            throw new Exception("Something went wrong while deleting order.");
        }
    }
}