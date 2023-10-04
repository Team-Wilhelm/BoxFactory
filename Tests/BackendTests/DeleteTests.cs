using Dapper;
using FluentAssertions;
using FluentAssertions.Execution;

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
        var box = await Helper.InsertBoxIntoDatabase(boxDto);
        
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
            response.IsSuccessStatusCode.Should().BeTrue();
            var boxAmount = await Helper.DbConnection.ExecuteScalarAsync<int>("SELECT * FROM testing.boxes WHERE box_id = @Id", new { box.Id });
            boxAmount.Should().Be(0);
        }
    }
}