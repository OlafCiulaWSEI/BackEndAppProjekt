using System.Net;
using System.Net.Http.Json;
using ApplicationCore.Models;
using ApplicationCore.Models.Filtering;
using ApplicationCore.Models.Sorting;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using WebApi;
using Xunit;

namespace Tests;

public class LegoSetControllerTests : IClassFixture<AppTestFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly AppTestFactory<Program> _factory;

    public LegoSetControllerTests(AppTestFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task GetAll_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/LegoSet");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<PagedResponse<LegoSet>>(content);
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task GetAll_WithPagination_ReturnsCorrectPageSize()
    {
        // Arrange
        int pageSize = 5;
        int pageNumber = 1;

        // Act
        var response = await _client.GetAsync($"/api/LegoSet?pageSize={pageSize}&pageNumber={pageNumber}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<PagedResponse<LegoSet>>(content);
        Assert.NotNull(result);
        Assert.True(result.Items.Count() <= pageSize);
        Assert.Equal(pageNumber, result.Metadata.CurrentPage);
    }

    [Fact]
    public async Task GetAll_WithFiltering_ReturnsFilteredResults()
    {
        // Arrange
        string theme = "Star Wars";

        // Act
        var response = await _client.GetAsync($"/api/LegoSet?theme={theme}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<PagedResponse<LegoSet>>(content);
        Assert.NotNull(result);
        Assert.All(result.Items, item => Assert.Equal(theme, item.Theme));
    }

    [Fact]
    public async Task GetAll_WithSorting_ReturnsOrderedResults()
    {
        // Arrange
        var sortBy = LegoSetSortingField.Price;
        var sortOrder = SortOrder.Descending;

        // Act
        var response = await _client.GetAsync($"/api/LegoSet?sortBy={sortBy}&sortOrder={sortOrder}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<PagedResponse<LegoSet>>(content);
        Assert.NotNull(result);
        
        // Verify ordering
        var prices = result.Items.Select(x => x.USRetailPrice).ToList();
        var sortedPrices = prices.OrderByDescending(x => x).ToList();
        Assert.Equal(sortedPrices, prices);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsLegoSet()
    {
        // Arrange
        var getAllResponse = await _client.GetAsync("/api/LegoSet");
        var content = await getAllResponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<PagedResponse<LegoSet>>(content);
        var firstLegoSet = result?.Items.FirstOrDefault();
        Assert.NotNull(firstLegoSet);

        // Act
        var response = await _client.GetAsync($"/api/LegoSet/{firstLegoSet.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var legoSetContent = await response.Content.ReadAsStringAsync();
        var legoSet = JsonConvert.DeserializeObject<LegoSet>(legoSetContent);
        Assert.NotNull(legoSet);
        Assert.Equal(firstLegoSet.Id, legoSet.Id);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/LegoSet/nonexistingid");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithComplexFiltering_ReturnsFilteredResults()
    {
        // Arrange
        int minPieces = 500;
        decimal minPrice = 50;
        string theme = "Star Wars";

        // Act
        var response = await _client.GetAsync($"/api/LegoSet?minPieces={minPieces}&minPrice={minPrice}&theme={theme}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<PagedResponse<LegoSet>>(content);
        Assert.NotNull(result);
        
        foreach (var legoSet in result.Items)
        {
            Assert.Equal(theme, legoSet.Theme);
            Assert.True(legoSet.Pieces >= minPieces);
            Assert.True(legoSet.USRetailPrice >= minPrice);
        }
    }

    [Fact]
    public async Task GetAll_WithInvalidPagination_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/LegoSet?pageNumber=0&pageSize=0");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetThemes_ReturnsUniqueThemes()
    {
        // Act
        var response = await _client.GetAsync("/api/LegoSet/themes");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var themes = JsonConvert.DeserializeObject<List<string>>(content);
        Assert.NotNull(themes);
        Assert.Equal(themes.Count, themes.Distinct().Count());
    }

    [Fact]
    public async Task GetYears_ReturnsOrderedYears()
    {
        // Act
        var response = await _client.GetAsync("/api/LegoSet/years");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var years = JsonConvert.DeserializeObject<List<int>>(content);
        Assert.NotNull(years);
        Assert.Equal(years, years.OrderBy(y => y).ToList());
    }
} 