using API.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MediatR;
using NSubstitute;

namespace Tests.API.TestHelpers;

public static class ControllerTestHelper
{
    /// <summary>
    /// Creates a mock HttpContext with IMediator and ILogger services
    /// </summary>
    public static HttpContext CreateHttpContext(IMediator? mediator = null, ILogger<BaseApiController>? logger = null)
    {
        var httpContext = new DefaultHttpContext();

        var serviceCollection = new ServiceCollection();

        // Add mediator to services
        if (mediator != null)
        {
            serviceCollection.AddSingleton(mediator);
        }

        // Add logger to services
        if (logger != null)
        {
            serviceCollection.AddSingleton<ILogger<BaseApiController>>(logger);
        }
        else
        {
            serviceCollection.AddSingleton(Substitute.For<ILogger<BaseApiController>>());
        }

        httpContext.RequestServices = serviceCollection.BuildServiceProvider();

        return httpContext;
    }

    /// <summary>
    /// Sets up a controller with the specified HttpContext
    /// </summary>
    public static T SetupController<T>(T controller, HttpContext httpContext) where T : ControllerBase
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    /// <summary>
    /// Creates a controller with mocked dependencies
    /// </summary>
    public static T CreateController<T>(IMediator? mediator = null, ILogger<BaseApiController>? logger = null)
        where T : ControllerBase, new()
    {
        var httpContext = CreateHttpContext(mediator, logger);
        var controller = new T();
        return SetupController(controller, httpContext);
    }
}
