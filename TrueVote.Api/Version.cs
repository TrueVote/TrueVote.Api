/*
 * This is a static template class as part of the CI/CD process. build-version.sh overwrites the values with current build info.
 * Version.cs is in .gitignore. To change this file, be sure and remove it from .gitignore, change it,
 * commit it, and then put the .gitignore entry back.
 */
namespace TrueVote.Api
{
    public static class VersionInfo
    {
        public static string BuildInfo = @"
        {
          ""Branch"": ""master"",
          ""BuildTime"": ""Thursday, Aug 26, 2021 18:16:27"",
          ""LastTag"": ""1.0.0"",
          ""Commit"": ""b00224c3f849f8f6cb4bf434e10bd571529621c6""
        }";
    }
}
