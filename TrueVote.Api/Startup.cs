using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using TrueVote.Api.Models;
using TrueVote.Api.Services;

[assembly: FunctionsStartup(typeof(TrueVote.Api.Startup))]
namespace TrueVote.Api
{
    // Modeled from here: https://github.com/Azure/azure-functions-openapi-extension/blob/main/docs/openapi-core.md#openapi-metadata-configuration
    // Overrides default OpenApi description and more
    // TODO Once this feature gets implemented and released: https://github.com/Azure/azure-functions-openapi-extension/issues/400
    // Add custom filter for enums exactly like this: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1387#issuecomment-582316007
    // Jira: https://truevote.atlassian.net/browse/AD-32
    [ExcludeFromCodeCoverage]
    public class OpenApiConfigurationOptions : IOpenApiConfigurationOptions
    {
        public OpenApiInfo Info { get; set; } = new OpenApiInfo()
        {
            Version = "1.0.0",
            Title = "TrueVote.Api",
            Description = "TrueVote APIs that run as serverless functions using OpenAPI specification.",
            TermsOfService = new Uri("https://truevote.org/terms"),
            Contact = new OpenApiContact()
            {
                Name = "TrueVote IT",
                Email = "info@truevote.org",
                Url = new Uri("https://github.com/TrueVote/TrueVote.Api/issues")
            },
            License = new OpenApiLicense()
            {
                Name = "MIT License",
                Url = new Uri("https://raw.githubusercontent.com/TrueVote/TrueVote.Api/master/LICENSE")
            }
        };

        public List<OpenApiServer> Servers { get; set; } = new List<OpenApiServer>();
        public OpenApiVersionType OpenApiVersion { get; set; } = OpenApiVersionType.V3;
        public bool IncludeRequestingHostName { get; set; } = true;
        public bool ForceHttp { get; set; } = false;
        public bool ForceHttps { get; set; } = false;
        public List<IDocumentFilter> DocumentFilters { get; set; } = new List<IDocumentFilter>();
    }

    [ExcludeFromCodeCoverage]
    public class OpenApiCustomUIOptions : DefaultOpenApiCustomUIOptions
    {
        public OpenApiCustomUIOptions(Assembly assembly) : base(assembly)
        {
        }

        public override string CustomStylesheetPath { get; } = "dist.truevote-api.css";
        public override string CustomJavaScriptPath { get; } = "dist.truevote-api.js";

        public override async Task<string> GetStylesheetAsync()
        {
            return await base.GetStylesheetAsync();
        }

        public override async Task<string> GetJavaScriptAsync()
        {
            return await base.GetJavaScriptAsync();
        }
    }

    [ExcludeFromCodeCoverage]
    public class TrueVoteDbContext : DbContext
    {
        public virtual DbSet<UserModel> Users { get; set; }
        public virtual DbSet<ElectionModel> Elections { get; set; }
        public virtual DbSet<RaceModel> Races { get; set; }
        public virtual DbSet<CandidateModel> Candidates { get; set; }

        public virtual async Task<bool> EnsureCreatedAsync()
        {
            return await Database.EnsureCreatedAsync();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseCosmos(Environment.GetEnvironmentVariable("CosmosDbConnectionString"), "true-vote");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultContainer("Users");
            modelBuilder.Entity<UserModel>().ToContainer("Users");
            modelBuilder.Entity<UserModel>().HasNoDiscriminator();

            modelBuilder.HasDefaultContainer("Elections");
            modelBuilder.Entity<ElectionModel>().ToContainer("Elections");
            modelBuilder.Entity<ElectionModel>().HasNoDiscriminator();

            modelBuilder.HasDefaultContainer("Races");
            modelBuilder.Entity<RaceModel>().ToContainer("Races");
            modelBuilder.Entity<RaceModel>().HasNoDiscriminator();
            modelBuilder.Entity<RaceModel>().Property(p => p.Candidates)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions) null),
                    v => JsonSerializer.Deserialize<List<CandidateModel>>(v, (JsonSerializerOptions) null),
                    new ValueComparer<ICollection<CandidateModel>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));

            modelBuilder.HasDefaultContainer("Candidates");
            modelBuilder.Entity<CandidateModel>().ToContainer("Candidates");
            modelBuilder.Entity<CandidateModel>().HasNoDiscriminator();
        }
    }

    [ExcludeFromCodeCoverage]
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddDbContext<TrueVoteDbContext>();
            builder.Services.TryAddScoped<IFileSystem, FileSystem>();
            builder.Services.TryAddSingleton<ILoggerFactory, LoggerFactory>();
            builder.Services.TryAddSingleton<TelegramBot, TelegramBot>();

            builder.AddGraphQLFunction().AddQueryType<Query>();

            ConfigureServices(builder.Services).BuildServiceProvider(true);
        }

        private IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddSingleton(typeof(ILogger), typeof(Logger<Startup>));

            return services;
        }
    }
}
