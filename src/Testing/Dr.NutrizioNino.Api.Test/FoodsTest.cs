using Dr.NutrizioNino.Api.Dto;
using Dr.NutrizioNino.Models.Dto;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;

public class FoodsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public FoodsEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_Food()
    {
        //var foodDto = new CreateFoodDto()
        //{
        //    Name = "Test",
        //    Barcode = null,
        //    BrandId = "38153233-449B-4DCD-8188-69A47DEBA0FD",
        //    Calorie = 263
        //};
        //_client.PostAsync("https://localhost:44360/brands", );
    }
}
