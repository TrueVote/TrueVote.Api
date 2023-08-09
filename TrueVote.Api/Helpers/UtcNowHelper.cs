using System;
using System.Diagnostics.CodeAnalysis;

namespace TrueVote.Api.Helpers
{
    public interface IUtcNowProvider
    {
        DateTime UtcNow { get; }
    }

    [ExcludeFromCodeCoverage]
    public class DefaultUtcNowProvider : IUtcNowProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }

    [ExcludeFromCodeCoverage]
    public static class UtcNowProviderFactory
    {
        private static IUtcNowProvider _UtcNowProvider;

        public static IUtcNowProvider GetProvider()
        {
            return _UtcNowProvider ??= new DefaultUtcNowProvider();
        }

        // Method for setting a custom DateTimeProvider during testing
        public static void SetProvider(IUtcNowProvider UtcNowProvider)
        {
            _UtcNowProvider = UtcNowProvider;
        }
    }
}
