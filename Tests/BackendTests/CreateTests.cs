using System.Net;
using System.Net.Http.Json;
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
    [TestCase(20, "red", "cardboard", 100, 10, 20, 20, 10)]
    public async Task CreateBoxSuccessfully(float weight, string colour, string material, float price, int stock, float height, float length, float width)
    {
        // Arrange
        Helper.TriggerRebuild();
        var box = Helper.CreateBoxCreateDto(weight, colour, material, price, stock, height, length, width);
        
        var url = Helper.UrlBase + "/box";
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
            
            box.Should().BeEquivalentTo(responseBox, options =>
            {
                return options
                    .Excluding(b => b.Id)
                    .Excluding(b => b.Dimensions)
                    .Excluding(b => b.CreatedAt);
            });
            
            box.DimensionsDto.Length.Should().Be(responseBox.Dimensions.Length);
            box.DimensionsDto.Width.Should().Be(responseBox.Dimensions.Width);
            box.DimensionsDto.Height.Should().Be(responseBox.Dimensions.Height);
        }
    }

    [Test]
    [TestCase(20, "red", "cardboard", 9, -5,20, 20, 10)]
    [TestCase(20, "red", "cardboard", 9, 5, -20, 20, 10)]
    [TestCase(20, "red", "cardboard", 9, 5, 20, -20, 10)]
    [TestCase(20, "red", "cardboard", 9, 5, 20, 20, -10)]
    [TestCase(20, "red", "cardboard", -9, 5, 20, 20, 10)]
    [TestCase(-20, "red", "cardboard", 9, 5, 20, 20, 10)]
    public async Task CreateBoxBadData(float weight, string colour, string material, float price, int stock, float height, float length, float width)
    {
        // Arrange
        Helper.TriggerRebuild();
        var box = Helper.CreateBoxCreateDto(weight, colour, material, price, stock, height, length, width);
        var url = Helper.UrlBase + "/box";
        
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
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Content.ReadAsStringAsync().Result.Should().Contain("One or more validation errors occurred.");
        }
    }
}