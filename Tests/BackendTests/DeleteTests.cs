using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Models;
using Newtonsoft.Json;

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
        var box = new BoxCreateDto()
        {
            Weight = weight,
            Colour = colour,
            Material = material,
            Price = price,
            Dimensions = new Dimensions()
            {
                Height = height,
                Length = length,
                Width = width
            }
        };
        
        var sql = $@""; //TODO add sql
        await using (var conn = await Helper.DataSource.OpenConnectionAsync())
        {
            await conn.ExecuteAsync(sql, box);
        }
        
        // Act
        var url = "http://localhost:ADD_ME"; //TODO add url
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
            await using (var conn = await Helper.DataSource.OpenConnectionAsync())
            {
                (conn.ExecuteScalar<int>($"SELECT COUNT(*) FROM testing WHERE boxId = 1;") == 0) //TODO add correct sql
                    .Should()
                    .BeTrue();
            }
            response.IsSuccessStatusCode.Should().BeTrue();
        }
    }
}