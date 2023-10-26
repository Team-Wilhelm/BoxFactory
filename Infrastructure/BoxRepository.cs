using System.Data;
using Dapper;
using Models.Models;
using Models.Util;

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
            : "Production";
        _colours = _dbConnection.Query<string>($"SELECT name FROM {_databaseSchema}.colours").ToList();
        _materials = _dbConnection.Query<string>($"SELECT name FROM {_databaseSchema}.materials").ToList();
    }

    public async Task<GetBoxesResponse> Get(BoxParameters boxParameters, Sorting sorting)
    {
        var searchQuery = GetSearchAndFilterQuery(boxParameters).Keys.First();
        var parameters = GetSearchAndFilterQuery(boxParameters).Values.First();
        
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
              {sorting.Query}
              LIMIT @BoxesPerPage 
              OFFSET @Offset";
        parameters.Add("BoxesPerPage", boxParameters.BoxesPerPage);
        parameters.Add("Offset", (boxParameters.CurrentPage - 1) * boxParameters.BoxesPerPage);
        var boxes = (await _dbConnection.QueryAsync<Box>(boxSql, parameters)).ToList();
        boxes.ToList().ForEach(box => box.Dimensions = GetDimensionsByBoxId(box.Id));
        
        var countSql = @$"SELECT COUNT(*) FROM {_databaseSchema}.boxes {searchQuery}";
        var count = await _dbConnection.QuerySingleAsync<int>(countSql, parameters);
        var pageCount = count % boxParameters.BoxesPerPage == 0
            ? count / boxParameters.BoxesPerPage
            : count / boxParameters.BoxesPerPage + 1;
        
        return new GetBoxesResponse { Boxes = boxes, PageCount = pageCount };
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
        var dimensions = InsertDimensions(box.Dimensions!, transaction);

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

        updatedBox.Dimensions = UpdateDimensions(box.Id, box.Dimensions!, transaction);
        transaction.Commit();
        return updatedBox;
    }

    public async Task Delete(Guid id)
    {
        using var transaction = _dbConnection.BeginTransaction();
        //TODO: Delete dimensions
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
        }, transaction);
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

    /// <summary>
    /// A util method to create a WHERE clause for the SQL query, based on the search term and filters.
    /// </summary>
    /// <param name="boxParameters"></param>
    /// <returns>A dictionary with a single entry, where key is a WHERE SQL clause, based on the search term and filters and value is the Dynamic Parameters pertaining to this query.</returns>
    /// <exception cref="Exception"></exception>
    private Dictionary<string, DynamicParameters> GetSearchAndFilterQuery(BoxParameters boxParameters)
    {
        var parameters = new DynamicParameters(); // To avoid SQL injection
        // If the search term is not null or whitespace, we need to add a WHERE clause to the SQL query, otherwise it's omitted and will return all boxes
        var searchQuery = "";
        if (!string.IsNullOrWhiteSpace(boxParameters.SearchTerm))
        {
            var searchTerms = boxParameters.SearchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var searchCondition = new List<string>();
           
            for (int i = 0; i < searchTerms.Length; i++)
            {
                var term = $"@term{i}";
                searchCondition.Add($"(colour ILIKE {term} OR material ILIKE {term})");
                parameters.Add(term, $"%{searchTerms[i]}%");
            }
            
            // Join the search terms with AND, which makes it possible to search for multiple terms
            // However, this means that we can search for 'red plastic' but not 'red blue', since the box can't be both red and blue
            searchQuery = $"WHERE {string.Join(" AND ", searchCondition)}"; 
        }
        
        var filterQuery = "";
        if (boxParameters.GetFilters().Count > 0)  // If there are any filters, we need to add them to the SQL query
        {
            foreach (var (key, value) in boxParameters.GetFilters())
            {
                switch (key)
                {
                    case FilterTypes.Weight: // Numeric values should be passed in 'x-y' format, therefore we need to split the string and add a BETWEEN clause 
                    case FilterTypes.Price:
                    case FilterTypes.Stock:
                    case FilterTypes.Width:
                    case FilterTypes.Length:
                    case FilterTypes.Height:
                        filterQuery +=
                            $" AND {key.ToString().ToLower()} BETWEEN {value.Split('-')[0]} AND {value.Split('-')[1]}";
                        parameters.Add(value.Split('-')[0]);
                        parameters.Add(value.Split('-')[1]);
                        break;
                    case FilterTypes.Colour: // Colour and material are passed in 'x,y,z' format, therefore we need to split the string and add an IN clause
                    case FilterTypes.Material:
                        var values = value.Split(','); // Split the string into an array of values
                        var validValues = new List<string>(); // We need to make sure that the values are valid
                        foreach (var val in values)
                        {
                            if ((key == FilterTypes.Colour && _colours.Contains(val)) ||
                                (key == FilterTypes.Material && _materials.Contains(val)))
                                validValues.Add(val);
                        }
                        
                        // We need to wrap the values in single quotes to make them valid SQL strings
                        var filterValues = string.Join("','", validValues);
                        filterQuery += $" AND {key.ToString().ToLower()} IN ('{filterValues}')";
                        parameters.Add(filterValues);
                        Console.WriteLine(filterQuery);
                        break;
                    default:
                        throw new Exception("Invalid filter type");
                }
            }
            
            // If the search query is empty, we need to add the WHERE keyword
            // Otherwise, we need to remove the first AND keyword, since each filter is preceded by an AND
            filterQuery = string.IsNullOrWhiteSpace(searchQuery) ? $"WHERE {filterQuery[5..]}" : filterQuery;
        }
        
        searchQuery += filterQuery; // Append the filter query to the search query, to create one WHERE clause
        return new Dictionary<string, DynamicParameters> { { searchQuery, parameters } } ;
    }
}