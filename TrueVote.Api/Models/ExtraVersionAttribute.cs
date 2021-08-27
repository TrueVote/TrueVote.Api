using System;
using System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
[ExcludeFromCodeCoverage]
public sealed class ExtraVersionAttribute : Attribute
{
    public string ExtraVersion { get; }

    public ExtraVersionAttribute(string extraVersion) { ExtraVersion = extraVersion; }
}
