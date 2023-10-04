using Dapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Models;

namespace Tests.BackendTests;

public class DeleteTests
{
    private HttpClient _httpClient;

    [SetUp]
    public void Setup()
    {
        _httpClient = new HttpClient();
    }
    
    [Test]
    [TestCase(20, "red", "cardboard", 9, 20, 20, 10)]
    public async Task DeleteBox(float weight, string colour, string material, float price, float height, float length, float width)
    {
        // Arrange
        Helper.TriggerRebuild();
        var boxDto = Helper.CreateBoxCreateDto(weight, colour, material, price, 1, height, length, width);
        
        // Insert dimensions
        var sql = $@"INSERT INTO testing.dimensions (length, width, height) 
                        VALUES (@Length, @Width, @Height) 
                        RETURNING 
                            dimensions_id AS {nameof(Dimensions.Id)},
                            length AS {nameof(Dimensions.Length)},
                            height AS {nameof(Dimensions.Height)},
                            width AS {nameof(Dimensions.Width)}";
        var dimensions = await Helper.DbConnection.QuerySingleAsync<Dimensions>(sql, boxDto.DimensionsDto);

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
        var box = await Helper.DbConnection.QuerySingleAsync<Box>(sql, new
        {
            boxDto.Weight,
            boxDto.Colour,
            boxDto.Material,
            boxDto.Price,
            boxDto.Stock,
            DimensionsId = dimensions.Id
        });
        box.Dimensions = dimensions;
        
        // Act
        var url = Helper.UrlBase + $"/box/{box.Id}";
        HttpResponseMessage response; 
        try
        {
            response = await _httpClient.DeleteAsync(url);
            TestContext.WriteLine("THE FULL BODY RESPONSE: " + await response.Content.ReadAsStringAsync());
        }
        catch (Exception e)
        {
            throw new Exception(e.Message, e.InnerException);
        }
        
        // Assert
        using (new AssertionScope())
        {
            sql = $"SELECT * FROM testing.boxes WHERE box_id = '{box.Id}';";
            await Helper.DbConnection.ExecuteAsync(sql, box.Id);
            response.IsSuccessStatusCode.Should().BeTrue();
        }
    }
}