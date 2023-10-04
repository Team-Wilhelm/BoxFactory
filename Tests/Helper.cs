using System.Data;
using Dapper;
using Models;
using Npgsql;

namespace Tests;

public static class Helper
{
    public static readonly IDbConnection DbConnection;
    public const string UrlBase = "http://localhost:5133";

    static Helper()
    {
        DbConnection = new NpgsqlConnection(Environment.GetEnvironmentVariable("box_conn"));
        DbConnection.Open();
    }

    public static BoxCreateDto CreateBoxCreateDto(float weight, string colour, string material, float price, int stock, float height, float length, float width)
    {
        return new BoxCreateDto()
        {
            Weight = weight,
            Colour = colour,
            Material = material,
            Price = price,
            Stock = stock,
            DimensionsDto = new DimensionsDto()
            {
                Height = height,
                Length = length,
                Width = width
            }
        };
    }
    public static void TriggerRebuild()
    {
        try
        {
            DbConnection.Execute(RebuildScript);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public static async Task<Box> InsertBoxIntoDatabase(BoxCreateDto boxDto)
    {
        // Insert dimensions
        var sql = $@"INSERT INTO testing.dimensions (length, width, height) 
                        VALUES (@Length, @Width, @Height) 
                        RETURNING 
                            dimensions_id AS {nameof(Dimensions.Id)},
                            length AS {nameof(Dimensions.Length)},
                            height AS {nameof(Dimensions.Height)},
                            width AS {nameof(Dimensions.Width)}";
        var dimensions = await DbConnection.QuerySingleAsync<Dimensions>(sql, boxDto.DimensionsDto);

        // Insert the box and link it to the dimensions, set the box's dimensions to the dimensions object
        sql = @$"INSERT INTO testing.boxes (weight, colour, material, price, created_at, stock, dimensions_id) 
                    VALUES (@Weight, @Colour, @Material, @Price, NOW(), @Stock, @DimensionsId)
                    RETURNING
                        weight AS {nameof(Box.Weight)},
                        colour AS {nameof(Box.Colour)},
                        material AS {nameof(Box.Material)},
                        price AS {nameof(Box.Price)},
                        created_at AS {nameof(Box.CreatedAt)},
                        stock AS {nameof(Box.Stock)},
                        box_id AS {nameof(Box.Id)};";
        var box = await DbConnection.QuerySingleAsync<Box>(sql, new
        {
            boxDto.Weight,
            boxDto.Colour,
            boxDto.Material,
            boxDto.Price,
            boxDto.Stock,
            DimensionsId = dimensions.Id
        });
        box.Dimensions = dimensions;
        return box;
    }

    /// <summary>
    /// Inserts a valid box into the database and returns the box with the id. 
    /// </summary>
    /// <returns>A box with weight 20, colour red, material cardboard, price 9, stock 20, height 20, length 20, width 10.</returns>
    public static async Task<Box> GetValidBoxFromDatabase()
    {
        var boxDto = CreateBoxCreateDto(20, "red", "cardboard", 9, 20, 20, 20, 10);
        return await InsertBoxIntoDatabase(boxDto);
    }
    
     private static string RebuildScript = @"
DROP SCHEMA IF EXISTS testing CASCADE;
CREATE SCHEMA testing;

CREATE TABLE IF NOT EXISTS testing.Dimensions
(
    dimensions_id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    length        float,
    width         float,
    height        float
);

CREATE TABLE IF NOT EXISTS testing.Boxes
(
    box_id        uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    weight        float,
    colour        varchar(25),
    material      varchar(25),
    price         float,
    created_at    timestamp,
    stock         integer,
    dimensions_id uuid REFERENCES testing.Dimensions (dimensions_id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS testing.Customers
(
    customer_id  uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    email        varchar,
    phone_number varchar(20),
    first_name   varchar(25),
    last_name    varchar
);

CREATE TABLE IF NOT EXISTS testing.Addresses
(
    address_id            uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    street_name           varchar,
    house_number          integer,
    house_number_addition varchar(10),
    city                  varchar,
    postal_code           varchar(10),
    country               varchar(25)
);

CREATE TABLE IF NOT EXISTS testing.Orders
(
    order_id    uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    status      varchar(25),
    created_at  timestamp,
    updated_at  timestamp,
    customer_id uuid REFERENCES testing.Customers (customer_id),
    address_id  uuid REFERENCES testing.Addresses (address_id)
);

CREATE TABLE IF NOT EXISTS testing.Box_Order_Link
(
    PRIMARY KEY (
        box_id,
        order_id
    ),
    box_id   uuid REFERENCES testing.Boxes (box_id),
    order_id uuid REFERENCES testing.Orders (order_id),
    quantity integer
);

CREATE TABLE IF NOT EXISTS testing.Customer_Address_Link
(
    PRIMARY KEY (
        customer_id,
        address_id
    ),
    customer_id uuid REFERENCES testing.Customers (customer_id),
    address_id  uuid REFERENCES testing.Addresses (address_id)
);
 ";
}