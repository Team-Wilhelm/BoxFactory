using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Models;
using Newtonsoft.Json;

namespace Tests.BackendTests;

public class UpdateTests
{
    private HttpClient _httpClient;

    [SetUp]
    public void Setup()
    {
        _httpClient = new HttpClient();
    }

    [Test]
    [TestCase(20, "red", "cardboard", 100, 10, 20, 20, 10)]
    public async Task UpdateBoxSuccessfully(float weight, string colour, string material, float price, int stock,
        float height, float length, float width)
    {
        // Arrange
        Helper.TriggerRebuild();
        var boxDto = Helper.CreateBoxCreateDto(weight, colour, material, price, stock, height, length, width);
        var box = await Helper.InsertBoxIntoDatabase(boxDto);
        var url = Helper.UrlBase + "/box/" + box.Id;

        var updateBoxDto = new BoxUpdateDto()
        {
            Weight = 30,
            Colour = "blue",
            Material = "plastic",
            Price = 200,
            Stock = 20,
            DimensionsDto = new DimensionsDto()
            {
                Height = 30,
                Length = 30,
                Width = 30
            }
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
    [TestCase(20, "red", "cardboard", 100, 10, 20, 20, 10)]
    public async Task UpdateBoxFailed(float weight, string colour, string material, float price, int stock,
        float height, float length, float width)
    {
        // Arrange
        Helper.TriggerRebuild();
        var boxDto = Helper.CreateBoxCreateDto(weight, colour, material, price, stock, height, length, width);
        var box = await Helper.InsertBoxIntoDatabase(boxDto);
        var url = Helper.UrlBase + "/box/" + box.Id;

        var updateBoxDto = new BoxUpdateDto()
        {
            Weight = -30,
            Colour = "blue",
            Material = "plastic",
            Price = -200,
            Stock = -20,
            DimensionsDto = new DimensionsDto()
            {
                Height = -30,
                Length = -30,
                Width = -30
            }
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