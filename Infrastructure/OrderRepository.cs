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
        _databaseSchema = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
            ?  "testing"
            :  "production";
    }

    public async Task<Order> Create(OrderCreateDto orderToCreate, DateTime? date = null)
    {
        using var transaction = _dbConnection.BeginTransaction();
        try
        {
            var customer = await CreateOrReturnCustomer(orderToCreate.Customer, transaction);

            var address = await CreateOrReturnAddress(orderToCreate.Customer.Address, transaction);

            await AddCustomerAddressLink(customer.Email, address.Id, transaction);

            var order = await CreateOrder(customer.Email, address.Id, transaction, date);
            customer.Address = address;
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
    order_id AS {nameof(Order.Id)},
    status AS {nameof(Order.ShippingStatus)},
    created_at AS {nameof(Order.CreatedAt)},
    updated_at AS {nameof(Order.UpdatedAt)}
    
    FROM {_databaseSchema}.orders";

        var orders = await _dbConnection.QueryAsync<Order>(sql);

        var enumerable = orders.ToList();
        foreach (var order in enumerable)
        {
            await FetchAdditionalOrderInfo(order);
        }
        return enumerable;
    }

    // Get received preparing orders
    public async Task<IEnumerable<Order>> GetByStatus(ShippingStatus status)
    {
        var sql = $@"SELECT 
    order_id AS {nameof(Order.Id)},
    status AS {nameof(Order.ShippingStatus)},
    created_at AS {nameof(Order.CreatedAt)},
    updated_at AS {nameof(Order.UpdatedAt)}
    
    FROM {_databaseSchema}.orders 
    WHERE status = @Status";

        var orders = await _dbConnection.QueryAsync<Order>(sql, new { Status = status.ToString() });
        var enumerable = orders.ToList();
        foreach (var order in enumerable)
        {
            await FetchAdditionalOrderInfo(order);
        }
        return enumerable;
    }

    // Get latest orders
    public async Task<IEnumerable<Order>> GetLatest()
    {
        var sql = $@"SELECT
    order_id AS {nameof(Order.Id)},
    status AS {nameof(Order.ShippingStatus)},
    created_at AS {nameof(Order.CreatedAt)},
    updated_at AS {nameof(Order.UpdatedAt)}
    
    FROM {_databaseSchema}.orders
    ORDER BY created_at DESC
    LIMIT 10;";

        var orders = await _dbConnection.QueryAsync<Order>(sql);
        var enumerable = orders.ToList();
        foreach (var order in enumerable)
        {
            await FetchAdditionalOrderInfo(order);
        }
        return enumerable;
    }
    
    // Get total price of order
    public async Task<decimal> GetTotalPrice(Guid id)
    {
        var sql = $@"SELECT SUM(price) FROM {_databaseSchema}.boxes b INNER JOIN {_databaseSchema}.box_order_link bol ON b.box_id = bol.box_id WHERE bol.order_id = @Id";
        var totalPrice = await _dbConnection.ExecuteScalarAsync<decimal>(sql, new { Id = id });
        return totalPrice;
    }
    
    // Get the amount of boxes of order
    public async Task<int> GetTotalBoxes(Guid id)
    {
        var sql = $@"SELECT SUM(quantity) FROM {_databaseSchema}.box_order_link WHERE order_id = @Id";
        var totalBoxes = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
        return totalBoxes;
    }
    
    // Get the number of all orders in the database
    public async Task<int> GetTotalOrders()
    {
        var sql = $@"SELECT COUNT(*) FROM {_databaseSchema}.orders";
        var totalOrders = await _dbConnection.ExecuteScalarAsync<int>(sql);
        return totalOrders;
    }
    
    // Get the total profit of all orders
    public async Task<decimal> GetTotalRevenue()
    {
        var sql = $@"SELECT SUM(price) FROM {_databaseSchema}.boxes b INNER JOIN {_databaseSchema}.box_order_link bol ON b.box_id = bol.box_id";
        var totalProfit = await _dbConnection.ExecuteScalarAsync<decimal>(sql);
        return totalProfit;
    }
    
    // Get the total amount of boxes sold
    public async Task<int> GetTotalBoxesSold()
    {
        var sql = $@"SELECT SUM(quantity) FROM {_databaseSchema}.box_order_link";
        var totalBoxesSold = await _dbConnection.ExecuteScalarAsync<int>(sql);
        return totalBoxesSold;
    }

    // Update shipping status
    public async Task UpdateStatus(Guid id, ShippingStatusUpdateDto status)
    {
        var updateOrderStatusSql =
            $"UPDATE {_databaseSchema}.orders SET status = @Status, updated_at = @UpdatedAt WHERE order_id = @Id";
        await _dbConnection.ExecuteAsync(updateOrderStatusSql,
            new { Id = id, Status = status.ShippingStatus.ToString(), UpdatedAt = DateTime.Now });
    }

    // Delete order if received
    public async Task Delete(Guid id)
    {
        var deleteBoxOrderLinkSql = $@"DELETE FROM {_databaseSchema}.box_order_link WHERE order_id = @Id";
        await _dbConnection.ExecuteAsync(deleteBoxOrderLinkSql, new { Id = id });

        var deleteOrderSql = $@"DELETE FROM {_databaseSchema}.orders WHERE order_id = @Id AND status = @Status";
        await _dbConnection.ExecuteAsync(deleteOrderSql, new { Id = id, Status = ShippingStatus.Received.ToString() });
    }


    private async Task<Order> CreateOrder(string customerEmail, Guid addressId, IDbTransaction transaction, DateTime? customCreatedAt = null)
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
            CreatedAt = customCreatedAt ?? DateTime.Now,
            UpdatedAt = customCreatedAt ?? DateTime.Now,
            CustomerEmail = customerEmail,
            AddressId = addressId
        }, transaction);

        return order;
    }

    // Create address if not exists & return Address
    private async Task<Address> CreateOrReturnAddress(CreateAddressDto addressToCreate, IDbTransaction transaction)
    {
        try
        {
            // Check if the address already exists
            var checkAddressExistsSql = $@"SELECT address_id FROM {_databaseSchema}.addresses 
                                       WHERE street_name = @StreetName 
                                         AND house_number = @HouseNumber 
                                         AND house_number_addition = @HouseNumberAddition 
                                         AND city = @City 
                                         AND postal_code = @PostalCode 
                                         AND country = @Country";

            var existingAddressId =
                await _dbConnection.ExecuteScalarAsync<Guid?>(checkAddressExistsSql, addressToCreate, transaction);

            if (existingAddressId != null)
            {
                // Address already exists, return it
                return await GetAddressById(existingAddressId.Value, transaction);
            }

            // Address does not exist, insert it
            var insertAddressSql =
                @$"INSERT INTO {_databaseSchema}.addresses (street_name, house_number, house_number_addition, city, postal_code, country) 
                                    VALUES (@StreetName, @HouseNumber, @HouseNumberAddition, @City, @PostalCode, @Country) 
                                    RETURNING address_id";

            var newAddressId =
                await _dbConnection.ExecuteScalarAsync<Guid>(insertAddressSql, addressToCreate, transaction);

            // Return the newly created address
            return await GetAddressById(newAddressId, transaction);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message, e.InnerException, "Failed to create address.");
            throw;
        }
    }

    private async Task<Address> GetAddressById(Guid addressId, IDbTransaction transaction)
    {
        try
        {
            var getAddressSql = $@"SELECT 
    address_id AS {nameof(Address.Id)},
    street_name AS {nameof(Address.StreetName)}, 
    house_number AS {nameof(Address.HouseNumber)},
    house_number_addition AS {nameof(Address.HouseNumberAddition)},
    city AS {nameof(Address.City)},
    postal_code AS {nameof(Address.PostalCode)},
    country AS {nameof(Address.Country)}
    FROM {_databaseSchema}.addresses 
                                       WHERE address_id = @AddressId";
            return await _dbConnection.QuerySingleAsync<Address>(getAddressSql, new { AddressId = addressId },
                transaction);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message, e.InnerException, "Failed to get address by id.");
            throw;
        }
    }
    
    private async Task<Customer> CreateOrReturnCustomer(CreateCustomerDto customerToCreate, IDbTransaction transaction)
    {
        try
        {
            var insertCustomerSql =
                @$"INSERT INTO {_databaseSchema}.customers (first_name, last_name, customer_email, phone_number, simpson_img_url) 
                VALUES (@FirstName, @LastName, @Email, @PhoneNumber, @SimpsonImgUrl) 
                ON CONFLICT (customer_email) DO NOTHING
                RETURNING  
                first_name AS {nameof(Customer.FirstName)}, 
                last_name AS {nameof(Customer.LastName)}, 
                customer_email AS {nameof(Customer.Email)}, 
                phone_number AS {nameof(Customer.PhoneNumber)},
                simpson_img_url AS {nameof(Customer.SimpsonImgUrl)}";

            return await _dbConnection.QuerySingleAsync<Customer>(insertCustomerSql, customerToCreate, transaction);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message, e.InnerException, "Failed to create customer.");
            throw;
        }
    }

    private async Task AddCustomerAddressLink(string customerEmail, Guid addressId, IDbTransaction transaction)
    {
        try
        {
            var linkExistsSql = @$"SELECT COUNT(*) FROM {_databaseSchema}.customer_address_link 
                                WHERE customer_email = @CustomerEmail AND address_id = @AddressId";

            int linkCount = await _dbConnection.ExecuteScalarAsync<int>(linkExistsSql,
                new { CustomerEmail = customerEmail, AddressId = addressId }, transaction);

            if (linkCount == 0)
            {
                // Link does not exist, so insert it
                var insertCustomerAddressLinkSql =
                    @$"INSERT INTO {_databaseSchema}.customer_address_link (customer_email, address_id) 
                    VALUES (@CustomerEmail, @AddressId)";

                await _dbConnection.ExecuteAsync(insertCustomerAddressLinkSql,
                    new { CustomerEmail = customerEmail, AddressId = addressId }, transaction);
            }
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

    private async Task<Address> GetAddress(Guid orderId)
    {
        try
        {
            var addressSql = $@"SELECT 
    o.address_id AS {nameof(Address.Id)},
    a.street_name AS {nameof(Address.StreetName)},
    a.house_number AS {nameof(Address.HouseNumber)},
    a.house_number_addition AS {nameof(Address.HouseNumberAddition)},
    a.city AS {nameof(Address.City)},
    a.postal_code AS {nameof(Address.PostalCode)},
    a.country AS {nameof(Address.Country)} 
FROM {_databaseSchema}.addresses a INNER JOIN {_databaseSchema}.orders o ON a.address_id = o.address_id
    WHERE o.order_id = @OrderId
";
            var address = await _dbConnection.QuerySingleAsync<Address>(addressSql, new { OrderId = orderId });
            return address;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message, e.InnerException, "Failed to get address for order.");
            throw;
        }
    }

    private async Task<Customer> GetCustomer(Guid orderId)
    {
        try
        {
            var customerSql = $@"SELECT 
    o.customer_email AS {nameof(Customer.Email)},
    c.first_name AS {nameof(Customer.FirstName)},
    c.last_name AS {nameof(Customer.LastName)},
    c.phone_number AS {nameof(Customer.PhoneNumber)},
    c.simpson_img_url AS {nameof(Customer.SimpsonImgUrl)}
FROM {_databaseSchema}.customers c INNER JOIN {_databaseSchema}.orders o ON c.customer_email = o.customer_email
    WHERE o.order_id = @OrderId
";
            var customer = await _dbConnection.QuerySingleAsync<Customer>(customerSql, new { OrderId = orderId });
            return customer;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message, e.InnerException, "Failed to get customer for order.");
            throw;
        }
    }

    private async Task<Dictionary<Guid, int>> GetBoxesForOrder(Guid orderId)
    {
        try
        {
            var boxSql = @$"SELECT box_id AS {nameof(Order.Boxes.Keys)}, quantity AS {nameof(Order.Boxes.Values)} 
FROM {_databaseSchema}.box_order_link 
    WHERE order_id = @Id";

            var boxes = (await _dbConnection.QueryAsync<(Guid, int)>(boxSql, new { Id = orderId })).ToDictionary();
            return boxes;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message, e.InnerException, "Failed to get boxes for order.");
            throw;
        }
    }

    public async Task FetchAdditionalOrderInfo(Order order)
    {
        order.Boxes = await GetBoxesForOrder(order.Id);
        var customer = await GetCustomer(order.Id);
        order.Customer = customer;
        customer.Address = await GetAddress(order.Id);
        order.TotalPrice = await GetTotalPrice(order.Id);
        order.TotalBoxes = await GetTotalBoxes(order.Id);
    }
}