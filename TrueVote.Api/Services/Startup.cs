using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Extensions.CosmosDB;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(TrueVote.Api.Services.Startup))]
namespace TrueVote.Api.Services
{
    // Modeled from here: https://github.com/Azure/azure-functions-openapi-extension/blob/main/docs/openapi-core.md#openapi-metadata-configuration
    // Overrides default OpenApi description and more
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
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.TryAddScoped<IFileSystem, FileSystem>();
            builder.Services.TryAddSingleton<ILoggerFactory, LoggerFactory>();

            builder.Services.AddSingleton((s) =>
            {
                var cosmosClientBuilder = new CosmosClientBuilder(Environment.GetEnvironmentVariable("CosmosDbConnectionString"));

                return cosmosClientBuilder.WithConnectionModeDirect()
                    .WithBulkExecution(true)
                    .Build();
            });

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
