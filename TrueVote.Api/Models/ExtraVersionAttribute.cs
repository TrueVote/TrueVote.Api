using System.Diagnostics.CodeAnalysis;

namespace TrueVote.Api.Models
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    [ExcludeFromCodeCoverage]
    public sealed class ExtraVersionAttribute : Attribute
    {
        public string ExtraVersion { get; }

        public ExtraVersionAttribute(string extraVersion)
        {
            ExtraVersion = extraVersion;
        }
    }
}
