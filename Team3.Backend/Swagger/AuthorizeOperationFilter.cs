using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Team3.Backend.Swagger;

public sealed class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var endpointMetadata =
            context.ApiDescription.ActionDescriptor.EndpointMetadata;

        // Anonymous endpoints do not require authentication.
        if (endpointMetadata.OfType<IAllowAnonymous>().Any())
        {
            return;
        }

        var hasAuthorizeMetadata =
            endpointMetadata.OfType<IAuthorizeData>().Any();

        // Also check [Authorize] directly on the action and controller.
        if (context.ApiDescription.ActionDescriptor
            is ControllerActionDescriptor controllerActionDescriptor)
        {
            hasAuthorizeMetadata |=
                controllerActionDescriptor.MethodInfo
                    .GetCustomAttributes(typeof(IAuthorizeData), true)
                    .Any();

            hasAuthorizeMetadata |=
                controllerActionDescriptor.ControllerTypeInfo
                    .GetCustomAttributes(typeof(IAuthorizeData), true)
                    .Any();
        }

        if (!hasAuthorizeMetadata)
        {
            return;
        }

        operation.Security ??= [];

        operation.Security.Add(
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
            });
    }
}