/*
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Authorization;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Routing;
using System.Linq;

// Use this for testing [Authorize]
// Since this would end up testing "Microsoft's" code, not really needed here. It was
// built as a prototype
//
public static class ControllerTestExtensions
{
    public static void SetupController(this ControllerBase controller, string userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, "Voter")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    public static void SetupControllerForAuth(this ControllerBase controller, string methodName, bool isAuthenticated = true, string userId = "test-user", string role = "Voter")
    {
        // Set up services
        var services = new ServiceCollection();
        services.AddAuthorization();
        services.AddLogging();

        // Add authorization service
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync((ClaimsPrincipal user, object resource, IEnumerable<IAuthorizationRequirement> requirements) =>
            {
                return isAuthenticated
                    ? AuthorizationResult.Success()
                    : AuthorizationResult.Failed();
            });

        services.AddSingleton(authorizationService.Object);

        var serviceProvider = services.BuildServiceProvider();

        // Create HTTP context with services
        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };

        if (isAuthenticated)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            httpContext.User = principal;
        }

        var methodInfo = controller.GetType().GetMethod(methodName)
            ?? throw new ArgumentException($"Method {methodName} not found on controller {controller.GetType().Name}");

        // Create proper ControllerActionDescriptor
        var controllerActionDescriptor = new ControllerActionDescriptor
        {
            ControllerName = controller.GetType().Name.Replace("Controller", ""),
            ControllerTypeInfo = controller.GetType().GetTypeInfo(),
            ActionName = methodName,
            MethodInfo = methodInfo,
            DisplayName = $"{controller.GetType().Name}.{methodName}"
        };

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            controllerActionDescriptor);

        controller.ControllerContext = new ControllerContext(actionContext);

        // Get the Authorize attribute and create a policy

        if (methodInfo.GetCustomAttributes(typeof(AuthorizeAttribute), true)
                                        .FirstOrDefault() is AuthorizeAttribute authorizeAttribute)
        {
            var policy = new AuthorizationPolicy(
                new[] { new DenyAnonymousAuthorizationRequirement() },
                new[] { JwtBearerDefaults.AuthenticationScheme });

            var authorizeFilter = new AuthorizeFilter(policy);
            var authContext = new AuthorizationFilterContext(
                actionContext,
                new List<IFilterMetadata> { authorizeFilter });

            if (!isAuthenticated)
            {
                authContext.Result = new UnauthorizedResult();
            }

            if (authContext.Result != null)
            {
                httpContext.Items["AuthorizationResult"] = authContext.Result;
            }
        }
    }

    public static async Task<IActionResult> ExecuteWithAuth<T>(this T controller, Func<Task<IActionResult>> action) where T : ControllerBase
    {
        // Check if we have a stored authorization result
        if (controller.HttpContext?.Items["AuthorizationResult"] is IActionResult authResult)
        {
            return authResult;
        }

        // Execute the actual action if authorization passed
        return await action();
    }
}
*/
