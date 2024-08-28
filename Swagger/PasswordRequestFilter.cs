using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SocialMediaAPI.Swagger;

public class PasswordRequestFilter : IRequestBodyFilter
{
    public void Apply(OpenApiRequestBody requestBody, RequestBodyFilterContext context)
    {
        string fieldName = "password";

        if (
            context.BodyParameterDescription.Name.Equals(
                fieldName,
                StringComparison.OrdinalIgnoreCase
            )
            || context
                .BodyParameterDescription.Type.GetProperties()
                .Any(p => p.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
        )
        {
            requestBody.Description = "Use a strong password, and make sure to remember it!";
        }
    }
}
