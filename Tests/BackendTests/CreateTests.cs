using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Models;
using Newtonsoft.Json;

namespace Tests.BackendTests;

public class CreateTests
{
    private HttpClient _httpClient;

    [SetUp]
    public void Setup()
    {
        _httpClient = new HttpClient();
    }

    [Test]
    [TestCase(20, "red", "cardboard", 100, 20, 20, 10)]
    public async Task CreateBoxSuccessfully(float weight, string colour, string material, float price, float height, float length, float width)
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
        var url = "http://localhost:ADD_ME"; //TODO add url
        // Act
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync(url, box);
            TestContext.WriteLine("THE FULL BODY RESPONSE: " + await response.Content.ReadAsStringAsync());
        }
        catch (Exception e)
        {
            throw new Exception(e.Message, e.InnerException);
        }
        
        Box responseBox;
        try
        {
            responseBox = JsonConvert.DeserializeObject<Box>(
                await response.Content.ReadAsStringAsync()) ?? throw new InvalidOperationException();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message, e.InnerException);
        }
        
        // Assert
        using (new AssertionScope())
        {
            response.IsSuccessStatusCode.Should().BeTrue();
            box.Should().BeEquivalentTo(responseBox, options => options.Excluding(b => b.Id));
        }
    }

    [Test]
    [TestCase(20, "red", "cardboard", 9, 20, 20, 10)] //TODO Add bad data here
    public async Task CreateBoxBadData(float weight, string colour, string material, float price, float height, float length, float width)
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
        var url = "http://localhost:ADD_ME"; //TODO add url
        
        // Act
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync(url, box);
            TestContext.WriteLine("THE FULL BODY RESPONSE: " + await response.Content.ReadAsStringAsync());
        }
        catch (Exception e)
        {
            throw new Exception(e.Message, e.InnerException);
        }
        
        // Assert
        using (new AssertionScope())
        {
            response.IsSuccessStatusCode.Should().BeFalse();
        }
    }
}