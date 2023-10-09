using System.Data;
using Dapper;
using Models;
using Models.DTOs;
using Models.Models;

namespace Infrastructure;

public class OrderRepository
{
    private readonly IDbConnection _dbConnection;
    private readonly string _databaseSchema;
    
    public OrderRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
        /*_databaseSchema = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" 
            ?  "testing"
            :  "production";*/
        _databaseSchema = "testing";
    }

    public async Task<Order> Create(OrderCreateDto orderToCreate)
    {
        using var transaction = _dbConnection.BeginTransaction();
        try
        {
            var address = await CreateAddress(orderToCreate.Customer.Address, transaction);
            var addressId = address.Id;
            
            await CreateCustomer(orderToCreate.Customer, transaction);
            var customer = new Customer
            {
                Email = orderToCreate.Customer.Email,
                FirstName = orderToCreate.Customer.FirstName,
                LastName = orderToCreate.Customer.LastName,
                PhoneNumber = orderToCreate.Customer.PhoneNumber,
                Address = address
            };
            
            await AddCustomerAddressLink(orderToCreate.Customer.Email, addressId, transaction);

            var order = await CreateOrder(orderToCreate.Customer.Email, addressId, transaction);
            order.Customer = customer;
            
            await AddBoxOrderLink(orderToCreate.Boxes, order.Id, transaction);
            order.Boxes = orderToCreate.Boxes;
            
            transaction.Commit();
            return order;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            Console.WriteLine(e.Message, e.InnerException);
            throw new Exception("Failed to complete order transaction.");
        }
    }
    
    // Get all orders
    public async Task<IEnumerable<Order>> Get()
    {
        var sql = $@"SELECT 
    o.order_id AS {nameof(Order.Id)},
    o.status AS {nameof(Order.ShippingStatus)},
    o.created_at AS {nameof(Order.CreatedAt)},
    o.updated_at AS {nameof(Order.UpdatedAt)},
    o.address_id AS {nameof(Order.Customer.Address.Id)},
    
    c.customer_email AS {nameof(Order.Customer.Email)},
    c.first_name AS {nameof(Order.Customer.FirstName)},
    c.last_name AS {nameof(Order.Customer.LastName)},
    c.phone_number AS {nameof(Order.Customer.PhoneNumber)},
    
    a.street_name AS {nameof(Order.Customer.Address.StreetName)},
    a.house_number AS {nameof(Order.Customer.Address.HouseNumber)},
    a.house_number_addition AS {nameof(Order.Customer.Address.HouseNumberAddition)},
    a.city AS {nameof(Order.Customer.Address.City)},
    a.postal_code AS {nameof(Order.Customer.Address.PostalCode)},
    a.country AS {nameof(Order.Customer.Address.Country)}
    
    FROM {_databaseSchema}.orders o 
        INNER JOIN {_databaseSchema}.customers c 
            ON o.customer_email = c.customer_email 
        INNER JOIN {_databaseSchema}.addresses a 
            ON o.address_id = a.address_id";
        
        var orders = await _dbConnection.QueryAsync<Order>(sql);
        
        foreach (var order in orders)
        {
            order.Boxes = await GetBoxesForOrder(order.Id);
        }
        
        return orders;
    }
    
    // Get received preparing orders
    public async Task<IEnumerable<Order>> GetByStatus(ShippingStatus status)
    {
        var getOrdersByStatusSql = $@"SELECT 
o.order_id AS {nameof(Order.Id)}, 
o.status AS {nameof(Order.ShippingStatus)},
o.created_at AS {nameof(Order.CreatedAt)},
o.updated_at AS {nameof(Order.UpdatedAt)},
o.address_id AS {nameof(Order.Customer.Address.Id)},

c.customer_email AS {nameof(Order.Customer.Email)},
c.first_name AS {nameof(Order.Customer.FirstName)},
c.last_name AS {nameof(Order.Customer.LastName)},
c.phone_number AS {nameof(Order.Customer.PhoneNumber)},

a.street_name AS {nameof(Order.Customer.Address.StreetName)}, 
a.house_number AS {nameof(Order.Customer.Address.HouseNumber)},
a.house_number_addition AS {nameof(Order.Customer.Address.HouseNumberAddition)},
a.city AS {nameof(Order.Customer.Address.City)},
a.postal_code AS {nameof(Order.Customer.Address.PostalCode)},
a.country AS {nameof(Order.Customer.Address.Country)} 

FROM {_databaseSchema}.orders o 
    INNER JOIN {_databaseSchema}.customers c 
        ON o.customer_email = c.customer_email 
    INNER JOIN {_databaseSchema}.addresses a 
        ON o.address_id = a.address_id
        WHERE o.status = @Status";
        
        var orders = await _dbConnection.QueryAsync<Order>(getOrdersByStatusSql, new { Status = status.ToString() });
        
        foreach (var order in orders)
        {
            order.Boxes = await GetBoxesForOrder(order.Id);
        }
        return orders;
    }
    
    
    // Update shipping status
    public async Task UpdateStatus(Guid id, ShippingStatus status)
    {
        var updateOrderStatusSql = $"UPDATE {_databaseSchema}.orders SET status = @Status WHERE order_id = @Id";
        await _dbConnection.ExecuteAsync(updateOrderStatusSql, new { Id = id, Status = status.ToString() });
        //TODO should something be returned?
    }
    
    // Delete order if received
    public async Task Delete(Guid id)
    {
        var deleteOrderSql = $"DELETE FROM {_databaseSchema}.orders WHERE order_id = @Id AND status = @Status";
        await _dbConnection.ExecuteAsync(deleteOrderSql, new { Id = id, Status = ShippingStatus.Received.ToString() });
    }
    
    
    private async Task<Order> CreateOrder(string customerEmail, Guid addressId, IDbTransaction transaction)
    {
        var insertOrderSql =
            @$"INSERT INTO {_databaseSchema}.orders (status, created_at, updated_at, customer_email, address_id) 
                                    VALUES (@Status, @CreatedAt, @UpdatedAt, @CustomerEmail, @AddressId) 
                                    RETURNING 
                                    order_id AS {nameof(Order.Id)},
                                    status AS {nameof(Order.ShippingStatus)},
                                    created_at AS {nameof(Order.CreatedAt)},
                                    updated_at AS {nameof(Order.UpdatedAt)}";

        var order = await _dbConnection.QuerySingleAsync<Order>(insertOrderSql, new
        {
            Status = ShippingStatus.Received.ToString(),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            CustomerEmail = customerEmail,
            AddressId = addressId
        }, transaction);
        return order;
    }
    
    // Create address if not exists & return Address
    private async Task<Address> CreateAddress(CreateAddressDto addressToCreate, IDbTransaction transaction)
    {
        //TODO: check if address exists
        try
        {
            var insertAddressSql =
                @$"INSERT INTO {_databaseSchema}.addresses (street_name, house_number, house_number_addition, city, postal_code, country) 
                                    VALUES (@StreetName, @HouseNumber, @HouseNumberAddition, @City, @PostalCode, @Country) 
                                    RETURNING 
                                        address_id AS {nameof(Address.Id)},
                                        street_name AS {nameof(Address.StreetName)},
                                        house_number AS {nameof(Address.HouseNumber)},
                                        house_number_addition AS {nameof(Address.HouseNumberAddition)},
                                        city AS {nameof(Address.City)},
                                        postal_code AS {nameof(Address.PostalCode)},
                                        country AS {nameof(Address.Country)}";
            var address = await _dbConnection.QuerySingleAsync<Address>(insertAddressSql, addressToCreate, transaction);
            return address;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message, e.InnerException);
            throw new Exception("Failed to create address.");
        }
    }
    
    
    // Create customer if not exists
    private async Task CreateCustomer(CreateCustomerDto customerToCreate, IDbTransaction transaction)
    {
        //TODO check if customer exists
        try
        {
            var insertCustomerSql =
                @$"INSERT INTO {_databaseSchema}.customers (first_name, last_name, customer_email, phone_number) 
                                    VALUES (@FirstName, @LastName, @Email, @PhoneNumber)";
            await _dbConnection.ExecuteAsync(insertCustomerSql, customerToCreate, transaction);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message, e.InnerException);
            throw new Exception("Failed to create customer.");
        }
    }
  
    private async Task AddCustomerAddressLink(string customerEmail, Guid addressId, IDbTransaction transaction)
    {
        //TODO check if link exists
        try
        {
            var insertCustomerAddressLinkSql =
                @$"INSERT INTO {_databaseSchema}.customer_address_link (customer_email, address_id) 
                                    VALUES (@CustomerEmail, @AddressId)";
            await _dbConnection.ExecuteAsync(insertCustomerAddressLinkSql,
                new { CustomerEmail = customerEmail, AddressId = addressId }, transaction);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message, e.InnerException);
            throw new Exception("Failed to create customer address link.");
        }
    }
    
    private async Task AddBoxOrderLink(Dictionary<Guid, int> boxes, Guid orderId, IDbTransaction transaction)
    {
        try
        {
            var insertBoxOrderLinkSql =
                @$"INSERT INTO {_databaseSchema}.box_order_link (box_id, order_id, quantity) 
                                    VALUES (@BoxId, @OrderId, @Quantity)";
            foreach (var box in boxes)
            {
                await _dbConnection.ExecuteAsync(insertBoxOrderLinkSql,
                    new { BoxId = box.Key, OrderId = orderId, Quantity = box.Value }, transaction);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message, e.InnerException);
            throw new Exception("Failed to create box order link.");
        }
    }
    
    private async Task<Dictionary<Guid, int>> GetBoxesForOrder(Guid orderId)
    {
        try
        {
            var boxSql = @$"SELECT box_id AS {nameof(Order.Boxes.Keys)}, quantity AS {nameof(Order.Boxes.Values)} 
FROM {_databaseSchema}.box_order_link 
    WHERE order_id = @Id";
            var boxes = (await _dbConnection.QueryAsync<(Guid,int)>(boxSql, new {Id = orderId})).ToDictionary();
            return boxes;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message, e.InnerException, "Failed to get boxes for order.");
            throw;
        }
    }

}