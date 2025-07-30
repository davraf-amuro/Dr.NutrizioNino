using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Dr.NutrizioNino.Api.Transformers;

public class AddHeaders : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (operation.Parameters == null) operation.Parameters = new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "internal-authorization",
            In = ParameterLocation.Header,
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "String"
            }
        });

        return Task.CompletedTask;
    }
}

