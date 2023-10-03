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
    public async Task UpdateBoxSuccessfully()
    {
        // Arrange

        // Act
        
        // Assert
        throw new NotImplementedException();

    }
    
    [Test]
    public async Task UpdateBoxFailed()
    {
        // Arrange
        
        // Act
        
        // Assert
        throw new NotImplementedException();
        
    }
}