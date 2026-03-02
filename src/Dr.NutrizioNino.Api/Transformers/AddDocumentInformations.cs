using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Dr.NutrizioNino.Api.Transformers;

public class AddDocumentInformations : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info.Title = "Dr.NutrizioNino API";
        document.Info.Description = "Minimal API per gestione alimenti, nutrienti, brand e unita di misura.";
        document.Info.Version = "v1";
        document.Info.Contact = new OpenApiContact
        {
            Name = "Dr.NutrizioNino Team",
            Url = new Uri("https://github.com/davraf-amuro"),
            Email = "d.raffagli@gmail.com"
        };

        return Task.CompletedTask;
    }
}
