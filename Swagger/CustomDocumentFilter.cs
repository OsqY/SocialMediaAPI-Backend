using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SocialMediaAPI.Swagger;

public class CustomDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        document.Info.Title = "SocialMediaAPI";
    }
}
