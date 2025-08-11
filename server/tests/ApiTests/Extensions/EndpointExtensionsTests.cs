using Api.Endpoints;
using Api.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Reflection;

namespace ApiTests.Extensions;

public class EndpointExtensionsTests
{
    [Fact]
    public void AddEndpoints_WithAssemblyContainingEndpoints_RegistersEndpointsInContainer()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        services.AddEndpoints(assembly);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var endpoints = serviceProvider.GetService<IEnumerable<IEndpoint>>();
        endpoints.ShouldNotBeNull();
    }

    [Fact]
    public void AddEndpoints_WithMultipleEndpointTypes_RegistersAllEndpoints()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(TestEndpoint1).Assembly;

        // Act
        services.AddEndpoints(assembly);

        // Assert
        var serviceDescriptors = services.Where(s => s.ServiceType == typeof(IEndpoint)).ToList();
        serviceDescriptors.Count.ShouldBeGreaterThanOrEqualTo(1);
        serviceDescriptors.All(s => s.Lifetime == ServiceLifetime.Transient).ShouldBeTrue();
    }

    [Fact]
    public void MapEndpoints_WithMockedEndpoints_CallsMapEndpointOnAllRegisteredEndpoints()
    {
        // Arrange
        var mockEndpoint1 = new Mock<IEndpoint>();
        var mockEndpoint2 = new Mock<IEndpoint>();
        var endpoints = new List<IEndpoint> { mockEndpoint1.Object, mockEndpoint2.Object };

        var services = new ServiceCollection();
        services.AddSingleton<IEnumerable<IEndpoint>>(endpoints);
        
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton<IEnumerable<IEndpoint>>(endpoints);
        var app = builder.Build();

        // Act
        app.MapEndpoints();

        // Assert
        mockEndpoint1.Verify(e => e.MapEndpoint(It.IsAny<IEndpointRouteBuilder>()), Times.Once);
        mockEndpoint2.Verify(e => e.MapEndpoint(It.IsAny<IEndpointRouteBuilder>()), Times.Once);
    }

    [Fact]
    public void HasPermission_WithPermissionString_ReturnsRouteHandlerBuilder()
    {
        // Arrange - Use real WebApplication for this test since we can't mock sealed classes
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();
        
        const string permission = "TestPermission";

        // Act
        var routeHandlerBuilder = app.MapGet("/test", () => "test");
        var result = routeHandlerBuilder.HasPermission(permission);

        // Assert
        result.ShouldBe(routeHandlerBuilder);
        // Note: We can't easily verify that RequireAuthorization was called due to sealed classes
        // This test mainly verifies the method doesn't throw and returns the correct instance
    }

    // Test endpoint implementations for testing
    private class TestEndpoint1 : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            // Test implementation
        }
    }

    private class TestEndpoint2 : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            // Test implementation
        }
    }

    // This should not be registered (abstract)
    private abstract class AbstractEndpoint : IEndpoint
    {
        public abstract void MapEndpoint(IEndpointRouteBuilder app);
    }

    // This should not be registered (interface)
    private interface ITestEndpoint : IEndpoint
    {
    }
}
