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
    [TestCase("cardboard", 1, 10)]
    [TestCase("plastic", 1, 10)]
    [TestCase("blue", 1, 10)]
    [TestCase("red plastic", 1, 10)]
    public async Task SearchAllBoxesPaginatedSuccessfully(string searchTerm, int currentPage, int boxesPerPage)
    {
        // Arrange
        Helper.TriggerRebuild();
        for (int i = 1; i <= 5; i++)
        {
            var boxDto = Helper.CreateBoxCreateDto(i, "red", "cardboard", i, i, i, i, i);
            await Helper.InsertBoxIntoDatabase(boxDto);
            
            boxDto = Helper.CreateBoxCreateDto(i, "blue", "plastic", i, i, i, i, i);
            await Helper.InsertBoxIntoDatabase(boxDto);
            
            boxDto = Helper.CreateBoxCreateDto(i, "red", "plastic", i, i, i, i, i);
            await Helper.InsertBoxIntoDatabase(boxDto);
            
            boxDto = Helper.CreateBoxCreateDto(i, "blue", "cardboard", i, i, i, i, i);
            await Helper.InsertBoxIntoDatabase(boxDto);
        }
        
        // Act
        var url = Helper.UrlBase + $"/box?searchTerm={searchTerm}&currentPage={currentPage}&boxesPerPage={boxesPerPage}";
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

        // Count the amount of boxes with the search term, which can be either in the colour or material column,
        // with multiple words, both words must be present in either the colour or material column.
        var enumerable = boxes.ToList();
        var searchWords = searchTerm.Split(' ');
        var count = enumerable.Count(box => searchWords.Any(word => box.Colour.Contains(word) || box.Material.Contains(word)));
                    
        // Assert
        using (new AssertionScope())
        {
            response.IsSuccessStatusCode.Should().BeTrue();
            var boxArray = boxes as Box[] ?? enumerable.ToArray();
            boxArray.Length.Equals(count).Should().BeTrue();
        }
    }

    [Test]
    [TestCase("non existent search term", 1, 10)]
    public async Task SearchAllBoxesPaginatedFailed(string searchTerm, int currentPage, int boxesPerPage)
    {
        // Arrange
        Helper.TriggerRebuild();
        for (int i = 1; i <= 20; i++)
        {
            var box = Helper.CreateBoxCreateDto(i, "red", "cardboard", i, i, i, i, i);
            await Helper.InsertBoxIntoDatabase(box);
        }
        
        // Act
        var url = Helper.UrlBase + $"/box?searchTerm={searchTerm}&currentPage={currentPage}&boxesPerPage={boxesPerPage}";
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