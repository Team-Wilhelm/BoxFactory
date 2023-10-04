using System.Net;
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
    public async Task DeleteBoxSuccess()
    {
        // Arrange
        Helper.TriggerRebuild();
        var box = await Helper.GetValidBoxFromDatabase();
        
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

    [Test]
    public async Task DeleteBoxFail()
    {
        // Arrange
        Helper.TriggerRebuild();
        
        // Act
        var url = Helper.UrlBase + $"/box/{Guid.NewGuid()}";
        
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
            response.IsSuccessStatusCode.Should().BeFalse();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}