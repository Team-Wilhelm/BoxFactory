using System.Data;
using Dapper;
using Models;

namespace Infrastructure;

public class BoxRepository
{
    private readonly IDbConnection _dbConnection;
    private readonly string _databaseSchema;
    private readonly List<string> _colours;
    private readonly List<string> _materials;

    public BoxRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
        _databaseSchema = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
            ? "testing"
            : "production";
        _colours = _dbConnection.Query<string>($"SELECT name FROM {_databaseSchema}.colours").ToList();
        _materials = _dbConnection.Query<string>($"SELECT name FROM {_databaseSchema}.materials").ToList();
    }

    public async Task<IEnumerable<Box>> Get(string? searchTerm, int currentPage, int boxesPerPage, Sorting? sorting)
    {
        //TODO: Resolve searching by multiple words to only include boxes that match all words
        //TODO: Please refactor this method to use Dapper's multi-mapping feature instead of the foreach loop to avoid certain doom.

        var searchQuery = "";
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchTerms = searchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var searchCondition = string.Join(" AND ", searchTerms.Select(term => $"(colour ILIKE '%{term}%' " +
                $"OR material ILIKE '%{term}%')"));
            searchQuery = $"WHERE {searchCondition}";
        }

        var boxSql = @$"SELECT
                 box_id AS {nameof(Box.Id)},
                 weight AS {nameof(Box.Weight)},
                 colour AS {nameof(Box.Colour)}, 
                 material AS {nameof(Box.Material)}, 
                 created_at AS {nameof(Box.CreatedAt)},
                 stock AS {nameof(Box.Stock)},
                 price AS {nameof(Box.Price)}
              FROM {_databaseSchema}.boxes
              {searchQuery}
              {sorting?.Query}
              LIMIT @BoxesPerPage 
              OFFSET @Offset";
        object queryParams = new { BoxesPerPage = boxesPerPage, Offset = (currentPage - 1) * boxesPerPage };
        var boxes = (await _dbConnection.QueryAsync<Box>(boxSql, queryParams)).ToList();
        boxes.ToList().ForEach(box => box.Dimensions = GetDimensionsByBoxId(box.Id));
        return boxes;
    }

    public async Task<Box> Get(Guid id)
    {
        var boxSql = @$"SELECT
                         box_id AS {nameof(Box.Id)},
                         weight AS {nameof(Box.Weight)},
                         colour AS {nameof(Box.Colour)}, 
                         material AS {nameof(Box.Material)}, 
                         created_at AS {nameof(Box.CreatedAt)},
                         stock AS {nameof(Box.Stock)},
                         price AS {nameof(Box.Price)}
                    FROM {_databaseSchema}.boxes
                    WHERE box_id = @Id";
        var box = await _dbConnection.QuerySingleAsync<Box>(boxSql, new { Id = id });
        box.Dimensions = GetDimensionsByBoxId(box.Id);
        return box;
    }

    public async Task<Box> Create(Box box)
    {
        var transaction = _dbConnection.BeginTransaction();
        var dimensions = InsertDimensions(box.Dimensions, transaction);

        var sql =
            @$"INSERT INTO {_databaseSchema}.boxes (weight, colour, material, price, stock, dimensions_id, created_at)
                     VALUES (@Weight, @Colour, @Material, @Price, @Stock, @DimensionsID, @CreatedAt)
                     RETURNING 
                         box_id AS {nameof(Box.Id)},
                         weight AS {nameof(Box.Weight)},
                         colour AS {nameof(Box.Colour)}, 
                         material AS {nameof(Box.Material)}, 
                         created_at AS {nameof(Box.CreatedAt)},
                         stock AS {nameof(Box.Stock)},
                         price AS {nameof(Box.Price)}";

        var createdBox = await _dbConnection.QuerySingleAsync<Box>(sql, new
        {
            box.Weight,
            box.Colour,
            box.Material,
            DimensionsID = dimensions.Id,
            box.CreatedAt,
            box.Stock,
            box.Price
        });

        createdBox.Dimensions = dimensions;
        transaction.Commit();
        return createdBox;
    }

    public async Task<Box> Update(Box box)
    {
        var transaction = _dbConnection.BeginTransaction();
        var sql = @$"UPDATE {_databaseSchema}.boxes 
                     SET weight = @Weight, colour = @Colour, material = @Material, price = @Price, stock = @Stock
                     WHERE box_id = @Id
                     RETURNING 
                         box_id AS {nameof(Box.Id)},
                         weight AS {nameof(Box.Weight)},
                         colour AS {nameof(Box.Colour)}, 
                         material AS {nameof(Box.Material)},  
                         created_at AS {nameof(Box.CreatedAt)},
                         stock AS {nameof(Box.Stock)},
                         price AS {nameof(Box.Price)}";

        var updatedBox = await _dbConnection.QuerySingleAsync<Box>(sql, new
        {
            box.Id,
            box.Weight,
            box.Colour,
            box.Material,
            box.CreatedAt,
            box.Stock,
            box.Price
        });

        updatedBox.Dimensions = UpdateDimensions(box.Id, box.Dimensions, transaction);
        transaction.Commit();
        return updatedBox;
    }

    public async Task Delete(Guid id)
    {
        using var transaction = _dbConnection.BeginTransaction();
        try
        {
            var sql = $"DELETE FROM {_databaseSchema}.boxes WHERE box_id = @Id";
            await _dbConnection.ExecuteAsync(sql, new { Id = id }, transaction);

            // Commit the transaction
            transaction.Commit();
        }
        catch (Exception)
        {
            // Handle any exceptions and possibly rollback the transaction
            transaction.Rollback();
            throw;
        }
    }

    private Dimensions InsertDimensions(Dimensions dimensions, IDbTransaction transaction)
    {
        var insertDimensionsSql = @$"INSERT INTO {_databaseSchema}.dimensions (length, width, height) 
                                        VALUES (@Length, @Width, @Height) 
                                        RETURNING
                                            dimensions_id AS {nameof(Box.Dimensions.Id)},
                                            length AS {nameof(Box.Dimensions.Length)},
                                            width AS {nameof(Box.Dimensions.Width)},
                                            height AS {nameof(Box.Dimensions.Height)}
                                        ";
        return _dbConnection.QuerySingle<Dimensions>(insertDimensionsSql, dimensions, transaction);
    }

    private Dimensions UpdateDimensions(Guid boxId, Dimensions dimensions, IDbTransaction transaction)
    {
        var dimensionsId =
            _dbConnection.QuerySingle<Guid>(
                $"SELECT dimensions_id FROM {_databaseSchema}.boxes WHERE box_id = @Id", new { Id = boxId });

        var dimensionsSql = @$"UPDATE {_databaseSchema}.dimensions
                              SET length = @Length, width = @Width, height = @Height
                              WHERE dimensions_id = @Id
                              RETURNING 
                                    dimensions_id AS {nameof(Box.Dimensions.Id)},
                                    length AS {nameof(Box.Dimensions.Length)},
                                    width AS {nameof(Box.Dimensions.Width)},
                                    height AS {nameof(Box.Dimensions.Height)}
                            ";
        return _dbConnection.QuerySingle<Dimensions>(dimensionsSql, new
        {
            Id = dimensionsId,
            dimensions.Length,
            dimensions.Width,
            dimensions.Height
        });
    }

    private Dimensions GetDimensionsByBoxId(Guid boxId)
    {
        var dimensionsId =
            _dbConnection.QuerySingle<Guid>(
                $"SELECT dimensions_id FROM {_databaseSchema}.boxes WHERE box_id = @Id", new { Id = boxId });
        var dimensionsSql = @$"SELECT
                         dimensions_id AS {nameof(Dimensions.Id)},
                         length AS {nameof(Dimensions.Length)},
                         width AS {nameof(Dimensions.Width)},
                         height AS {nameof(Dimensions.Height)}
                    FROM {_databaseSchema}.dimensions
                    WHERE dimensions_id = @Id";
        return _dbConnection.QuerySingle<Dimensions>(dimensionsSql, new { Id = dimensionsId });
    }
    
    // TODO: Delete this line comment
}