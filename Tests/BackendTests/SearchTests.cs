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
    public async Task SearchBox()
    {
        // Search for boxes with parameters
        // Arrange
        Helper.TriggerRebuild();
        // Act
        
        // Assert

    }
}