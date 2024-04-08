using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Reflection;
using System.Text.Json;
using HotChocolate.Types.Descriptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TrueVote.Api.Helpers;
using TrueVote.Api.Interfaces;
using TrueVote.Api.Models;
using TrueVote.Api.Services;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Path = System.IO.Path;

namespace TrueVote.Api
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public string CustomStylesheetPath { get; set; } = "/dist/truevote-api.css";

        public string CustomJavaScriptPath { get; set; } = "/dist/truevote-api.js";

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson(o =>
            {
                o.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.IsoDateTimeConverter());
                o.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                o.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                o.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(o =>
            {
                var baseUrl = "/api";
                o.AddServer(new OpenApiServer
                {
                    Url = baseUrl
                });
                o.EnableAnnotations();
                o.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Version = "1.0.0",
                    Title = "TrueVote.Api",
                    Description = "TrueVote APIs using strict OpenAPI specification.",
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
                });
            });

            services.AddApplicationInsightsTelemetry();
            services.AddDbContext<ITrueVoteDbContext, TrueVoteDbContext>();
            services.TryAddScoped<IFileSystem, FileSystem>();
            services.TryAddScoped<IServiceBus, ServiceBus>();
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug)
                       .AddProvider(new CustomLoggerProvider(builder));
            });
            services.TryAddScoped<Query, Query>();
            services.TryAddScoped<IJwtHandler, JwtHandler>();
            services.TryAddSingleton<INamingConventions, TrueVoteNamingConventions>();
            services.TryAddSingleton(new Uri("https://a.pool.opentimestamps.org")); // TODO Need to pull the Timestamp URL from Config. Also, TrueVote needs to stand up its own Timestamp servers.
            services.AddHttpClient<IOpenTimestampsClient, OpenTimestampsClient>().ConfigureHttpClient((provider, client) =>
            {
                var uri = provider.GetRequiredService<Uri>();
                client.BaseAddress = uri;
            });
            services.TryAddScoped<IValidator, Validator>();
            services.AddLogging();
            services.AddSingleton(typeof(ILogger), typeof(Logger<Startup>));
            services.AddGraphQLServer().AddQueryType<Query>();
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();
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

            app.UsePathBase("/api");
            app.UseOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.InjectJavascript(CustomJavaScriptPath);
                c.InjectStylesheet(CustomStylesheetPath);
            });

            app.UseHttpsRedirection();

            app.UseExceptionHandler();

            app.UseRouting();

            app.UseAuthorization();

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(GetDistFolderPath()),
                RequestPath = "/dist"
            });

            app.UseEndpoints(e =>
            {
                e.MapControllers();
                e.MapGraphQL();
                e.MapSwagger();
            });

            Console.WriteLine("HostingEnvironmentName: '{0}'", env.EnvironmentName);
        }

        public class TrueVoteDbContext : DbContext, ITrueVoteDbContext
        {
            public virtual DbSet<UserModel> Users { get; set; }

            public virtual DbSet<ElectionModel> Elections { get; set; }

            public virtual DbSet<RaceModel> Races { get; set; }

            public virtual DbSet<CandidateModel> Candidates { get; set; }

            public virtual DbSet<BallotModel> Ballots { get; set; }

            public virtual DbSet<TimestampModel> Timestamps { get; set; }

            public virtual DbSet<BallotHashModel> BallotHashes { get; set; }

            private readonly IConfiguration? _configuration;
            private readonly string? _connectionString;

            public TrueVoteDbContext(IConfiguration configuration)
            {
                _configuration = configuration;
                _connectionString = _configuration.GetConnectionString("CosmosDbConnectionString");
            }

            public TrueVoteDbContext()
            {
            }

            public virtual async Task<bool> EnsureCreatedAsync()
            {
                return await Database.EnsureCreatedAsync();
            }

            public virtual async Task<int> SaveChangesAsync()
            {
                return await base.SaveChangesAsync();
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseCosmos(_connectionString, "true-vote");
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.HasDefaultContainer("Users");
                modelBuilder.Entity<UserModel>().ToContainer("Users");
                modelBuilder.Entity<UserModel>().HasNoDiscriminator();

                modelBuilder.HasDefaultContainer("Ballots");
                modelBuilder.Entity<BallotModel>().ToContainer("Ballots");
                modelBuilder.Entity<BallotModel>().HasNoDiscriminator();
                modelBuilder.Entity<BallotModel>().Property(p => p.Election)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions) null),
                        v => JsonSerializer.Deserialize<ElectionModel>(v, (JsonSerializerOptions) null));

                modelBuilder.HasDefaultContainer("Elections");
                modelBuilder.Entity<ElectionModel>().ToContainer("Elections");
                modelBuilder.Entity<ElectionModel>().HasNoDiscriminator();
                modelBuilder.Entity<ElectionModel>().Property(p => p.Races)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions) null),
                        v => JsonSerializer.Deserialize<List<RaceModel>>(v, (JsonSerializerOptions) null),
                        new ValueComparer<ICollection<RaceModel>>(
                            (c1, c2) => c1.SequenceEqual(c2),
                            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                            c => c.ToList()));

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

                modelBuilder.HasDefaultContainer("Timestamps");
                modelBuilder.Entity<TimestampModel>().ToContainer("Timestamps");
                modelBuilder.Entity<TimestampModel>().HasNoDiscriminator();

                modelBuilder.HasDefaultContainer("BallotHashes");
                modelBuilder.Entity<BallotHashModel>().ToContainer("BallotHashes");
                modelBuilder.Entity<BallotHashModel>().HasNoDiscriminator();
            }
        }

        private string GetDistFolderPath()
        {
            // Get the base directory where the application is running
            var basePath = AppContext.BaseDirectory;

            // Combine with the 'dist' folder path and the runtime folder name
            return Path.Combine(basePath, "dist");
        }
    }

    [ExcludeFromCodeCoverage]
    public partial class TrueVoteNamingConventions : DefaultNamingConventions
    {
        // https://github.com/nigel-sampson/nigel-sampson.github.io/blob/07c87b04a3ab4c7133820d42814cf7c45d7a3a76/_posts/2020-10-8-graphlq-naming-conventions.md
        // https://compiledexperience.com/blog/posts/graphlq-naming-conventions
        // https://www.apollographql.com/docs/react/data/operation-best-practices/
        // Overriding this name via filter forces the GraphQL schema to "do nothing", which is the desired behavior. The default behavior is to mangle the names.
        public override string GetMemberName(MemberInfo member, MemberKind kind)
        {
            return member.Name;
        }
    }
}
