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
            await AddCustomerAddressLink(orderToCreate.Customer.Email, addressId, transaction);
            
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
                Status = ShippingStatus.Preparing.ToString(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CustomerEmail = orderToCreate.Customer.Email,
                AddressId = addressId
            }, transaction);

            var customer = new Customer
            {
                Email = orderToCreate.Customer.Email,
                FirstName = orderToCreate.Customer.FirstName,
                LastName = orderToCreate.Customer.LastName,
                PhoneNumber = orderToCreate.Customer.PhoneNumber,
                Address = address
            };
            order.Customer = customer;
            
            await AddBoxOrderLink(orderToCreate.Boxes, order.Id, transaction);
            
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
    
    // Create address if not exists & return Id (transaction)
    public async Task<Address> CreateAddress(CreateAddressDto addressToCreate, IDbTransaction transaction)
    {
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
    
    
    // Create customer if not exists (transaction)
    public async Task CreateCustomer(CreateCustomerDto customerToCreate, IDbTransaction transaction)
    {
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
    
    // Add customer and address to liking table (transaction)
    public async Task AddCustomerAddressLink(string customerEmail, Guid addressId, IDbTransaction transaction)
    {
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
    
    // Add boxes to linking table return Dictionary<BoxId, Quantity> (transaction)
    public async Task AddBoxOrderLink(Dictionary<Guid, int> boxes, Guid orderId, IDbTransaction transaction)
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
    
    
    // Get all orders
    public async Task<IEnumerable<Order>> Get()
    {
        var sql = $@"SELECT 
    o.order_id AS {nameof(Order.Id)},
    o.status AS {nameof(Order.ShippingStatus)},
    o.created_at AS {nameof(Order.CreatedAt)},
    o.updated_at AS {nameof(Order.UpdatedAt)},
    
    c.customer_email AS {nameof(Order.Customer.Email)},
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
            ON c.customer_email = cal.customer_email  
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