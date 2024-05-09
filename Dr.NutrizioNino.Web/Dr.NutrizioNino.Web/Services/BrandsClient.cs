using Dr.NutrizioNino.Api.Dto;
using Dr.NutrizioNino.Models.Dto;
using System.Text.Json;

namespace Dr.NutrizioNino.Web.Services
{
    internal class BrandsClient(IHttpClientFactory clientFactory, ConfigurationService configuration)
    {
        internal async Task<ApiResponseDto<BrandDto>> GetBrandsAsync()
        {
            var uriRequest = new HttpRequestMessage(HttpMethod.Get, configuration.GetBaseUri() + "/brands");
            var client = clientFactory.CreateClient();
            var response = await client.SendAsync(uriRequest);
            ApiResponseDto<BrandDto>? result;

            if (response.IsSuccessStatusCode)
            {
                var responseStream = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                result = JsonSerializer.Deserialize<ApiResponseDto<BrandDto>>(responseStream
                    , new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            }
            else
            {
                result = new ApiResponseDto<BrandDto>();
                result.Success = false;
                result.Errors = new List<string> { response.ReasonPhrase };
            }

            return result;
        }
    }
}
