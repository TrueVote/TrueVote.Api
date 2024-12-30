using Microsoft.AspNetCore.Authorization;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using System.Diagnostics.CodeAnalysis;

namespace TrueVote.Api.Helpers;

[ExcludeFromCodeCoverage]
public class AuthorizeCheckOperationProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        // Check if the controller or the action has [Authorize] attribute
        var hasAuthorize = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
            .OfType<AuthorizeAttribute>().Any() ||
            context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

        if (hasAuthorize)
        {
            // Add 401 response
            if (!context.OperationDescription.Operation.Responses.ContainsKey("401"))
                context.OperationDescription.Operation.Responses.Add("401", new OpenApiResponse
                {
                    Description = "Unauthorized"
                });

            // Initialize security if null
            context.OperationDescription.Operation.Security ??= new List<OpenApiSecurityRequirement>();

            // Add security requirement
            var requirement = new OpenApiSecurityRequirement
            {
                { "Bearer", new string[0] }
            };
            context.OperationDescription.Operation.Security.Add(requirement);
        }

        return true;
    }
}
