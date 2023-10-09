using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Models;
using Models.DTOs;
using Newtonsoft.Json;

namespace Tests.BackendTests;

public class  UpdateTests
{
    private HttpClient _httpClient;

    [SetUp]
    public void Setup()
    {
        _httpClient = new HttpClient();
    }

    [Test]
    [TestCase(30, "blue", "plastic", 100, 200, 200, 200, 200)]
    public async Task UpdateBoxSuccessfully(float weight, string colour, string material, float price, int stock,
        float height, float length, float width)
    {
        // Arrange
        Helper.TriggerRebuild();
        var box = await Helper.GetValidBoxFromDatabase();
        var url = Helper.UrlBase + "/box/" + box.Id;

        var updateBoxDto = new BoxUpdateDto()
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
            },
        };

        // Act
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PutAsJsonAsync(url, updateBoxDto);
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

            updateBoxDto.Should().BeEquivalentTo(responseBox, options =>
            {
                return options
                    .Excluding(b => b.Id)
                    .Excluding(b => b.Dimensions)
                    .Excluding(b => b.CreatedAt);
            });

            updateBoxDto.DimensionsDto.Length.Should().Be(responseBox.Dimensions.Length);
            updateBoxDto.DimensionsDto.Width.Should().Be(responseBox.Dimensions.Width);
            updateBoxDto.DimensionsDto.Height.Should().Be(responseBox.Dimensions.Height);
        }
    }

    [Test]
    [TestCase(-30, "blue", "plastic", -200, -10, -20, -20, -10)]
    public async Task UpdateBoxFailed(float weight, string colour, string material, float price, int stock,
        float height, float length, float width)
    {
        // Arrange
        Helper.TriggerRebuild();
        var box = await Helper.GetValidBoxFromDatabase();
        var url = Helper.UrlBase + "/box/" + box.Id;

        var updateBoxDto = new BoxUpdateDto()
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
            },
        };
        
        // Act
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PutAsJsonAsync(url, updateBoxDto);
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