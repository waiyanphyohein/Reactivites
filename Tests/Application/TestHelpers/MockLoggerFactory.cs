using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Tests.Application.TestHelpers;

public static class MockLoggerFactory
{
    public static ILogger<T> CreateLogger<T>()
    {
        var logger = Substitute.For<ILogger<T>>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        return logger;
    }
}
