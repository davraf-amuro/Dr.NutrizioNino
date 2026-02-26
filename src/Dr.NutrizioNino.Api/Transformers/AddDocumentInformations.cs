using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Dr.NutrizioNino.Api.Transformers;

public class AddDocumentInformations : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info.Title = "Example API";
        document.Info.Description = "template net9 mminimal api";
        document.Info.Version = "v1";
        document.Info.Contact = new OpenApiContact
        {
            Name = "davide 'davraf' raffagli",
            Url = new Uri("https://github.com/davraf-amuro"),
            Email = "d.raffagli@gmail.com"
        };
    }
}
