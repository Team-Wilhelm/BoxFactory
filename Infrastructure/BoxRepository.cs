using System.Data;
using Dapper;
using Models;

namespace Infrastructure;

public class BoxRepository
{
    private readonly IDbConnection _dbConnection;
    private readonly string _databaseSchema;
    
    public BoxRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
        _databaseSchema = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" 
            ?  "testing"
            :  "production";
        RebuildDatabase("testing");
    }
    
    public async Task<IEnumerable<Box>> Get()
    {
        return new List<Box>();
    }
    
    public async Task<Box> Get(Guid id)
    {
        return new Box();
    }
    
    public async Task<Box> Create(Box box)
    {
        var insertDimensionsSql = @$"INSERT INTO {_databaseSchema}.dimensions (length, width, height) 
                                    VALUES (@Length, @Width, @Height) RETURNING dimensions_id";
        var dimensionsId = await _dbConnection.QuerySingleAsync<Guid>(insertDimensionsSql, box.Dimensions);
        
        var sql = @$"INSERT INTO {_databaseSchema}.boxes (weight, colour, material, dimensions_id, created_at)
                     VALUES (@Weight, @Colour, @Material, @Dimensions, @CreatedAt)
                     RETURNING box_id, weight, colour, material, dimensions_id, created_at, stock";
        return await _dbConnection.QuerySingleAsync<Box>(sql, new {Dimensions = dimensionsId, box});
    }
    
    public async Task<Box> Update(Guid id, BoxUpdateDto boxUpdateDto)
    {
        throw new NotImplementedException();
    }
    
    public async Task Delete(Guid id)
    {
        throw new NotImplementedException();
    }

    private void RebuildDatabase(string schemaName)
    {
        var sql = @$"DROP SCHEMA IF EXISTS {schemaName} CASCADE;
CREATE SCHEMA {schemaName};

CREATE TABLE IF NOT EXISTS {schemaName}.Dimensions
(
    ""dimensions_id"" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    ""length""        float,
    ""width""         float,
    ""height""        float
);

CREATE TABLE IF NOT EXISTS {schemaName}.Boxes
(
    ""box_id""        uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    ""weight""        float,
    ""colour""        varchar(25),
    ""material""      varchar(25),
    ""price""         float,
    ""created_at""    timestamp,
    ""stock""         integer,
    ""dimensions_id"" uuid REFERENCES {schemaName}.Dimensions (""dimensions_id"") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS {schemaName}.Customers
(
    ""customer_id""  uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    ""email""        varchar,
    ""phone_number"" varchar(20),
    ""first_name""   varchar(25),
    ""last_name""    varchar
);

CREATE TABLE IF NOT EXISTS {schemaName}.Addresses
(
    ""address_id""            uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    ""street_name""           varchar,
    ""house_number""          integer,
    ""house_number_addition"" varchar(10),
    ""city""                  varchar,
    ""postal_code""           varchar(10),
    ""country""               varchar(25)
);

CREATE TABLE IF NOT EXISTS {schemaName}.Orders
(
    ""order_id""    uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    ""status""      varchar(25),
    ""created_at""  timestamp,
    ""updated_at""  timestamp,
    ""customer_id"" uuid REFERENCES {schemaName}.Customers (""customer_id""),
    ""address_id""  uuid REFERENCES {schemaName}.Addresses (""address_id"")
);

CREATE TABLE IF NOT EXISTS {schemaName}.Box_Order_Link
(
    PRIMARY KEY (
                 ""box_id"",
                 ""order_id""
        ),
    ""box_id""   uuid REFERENCES {schemaName}.Boxes (""box_id""),
    ""order_id"" uuid REFERENCES {schemaName}.Orders (""order_id""),
    ""quantity"" integer
);

CREATE TABLE IF NOT EXISTS {schemaName}.Customer_Address_Link
(
    PRIMARY KEY (
                 ""customer_id"",
                 ""address_id""
        ),
    ""customer_id"" uuid REFERENCES {schemaName}.Customers (""customer_id""),
    ""address_id""  uuid REFERENCES {schemaName}.Addresses (""address_id"")
);";
    }
}