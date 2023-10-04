using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Models;
using Newtonsoft.Json;

namespace Tests.BackendTests;

public class SearchTests
{
    private HttpClient _httpClient;

    [SetUp]
    public void Setup()
    {
        _httpClient = new HttpClient();
    }
    
    [Test]
    [TestCase("red", 1, 10)]
    public async Task SearchAllBoxesPaginatedSuccessfully(string searchTerm, int currentPage, int boxesPerPage)
    {
        // Arrange
        Helper.TriggerRebuild();
        for (int i = 0; i < 20; i++)
        {
            var box = Helper.CreateBoxCreateDto(i, "red", "cardboard", i, i, i, i, i);
            var sql = $@""; //TODO add sql
            await Helper.DbConnection.ExecuteAsync(sql, box);
        }
        
        // Act
        var url = $"http://localhost:!!!ADD:ME!!!!searchTerm={searchTerm}&currentPage={currentPage}&boxesPerPage={boxesPerPage}"; //TODO add url
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

        var content = await response.Content.ReadAsStringAsync();
        IEnumerable<Box> boxes;
        try
        {
            boxes = JsonConvert.DeserializeObject<IEnumerable<Box>>(content) ??
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
            var boxArray = boxes as Box[] ?? boxes.ToArray();
            boxArray.Length.Equals(10).Should().BeTrue();
            boxArray.Length.Equals(11).Should().BeFalse();
        }
    }

    [Test]
    [TestCase("non existent search term", 1, 10)]
    public async Task SearchAllBoxesPaginatedFailed(string searchTerm, int currentPage, int boxesPerPage)
    {
        // Arrange
        Helper.TriggerRebuild();
        for (int i = 0; i < 20; i++)
        {
            var box = Helper.CreateBoxCreateDto(i, "red", "cardboard", i, i, i, i, i);
            var sql = $@""; //TODO add sql
            await Helper.DbConnection.ExecuteAsync(sql, box);
        }
        
        // Act
        var url = $"http://localhost:!!!ADD:ME!!!!searchTerm={searchTerm}&currentPage={currentPage}&boxesPerPage={boxesPerPage}"; //TODO add url
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

        var content = await response.Content.ReadAsStringAsync();
        IEnumerable<Box> boxes;
        try
        {
            boxes = JsonConvert.DeserializeObject<IEnumerable<Box>>(content) ??
                    throw new InvalidOperationException();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message, e.InnerException);
        }

        // Assert
        using (new AssertionScope())
        {
            boxes.Should().BeEmpty();
        }
    }
    
}