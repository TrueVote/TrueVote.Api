using System.Diagnostics.CodeAnalysis;

namespace TrueVote.Api.Helpers
{
    [ExcludeFromCodeCoverage]
    public static class GuidProviderFactory
    {
        private static IGuidProvider? _guidProvider;

        public static IGuidProvider GetProvider()
        {
            return _guidProvider ??= new DefaultGuidProvider();
        }

        // Method for setting a custom DateTimeProvider during testing
        public static void SetProvider(IGuidProvider guidProvider)
        {
            _guidProvider = guidProvider;
        }
    }

    public interface IGuidProvider
    {
        Guid NewGuid { get; }
    }

    [ExcludeFromCodeCoverage]
    public class DefaultGuidProvider : IGuidProvider
    {
        public Guid NewGuid => Guid.NewGuid();
    }
}
