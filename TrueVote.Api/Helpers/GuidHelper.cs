using System.Diagnostics.CodeAnalysis;

namespace TrueVote.Api.Helpers
{
    public interface IGuidProvider
    {
        Guid NewGuid { get; }
    }

    [ExcludeFromCodeCoverage]
    public class DefaultGuidProvider : IGuidProvider
    {
        public Guid NewGuid => Guid.NewGuid();
    }

    [ExcludeFromCodeCoverage]
    public static class GuidProviderFactory
    {
        private static IGuidProvider? _GuidProvider;

        public static IGuidProvider GetProvider()
        {
            return _GuidProvider ??= new DefaultGuidProvider();
        }

        // Method for setting a custom DateTimeProvider during testing
        public static void SetProvider(IGuidProvider GuidProvider)
        {
            _GuidProvider = GuidProvider;
        }
    }
}
