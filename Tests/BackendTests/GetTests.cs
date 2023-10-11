using FluentAssertions;
using FluentAssertions.Execution;
using Models.Models;
using Newtonsoft.Json;

namespace Tests.BackendTests;

public class GetTests
{
    private readonly HttpClient _httpClient = new();

    [Test]
    public async Task GetAllBoxes()
    {
        // Arrange
        Helper.TriggerRebuild();
        var expectedBoxes = new List<Box>();
        for (int i = 1; i < 10; i++)
        {
            var boxDto = Helper.CreateBoxCreateDto(i, "red", "cardboard", i, i, i, i, i);
            var box = await Helper.InsertBoxIntoDatabase(boxDto);
            expectedBoxes.Add(box);
        }
        
        // Act
        var url = Helper.UrlBase + "/box";
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
            response.IsSuccessStatusCode.Should().BeTrue();
            expectedBoxes.Should().BeEquivalentTo(dbBoxes.ToList());
        }
    }
    
    
    [Test]
    [TestCase(20, "red", "cardboard", 9, 20, 20, 10)]
    public async Task GetBoxById(float weight, string colour, string material, float price, float height, float length, float width)
    {
        // Arrange
        Helper.TriggerRebuild();
        var boxDto = Helper.CreateBoxCreateDto(weight, colour, material, price, 1, height, length, width);
        var box = await Helper.InsertBoxIntoDatabase(boxDto);

        var url = Helper.UrlBase + $"/box/{box.Id}";
        
        // Act
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
        
        Box dbBox;
        try
        {
            dbBox = JsonConvert.DeserializeObject<Box>(
                await response.Content.ReadAsStringAsync()) ?? throw new InvalidOperationException();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message, e.InnerException);
        }
        
        // Assert
        box.Should().BeEquivalentTo(dbBox, options => options.Excluding(b => b.Id));
    }
}