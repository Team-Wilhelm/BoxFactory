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
            var box = Helper.CreateBoxCreateDto(i, "red", "cardboard", i, i, i, i);
            expectedBoxes.Add(box);
            var sql = $@""; //TODO add sql
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
        var box = Helper.CreateBoxCreateDto(weight, colour, material, price, height, length, width);
        
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
        // Act

        var boxId = responseBox.Id;
        url = "http://localhost:ADD_ME"; //TODO add url with boxId
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