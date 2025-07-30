using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Dr.NutrizioNino.Api.Transformers
{
    public class AddGenericsInformations : IOpenApiOperationTransformer
    {
        public async Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
        {
            if (operation.OperationId == "generics.GetCoverage")
            {
                foreach (var parameter in operation.Parameters)
                {
                    if (parameter.Name == "CivicEgon") parameter.Description = "";
                }
            }
        }
    }
}
