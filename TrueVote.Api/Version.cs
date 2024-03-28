namespace TrueVote.Api
{
    public static class VersionInfo
    {
        public static readonly string BuildInfo = @"
        {
          ""Branch"": ""{{branch}}"",
          ""BuildTime"": ""{{buildtime}}"",
          ""LastTag"": ""{{lasttag}}"",
          ""Commit"": ""{{commit}}""
        }";
    }
}
