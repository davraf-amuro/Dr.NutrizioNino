using System.Text.Json;
using Dr.NutrizioNino.Models.Dto;
using Microsoft.AspNetCore.Mvc.Testing;

public class BrandsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public BrandsEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_All()
    {
        var response = await _client.GetAsync("https://localhost:44360/brands");
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<List<BrandDto>>(payload);

        Assert.NotNull(apiResponse);
    }
}
