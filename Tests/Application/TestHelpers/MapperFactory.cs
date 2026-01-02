using AutoMapper;
using Application.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Tests.Application.TestHelpers;

public static class MapperFactory
{
    /// <summary>
    /// Creates an AutoMapper instance configured with production profiles
    /// </summary>
    public static IMapper CreateMapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfiles>());
        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IMapper>();
    }
}
