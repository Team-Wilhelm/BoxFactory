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
            ? "testing"
            : "production";
    }

    public async Task<Order> Create(Order orderToCreate)
    {
        using var transaction = _dbConnection.BeginTransaction();
        try
        {
            var addressId = await InsertAddress(orderToCreate.Customer!.Address!, transaction);
            var customerId = await InsertCustomer(orderToCreate.Customer!, transaction);
            await InsertCustomerAddressLink(customerId, addressId, transaction);
            var order = await InsertOrder(orderToCreate, customerId, addressId, transaction);
            foreach (var box in orderToCreate.Boxes!)
            {
                await InsertBoxOrderLink(box, order.Id, transaction);
            }

            transaction.Commit();
            return order;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            Console.WriteLine(e.Message);
            Console.WriteLine(e.InnerException);
            Console.WriteLine(e.StackTrace);
            throw new Exception("Failed to complete transaction.");
        }
    }

    private async Task<Guid> InsertAddress(Address address, IDbTransaction transaction)
    {
        var insertAddressSql =
            @$"INSERT INTO {_databaseSchema}.address (street_name, house_number, house_number_addition, city, postal_code, country))
    VALUES (@StreetName, @HouseNumber, @HouseNumberAddition, @City, @PostalCode, @Country)
    RETURNING address_id";
        return await _dbConnection.QuerySingleAsync<Guid>(insertAddressSql, address, transaction);
    }

    private async Task<Guid> InsertCustomer(Customer customer, IDbTransaction transaction)
    {
        var insertCustomerSql = @$"INSERT INTO {_databaseSchema}.customer (first_name, last_name, email, phone_number)) 
    VALUES (@FirstName, @LastName, @Email, @PhoneNumber) 
    RETURNING customer_id";
        return await _dbConnection.QuerySingleAsync<Guid>(insertCustomerSql, customer, transaction);
    }

    private async Task InsertCustomerAddressLink(Guid customerId, Guid addressId, IDbTransaction transaction)
    {
        var insertCustomerAddressLinkSql =
            @$"INSERT INTO {_databaseSchema}.customer_address_link (customer_id, address_id) 
    VALUES (@CustomerId, @AddressId)";
        await _dbConnection.ExecuteAsync(insertCustomerAddressLinkSql,
            new { CustomerId = customerId, AddressId = addressId }, transaction);
    }

    private async Task<Order> InsertOrder(Order orderToCreate, Guid customerId, Guid addressId,
        IDbTransaction transaction)
    {
        var insertOrderSql =
            @$"INSERT INTO {_databaseSchema}.order (status, created_at, updated_at, customer_id, address_id)) 
    VALUES (@Status, @CreatedAt, @UpdatedAt, @CustomerId, @AddressId) 
    RETURNING 
    order_id AS {nameof(Order.Id)},
    status AS {nameof(Order.ShippingStatus)},
    created_at AS {nameof(Order.CreatedAt)},
    updated_at AS {nameof(Order.UpdatedAt)},
    customer_id AS {nameof(Order.Customer)},
    address_id AS {nameof(Order.Customer.Address)}";
        return await _dbConnection.QuerySingleAsync<Order>(insertOrderSql, new
        {
            orderToCreate.ShippingStatus,
            orderToCreate.CreatedAt,
            orderToCreate.UpdatedAt,
            CustomerId = customerId,
            AddressId = addressId
        }, transaction);
    }

    private async Task InsertBoxOrderLink(KeyValuePair<Guid, int> boxOrderLink, Guid orderId, IDbTransaction transaction)
    {
        var insertBoxOrderSql = @$"INSERT INTO {_databaseSchema}.box_order_link (box_id, order_id, quantity) 
    VALUES (@BoxId, @OrderId, @Quantity)";
        await _dbConnection.ExecuteAsync(insertBoxOrderSql, new
        {
            BoxId = boxOrderLink.Key,
            OrderId = orderId,
            Quantity = boxOrderLink.Value
        }, transaction);
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
            ON o.customer_email = c.email 
        INNER JOIN {_databaseSchema}.customer_address_link cal 
            ON c.email = cal.customer_email  
        INNER JOIN {_databaseSchema}.address a 
            ON cal.address_id = a.address_id";

        var orders = await _dbConnection.QueryAsync<Order>(sql);

        foreach (var order in orders)
        {
            var boxSql = @$"SELECT 
box_id AS {nameof(Order.Boxes.Keys)},
quantity AS {nameof(Order.Boxes.Values)},
 FROM {_databaseSchema}.box_order_link WHERE order_id = @Id";

            var boxes = (await _dbConnection.QueryAsync<(Guid, int)>(boxSql, new { Id = order.Id }))
                .ToDictionary();
            order.Boxes = boxes;
        }

        return orders;
    }

    // Get received preparing orders
    public async Task<IEnumerable<Order>> GetByStatus(ShippingStatus status) //multiple statuses, how?
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