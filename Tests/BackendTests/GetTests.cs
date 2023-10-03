using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Models;
using Newtonsoft.Json;

namespace Tests.BackendTests;

public class GetTests
{
    private HttpClient _httpClient;

    [SetUp]
    public void Setup()
    {
        _httpClient = new HttpClient();
    }
    
     [Test]
    public async Task GetAllBoxes()
    {
        // Arrange
        Helper.TriggerRebuild();
        var expectedBoxes = new List<BoxCreateDto>();
        for (int i = 0; i < 10; i++)
        {
            var box = new BoxCreateDto()
            {
                Weight = i+1,
                Colour = "red",
                Material = "cardboard",
                Price = i+100,
                Dimensions = new Dimensions()
                {
                    Height = i+20,
                    Length = i+20,
                    Width = i+10
                }
            };
            expectedBoxes.Add(box);
            var sql = $@" 
            insert into testing.Boxes () VALUES ();"; //TODO add sql
            await using var conn = await Helper.DataSource.OpenConnectionAsync();
            await conn.ExecuteAsync(sql, box);
        }
        
        // Act
        var url = "http://localhost:ADD_ME"; //TODO add url
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(url);
            TestContext.WriteLine("THE FULL BODY RESPONSE: " + await response.Content.ReadAsStringAsync());

        }
        catch (Exception e)
        {
            throw new Exception(e.Message, e.InnerException);
        }


        IEnumerable<Box> dbBoxes;
        try
        {
            dbBoxes = JsonConvert.DeserializeObject<IEnumerable<Box>>(await response.Content.ReadAsStringAsync()) ??
                      throw new InvalidOperationException();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message, e.InnerException);
        }
        
        // Assert
        using (new AssertionScope())
        {
            var dbBoxList = dbBoxes.ToList();
            foreach (var box in dbBoxList.ToList())
            {
                response.IsSuccessStatusCode.Should().BeTrue();
            }
            expectedBoxes.Should().BeEquivalentTo(dbBoxList, options => options.Excluding(b => b.Id));
        }
    }
    
    [Test]
    [TestCase(20, "red", "cardboard", 9, 20, 20, 10)]
    public async Task GetBoxById(float weight, string colour, string material, float price, float height, float length, float width)
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
        // Act
        
        // Assert

    }

    [Test]
    public async Task GetAllBoxesPaginated()
    {
        // Add large number of boxes
        //check if pagination limit works
        // Arrange
        Helper.TriggerRebuild();
        // Act
        
        // Assert

    }
    
}