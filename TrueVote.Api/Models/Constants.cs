namespace TrueVote.Api.Models
{
    public static class Constants
    {
        public const string EMailRegex = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";

        // TODO Make this restriction more loose, to include other types of characters
        public const string GenericStringRegex = @".";
    }
}
