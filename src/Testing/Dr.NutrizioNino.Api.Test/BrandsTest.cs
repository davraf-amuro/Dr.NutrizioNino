using Dr.NutrizioNino.Api.Dto;
using Dr.NutrizioNino.Models.Dto;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;

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
        var payload = response.Content.ReadAsStringAsync().Result;
        var apiResponse = JsonSerializer.Deserialize<ApiResponseMultipleDto<BrandDto>>(payload);

        Assert.True(apiResponse.Success == true);
    }
}
