using API.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Tests.API.Middleware;

public class SecurityHeadersMiddlewareTests
{
    private static DefaultHttpContext CreateHttpContext()
    {
        return new DefaultHttpContext();
    }

    private static SecurityHeadersMiddleware CreateMiddleware(RequestDelegate? next = null)
    {
        next ??= _ => Task.CompletedTask;
        return new SecurityHeadersMiddleware(next);
    }

    // ─────────────────────────────────────────────
    // Security headers are set
    // ─────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_SetsXContentTypeOptionsHeader()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers["X-Content-Type-Options"].ToString().Should().Be("nosniff");
    }

    [Fact]
    public async Task InvokeAsync_SetsXFrameOptionsHeader()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers["X-Frame-Options"].ToString().Should().Be("DENY");
    }

    [Fact]
    public async Task InvokeAsync_SetsXXssProtectionHeader()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers["X-XSS-Protection"].ToString().Should().Be("1; mode=block");
    }

    [Fact]
    public async Task InvokeAsync_SetsReferrerPolicyHeader()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers["Referrer-Policy"].ToString().Should().Be("strict-origin-when-cross-origin");
    }

    [Fact]
    public async Task InvokeAsync_SetsPermissionsPolicyHeader()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers["Permissions-Policy"].ToString()
            .Should().Be("geolocation=(), microphone=(), camera=()");
    }

    [Fact]
    public async Task InvokeAsync_SetsContentSecurityPolicyHeader()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        csp.Should().Contain("default-src 'self'");
        csp.Should().Contain("script-src 'self'");
        csp.Should().Contain("frame-ancestors 'none'");
        csp.Should().Contain("form-action 'self'");
    }

    // ─────────────────────────────────────────────
    // Sensitive headers are removed
    // ─────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_RemovesServerHeader()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.Headers["Server"] = "Kestrel";
        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.ContainsKey("Server").Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_RemovesXPoweredByHeader()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.Headers["X-Powered-By"] = "ASP.NET";
        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.ContainsKey("X-Powered-By").Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_RemovesXAspNetVersionHeader()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.Headers["X-AspNet-Version"] = "4.0";
        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.ContainsKey("X-AspNet-Version").Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_RemovesXAspNetMvcVersionHeader()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.Headers["X-AspNetMvc-Version"] = "5.0";
        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.ContainsKey("X-AspNetMvc-Version").Should().BeFalse();
    }

    // ─────────────────────────────────────────────
    // Middleware calls next
    // ─────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware()
    {
        // Arrange
        var context = CreateHttpContext();
        var nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_SecurityHeadersSetBeforeCallingNext()
    {
        // Arrange
        var context = CreateHttpContext();
        var headerSetBeforeNext = false;

        var middleware = CreateMiddleware(ctx =>
        {
            // Check that the header was already set when next is called
            headerSetBeforeNext = ctx.Response.Headers.ContainsKey("X-Content-Type-Options");
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        headerSetBeforeNext.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_PassesCorrectHttpContextToNext()
    {
        // Arrange
        var context = CreateHttpContext();
        HttpContext? capturedContext = null;

        var middleware = CreateMiddleware(ctx =>
        {
            capturedContext = ctx;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        capturedContext.Should().BeSameAs(context);
    }
}
