using System.Data;
using Dapper;
using Models;

namespace Infrastructure;

public class OrderRepository
{
    private readonly IDbConnection _dbConnection;
    private readonly string _databaseSchema;
    
    public OrderRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
        _databaseSchema = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" 
            ?  "testing"
            :  "production";
    }
    
    public async Task<Order> Create(Order orderToCreate)
    {
        using var transaction = _dbConnection.BeginTransaction();
        try
        {
            var insertAddressSql = @$"INSERT INTO {_databaseSchema}.address (street_name, house_number, house_number_addition, city, postal_code, country)) 
                                    VALUES (@StreetName, @HouseNumber, @HouseNumberAddition, @City, @PostalCode, @Country) 
                                    RETURNING address_id";
            var address = orderToCreate.Customer!.Address!;
            var addressId = await _dbConnection.QuerySingleAsync<Guid>(insertAddressSql, new
            {
                address.StreetName, 
                address.HouseNumber,
                address.HouseNumberAddition,
                address.City,
                address.PostalCode,
                address.Country
            }, transaction);
        
            var insertCustomerSql = @$"INSERT INTO {_databaseSchema}.customer (first_name, last_name, email, phone_number)) 
                                    VALUES (@FirstName, @LastName, @Email, @PhoneNumber) 
                                    RETURNING customer_id";
            var customer = orderToCreate.Customer;
            var customerId = await _dbConnection.QuerySingleAsync<Guid>(insertCustomerSql, new
            {
                customer.FirstName,
                customer.LastName,
                customer.Email,
                customer.PhoneNumber
            }, transaction);
        
            var insertCustomerAddressLinkSql = @$"INSERT INTO {_databaseSchema}.customer_address_link (customer_id, address_id) 
                                    VALUES (@CustomerId, @AddressId)";
            await _dbConnection.QuerySingleAsync<Guid>(insertCustomerAddressLinkSql, new {CustomerId = customerId, AddressId = addressId}, transaction);

            var insertOrderSql = @$"INSERT INTO {_databaseSchema}.order (status, created_at, updated_at, customer_id, address_id)) 
                                    VALUES (@Status, @CreatedAt, @UpdatedAt, @CustomerId, @AddressId) 
                                    RETURNING 
                                    order_id AS {nameof(Order.Id)},
                                    status AS {nameof(Order.ShippingStatus)},
                                    created_at AS {nameof(Order.CreatedAt)},
                                    updated_at AS {nameof(Order.UpdatedAt)},
                                    customer_id AS {nameof(Order.Customer)},
                                    address_id AS {nameof(Order.Customer.Address)}";

            var order = await _dbConnection.QuerySingleAsync<Order>(insertOrderSql, new
            {
                orderToCreate.ShippingStatus,
                orderToCreate.CreatedAt,
                orderToCreate.UpdatedAt,
                CustomerId = customerId,
                AddressId = addressId
            }, transaction);
        
            foreach (var box in orderToCreate.Boxes!)
            {
                var insertBoxOrderSql = @$"INSERT INTO {_databaseSchema}.box_order_link (box_id, order_id, quantity) 
                                    VALUES (@BoxId, @OrderId, @Quantity)";
                await _dbConnection.QuerySingleAsync<Box>(insertBoxOrderSql, new
                {
                    BoxId = box.Key, OrderId = order.Id, box.Value
                }, transaction);
            }
            
            transaction.Commit();
            return order;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            transaction.Rollback();
            throw;
        }
    }
    
    // Get all orders
    public async Task<IEnumerable<Order>> Get()
    {
        throw new NotImplementedException();
    }
    
    // Get received preparing orders
    public async Task<IEnumerable<Order>> GetByStatus(ShippingStatus status)
    {
        throw new NotImplementedException();
    }
    
    // Update shipping status
    public async Task<Order> UpdateStatus(Order orderToUpdate)
    {
        throw new NotImplementedException();
    }
    
    // Delete order if received
    public async Task Delete(Guid id)
    {
        throw new NotImplementedException();
    }
}