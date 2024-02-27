#pragma warning disable IDE0058 // Expression value is never used
namespace TrueVote.Api2
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                Console.WriteLine("Running Locally");
            }
            else
            {
                Console.WriteLine("Running Deployed");
            }

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(e => e.MapControllers());
        }
    }
}
#pragma warning restore IDE0058 // Expression value is never used
