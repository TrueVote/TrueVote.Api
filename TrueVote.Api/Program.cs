using System.Diagnostics.CodeAnalysis;

namespace TrueVote.Api
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(o =>
                {
                    o.UseStartup<Startup>();
                });
        }
    }
}
