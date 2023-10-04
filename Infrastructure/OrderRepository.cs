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
        catch (Exception)
        {
            transaction.Rollback();
            throw new Exception("Failed to complete transaction.");
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
    
    c.email AS {nameof(Order.Customer.Email)},
    c.first_name AS {nameof(Order.Customer.FirstName)},
    c.last_name AS {nameof(Order.Customer.LastName)},
    c.phone_number AS {nameof(Order.Customer.PhoneNumber)},
    
    a.street_name AS {nameof(Order.Customer.Address.StreetName)},
    a.house_number AS {nameof(Order.Customer.Address.HouseNumber)},
    a.house_number_addition AS {nameof(Order.Customer.Address.HouseNumberAddition)},
    a.city AS {nameof(Order.Customer.Address.City)},
    a.postal_code AS {nameof(Order.Customer.Address.PostalCode)},
    a.country AS {nameof(Order.Customer.Address.Country)},
    
    
    FROM {_databaseSchema}.orders o 
        INNER JOIN {_databaseSchema}.customer c 
            ON o.email = c.email 
        INNER JOIN {_databaseSchema}.customer_address_link cal 
            ON c.email = cal.email  
        INNER JOIN {_databaseSchema}.address a 
            ON cal.address_id = a.address_id";
        
        var orders = await _dbConnection.QueryAsync<Order>(sql);
        
        foreach (var order in orders)
        {
            var boxSql = @$"SELECT 
box_id AS {nameof(Order.Boxes.Keys)},
quantity AS {nameof(Order.Boxes.Values)},
 FROM {_databaseSchema}.box_order_link WHERE order_id = @Id";
            
            var boxes = (await _dbConnection.QueryAsync<(Guid,int)>(boxSql, new {Id = order.Id}))
                .ToDictionary();
            order.Boxes = boxes;
        }
        
        return orders;
    }
    
    // Get received preparing orders
    public async Task<IEnumerable<Order>> GetByStatus(ShippingStatus status)//multiple statuses, how?
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