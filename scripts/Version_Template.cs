namespace TrueVote.Api
{
    public static class VersionInfo
    {
        public static string BuildInfo = @"
        {
          ""Branch"": ""{{branch}}"",
          ""BuildTime"": ""{{buildtime}}"",
          ""LastTag"": ""{{lasttag}}"",
          ""Commit"": ""{{commit}}""
        }";
    }
}
