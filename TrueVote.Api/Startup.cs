using Azure.Messaging.ServiceBus;
using HotChocolate.Language;
using HotChocolate.Types.Descriptors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Abstractions;
using System.Reflection;
using System.Text.Json;
using TrueVote.Api.Helpers;
using TrueVote.Api.Interfaces;
using TrueVote.Api.Models;
using TrueVote.Api.Services;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Path = System.IO.Path;

#pragma warning disable IDE0046 // Convert to conditional expression
namespace TrueVote.Api
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private readonly IConfiguration _configuration;
        public string CustomStylesheetPath { get; set; } = "/dist/truevote-api.css";
        public string CustomJavaScriptPath { get; set; } = "/dist/truevote-api.js";

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson(jsonoptions =>
            {
                jsonoptions.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.IsoDateTimeConverter());
                jsonoptions.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                jsonoptions.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                jsonoptions.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });

            services.AddEndpointsApiExplorer();
            services.AddOpenApiDocument(o =>
            {
                o.PostProcess = doc =>
                {
                    doc.Info.Title = "TrueVote.Api";
                    doc.Info.Version = "v1";
                    doc.Info.Description = "TrueVote APIs using strict OpenAPI specification.";

                    // Add SecurityDefinitions for Swagger UI authorization
                    doc.SecurityDefinitions.Add("Bearer", new NSwag.OpenApiSecurityScheme
                    {
                        Description = "Please enter a valid TrueVote.Api token",
                        Name = "Authorization",
                        In = NSwag.OpenApiSecurityApiKeyLocation.Header,
                        Type = NSwag.OpenApiSecuritySchemeType.Http,
                        BearerFormat = "JWT",
                        Scheme = "Bearer"
                    });
                };
                o.OperationProcessors.Add(new AuthorizeCheckOperationProcessor());
                o.DocumentProcessors.Add(new CustomModelDocumentProcessor<ElectionResults>());
                o.DocumentProcessors.Add(new CustomModelDocumentProcessor<RaceResult>());
                o.DocumentProcessors.Add(new CustomModelDocumentProcessor<CandidateResult>());
                o.DocumentProcessors.Add(new CustomModelDocumentProcessor<ServiceBusCommsMessage>());
                o.DocumentProcessors.Add(new CustomModelDocumentProcessor<ServiceBusCommsMessage>());
                o.DocumentProcessors.Add(new CustomModelDocumentProcessor<BallotIdInfo>());
                o.DocumentProcessors.Add(new CustomModelDocumentProcessor<PaginatedBallotIds>());
            });
            services.AddSwaggerGen(o =>
            {
                var baseUrl = "/api";
                o.AddServer(new OpenApiServer
                {
                    Url = baseUrl
                });
                o.EnableAnnotations();
                o.SchemaFilter<AddCustomModelsFilter>();

                // Explicitly tell Swagger to include these models
                o.DocumentFilter<CustomModelDocumentFilter<ElectionResults>>();
                o.DocumentFilter<CustomModelDocumentFilter<RaceResult>>();
                o.DocumentFilter<CustomModelDocumentFilter<CandidateResult>>();
                o.DocumentFilter<CustomModelDocumentFilter<ServiceBusCommsMessage>>();
                o.DocumentFilter<CustomModelDocumentFilter<BallotIdInfo>>();
                o.DocumentFilter<CustomModelDocumentFilter<PaginatedBallotIds>>();

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
                o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Please enter a valid TrueVote.API token",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                o.OperationFilter<AuthorizeCheckOperationFilter>();
                o.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
                o.OperationFilter<AuthorizeCheckOperationFilter>();
            });

            services.AddHealthChecks()
                .AddCheck("Api", () => HealthCheckResult.Healthy("Api is healthy"))
                .AddCheck<DatabaseHealthCheck>("Database")
                .AddCheck<ServiceBusHealthCheck>("ServiceBus", failureStatus: HealthStatus.Degraded, tags: new[] { "servicebus", "messaging" });

            services.AddApplicationInsightsTelemetry();
            services.AddDbContext<ITrueVoteDbContext, TrueVoteDbContext>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                var secretKey = _configuration["JWTSecret"];
                var secretByte = Convert.FromBase64String(secretKey);
                var symmetricSecurityKey = new SymmetricSecurityKey(secretByte);
                var clockSkew = TimeSpan.FromMinutes(1);

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "TrueVoteApi",
                    ValidAudience = "https://api.truevote.org/api/",
                    IssuerSigningKey = symmetricSecurityKey,
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ValidateActor = false,
                    ValidateTokenReplay = false,
                    ClockSkew = clockSkew
                };

                // Enable additional logging
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Authentication failed: {context.Exception}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        // Truncate the token so it doesn't get logged
                        Console.WriteLine($"Token validated: {context.SecurityToken.ToString()[..3]}");
                        return Task.CompletedTask;
                    }
                };
            });
            services.TryAddScoped<IFileSystem, FileSystem>();

            services.AddSingleton<ResilientServiceBus>();
            services.AddSingleton<IServiceBus>(sp => sp.GetRequiredService<ResilientServiceBus>());
            services.AddHostedService<RetryBackgroundService>();

            services.TryAddScoped<Ballot>();

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug).AddProvider(new CustomLoggerProvider(builder));
            });

            services.AddDbContextFactory<TrueVoteDbContext>(options => options.UseCosmos(_configuration.GetConnectionString("CosmosDbConnectionString"), "true-vote"), ServiceLifetime.Scoped);

            services.TryAddScoped<Query, Query>();
            services.TryAddScoped<Subscription, Subscription>();
            services.TryAddScoped<IJwtHandler, JwtHandler>();
            services.TryAddScoped<IRecursiveValidator, RecursiveValidator>();
            services.TryAddSingleton<INamingConventions, TrueVoteNamingConventions>();
            services.TryAddSingleton<IUniqueKeyGenerator, UniqueKeyGenerator>();
            services.TryAddSingleton(new Uri("https://a.pool.opentimestamps.org")); // TODO Need to pull the Timestamp URL from Config. Also, TrueVote needs to stand up its own Timestamp servers.
            services.AddHttpClient<IOpenTimestampsClient, OpenTimestampsClient>().ConfigureHttpClient((provider, client) =>
            {
                var uri = provider.GetRequiredService<Uri>();
                client.BaseAddress = uri;
            });
            services.TryAddScoped<IHasher, Hasher>();
            services.AddLogging();
            services.AddSingleton(typeof(ILogger), typeof(Logger<Startup>));
            services.AddGraphQLServer().AddQueryType<Query>().BindRuntimeType<DateTime, UTCDateTimeFormatted>()
                .AddSubscriptionType<Subscription>()
                .AddInMemorySubscriptions();

            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();
            services.AddHostedService<TimerJobs>();
            services.AddDatabaseSeeder();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, TrueVoteDbContext dbContext)
        {
            if (env.IsDevelopment())
            {
                Console.WriteLine("Running Locally");
            }
            else
            {
                Console.WriteLine("Running Deployed");
            }

            app.UseStatusCodePages(async context =>
            {
                // Return clean JSON errors instead of HTML
                context.HttpContext.Response.ContentType = "application/json";
                var response = new
                {
                    status = context.HttpContext.Response.StatusCode,
                    message = "Not Found"
                };
                await context.HttpContext.Response.WriteAsJsonAsync(response);
            });

            app.UseHttpsRedirection();
            app.UseExceptionHandler();

            app.UseOpenApi();
            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    swaggerDoc.Servers = new List<OpenApiServer>
                    {
                        new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}/api" }
                    };
                });
                c.RouteTemplate = "swagger/{documentName}/swagger.json";
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TrueVote.Api");
                c.RoutePrefix = string.Empty;
                c.InjectJavascript(CustomJavaScriptPath);
                c.InjectStylesheet(CustomStylesheetPath);
                c.DisplayRequestDuration();
                c.EnableDeepLinking();
                c.EnableValidator();
            });

            app.UsePathBase("/api");
            app.UseRouting();
            app.UseWebSockets();
            app.UseAuthentication();
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
                e.MapHealthChecks("/health", new HealthCheckOptions
                {
                    ResponseWriter = async (context, report) =>
                    {
                        context.Response.ContentType = "application/json";
                        var response = new
                        {
                            status = report.Status.ToString(),
                            checks = report.Entries.Select(e => new
                            {
                                name = e.Key,
                                status = e.Value.Status.ToString(),
                                description = e.Value.Description,
                                duration = e.Value.Duration.ToString()
                            }),
                            duration = report.TotalDuration
                        };
                        await context.Response.WriteAsJsonAsync(response);
                    }
                });
                e.MapGet("/", context =>
                {
                    context.Response.Redirect("/index.html");
                    return Task.CompletedTask;
                });
            });

            // Security headers
            app.Use(async (context, next) =>
            {
                // Add security headers
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Append("X-Frame-Options", "DENY");
                context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

                // Block access to sensitive files
                var path = context.Request.Path.Value?.ToLower();
                if (path != null && (path.Contains("/.env") || path.Contains("/web.config") || path.Contains("/appsettings")))
                {
                    context.Response.StatusCode = 404;
                    return;
                }

                await next();
            });

            dbContext.EnsureCreatedAsync().GetAwaiter().GetResult();
            _ = app.SeedDatabase();

            Console.WriteLine("HostingEnvironmentName: '{0}'", env.EnvironmentName);
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
    public class TrueVoteDbContext : DbContext, ITrueVoteDbContext
    {
        public virtual required DbSet<UserModel> Users { get; set; }
        public virtual required DbSet<ElectionModel> Elections { get; set; }
        public virtual required DbSet<RaceModel> Races { get; set; }
        public virtual required DbSet<CandidateModel> Candidates { get; set; }
        public virtual required DbSet<BallotModel> Ballots { get; set; }
        public virtual required DbSet<TimestampModel> Timestamps { get; set; }
        public virtual required DbSet<BallotHashModel> BallotHashes { get; set; }
        public virtual required DbSet<FeedbackModel> Feedbacks { get; set; }
        public virtual required DbSet<AccessCodeModel> ElectionAccessCodes { get; set; }
        public virtual required DbSet<UsedAccessCodeModel> UsedAccessCodes { get; set; }
        public virtual required DbSet<ElectionUserBindingModel> ElectionUserBindings { get; set; }
        public virtual required DbSet<RoleModel> Roles { get; set; }
        public virtual required DbSet<UserRoleModel> UserRoles { get; set; }
        public virtual required DbSet<CommunicationEventModel> CommunicationEvents { get; set; }

        private readonly IConfiguration? _configuration;
        private readonly string? _connectionString;

        public TrueVoteDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("CosmosDbConnectionString");
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
            optionsBuilder.UseCosmos(_connectionString, "true-vote")
                .ConfigureWarnings(warnings =>
                {
                    // TODO Remove this and make all calls Async so this isn't needed
                    warnings.Ignore(CosmosEventId.SyncNotSupported);
                });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultContainer("Users");
            modelBuilder.Entity<UserModel>().ToContainer("Users");
            modelBuilder.Entity<UserModel>().HasNoDiscriminator();
            modelBuilder.Entity<UserModel>().HasPartitionKey(u => u.UserId);
            modelBuilder.Entity<UserModel>().HasKey(u => u.UserId);

            modelBuilder.HasDefaultContainer("Ballots");
            modelBuilder.Entity<BallotModel>().ToContainer("Ballots");
            modelBuilder.Entity<BallotModel>().HasNoDiscriminator();
            modelBuilder.Entity<BallotModel>().HasPartitionKey(b => b.BallotId);
            modelBuilder.Entity<BallotModel>().HasKey(b => b.BallotId);
            modelBuilder.Entity<BallotModel>().Property(p => p.Election)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions) null),
                    v => JsonSerializer.Deserialize<ElectionModel>(v, (JsonSerializerOptions) null));
            modelBuilder.Entity<BallotModel>().HasOne<BallotHashModel>().WithOne()
                .HasForeignKey<BallotHashModel>(e => e.BallotId).IsRequired(false);

            modelBuilder.HasDefaultContainer("Elections");
            modelBuilder.Entity<ElectionModel>().ToContainer("Elections");
            modelBuilder.Entity<ElectionModel>().HasNoDiscriminator();
            modelBuilder.Entity<ElectionModel>().HasPartitionKey(e => e.ElectionId);
            modelBuilder.Entity<ElectionModel>().HasKey(e => e.ElectionId);
            modelBuilder.Entity<ElectionModel>().Property(e => e.Races)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions) null),
                    v => JsonSerializer.Deserialize<List<RaceModel>>(v, (JsonSerializerOptions) null),
                    new ValueComparer<List<RaceModel>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));

            modelBuilder.HasDefaultContainer("Races");
            modelBuilder.Entity<RaceModel>().ToContainer("Races");
            modelBuilder.Entity<RaceModel>().HasNoDiscriminator();
            modelBuilder.Entity<RaceModel>().HasPartitionKey(r => r.RaceId);
            modelBuilder.Entity<RaceModel>().HasKey(r => r.RaceId);
            modelBuilder.Entity<RaceModel>().Property(r => r.Candidates)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions) null),
                    v => JsonSerializer.Deserialize<List<CandidateModel>>(v, (JsonSerializerOptions) null),
                    new ValueComparer<List<CandidateModel>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));

            modelBuilder.HasDefaultContainer("Candidates");
            modelBuilder.Entity<CandidateModel>().ToContainer("Candidates");
            modelBuilder.Entity<CandidateModel>().HasNoDiscriminator();
            modelBuilder.Entity<CandidateModel>().HasPartitionKey(c => c.CandidateId);
            modelBuilder.Entity<CandidateModel>().HasKey(c => c.CandidateId);

            modelBuilder.HasDefaultContainer("Timestamps");
            modelBuilder.Entity<TimestampModel>().ToContainer("Timestamps");
            modelBuilder.Entity<TimestampModel>().HasNoDiscriminator();
            modelBuilder.Entity<TimestampModel>().HasPartitionKey(t => t.TimestampId);
            modelBuilder.Entity<TimestampModel>().HasKey(t => t.TimestampId);

            modelBuilder.HasDefaultContainer("BallotHashes");
            modelBuilder.Entity<BallotHashModel>().ToContainer("BallotHashes");
            modelBuilder.Entity<BallotHashModel>().HasNoDiscriminator();
            modelBuilder.Entity<BallotHashModel>().HasPartitionKey(b => b.BallotId);
            modelBuilder.Entity<BallotHashModel>().HasKey(b => b.BallotId);

            modelBuilder.HasDefaultContainer("Feedbacks");
            modelBuilder.Entity<FeedbackModel>().ToContainer("Feedbacks");
            modelBuilder.Entity<FeedbackModel>().HasNoDiscriminator();
            modelBuilder.Entity<FeedbackModel>().HasPartitionKey(f => f.FeedbackId);
            modelBuilder.Entity<FeedbackModel>().HasKey(f => f.FeedbackId);

            modelBuilder.HasDefaultContainer("ElectionAccessCodes");
            modelBuilder.Entity<AccessCodeModel>().ToContainer("ElectionAccessCodes");
            modelBuilder.Entity<AccessCodeModel>().HasNoDiscriminator();
            modelBuilder.Entity<AccessCodeModel>().HasPartitionKey(eac => eac.RequestId);
            modelBuilder.Entity<AccessCodeModel>().HasKey(eac => new { eac.RequestId, eac.AccessCode, eac.ElectionId });

            modelBuilder.HasDefaultContainer("UsedAccessCodes");
            modelBuilder.Entity<UsedAccessCodeModel>().ToContainer("UsedAccessCodes");
            modelBuilder.Entity<UsedAccessCodeModel>().HasNoDiscriminator();
            modelBuilder.Entity<UsedAccessCodeModel>().HasPartitionKey(uac => uac.AccessCode);
            modelBuilder.Entity<UsedAccessCodeModel>().HasKey(uac => uac.AccessCode);

            modelBuilder.HasDefaultContainer("ElectionUserBindings");
            modelBuilder.Entity<ElectionUserBindingModel>().ToContainer("ElectionUserBindings");
            modelBuilder.Entity<ElectionUserBindingModel>().HasNoDiscriminator();
            modelBuilder.Entity<ElectionUserBindingModel>().HasPartitionKey(eub => eub.UserId);
            modelBuilder.Entity<ElectionUserBindingModel>().HasKey(eub => new { eub.UserId, eub.ElectionId });

            modelBuilder.HasDefaultContainer("Roles");
            modelBuilder.Entity<RoleModel>().ToContainer("Roles").HasPartitionKey(r => r.RoleId);

            modelBuilder.HasDefaultContainer("UserRoles");
            modelBuilder.Entity<UserRoleModel>().ToContainer("UserRoles").HasPartitionKey(ur => ur.UserId)
            .HasOne<UserModel>()
            .WithMany()
            .HasForeignKey(ur => ur.UserId)
            .HasPrincipalKey(u => u.UserId);

            modelBuilder.HasDefaultContainer("CommunicationEvents");
            modelBuilder.Entity<CommunicationEventModel>().ToContainer("CommunicationEvents");
            modelBuilder.Entity<CommunicationEventModel>().HasNoDiscriminator();
            modelBuilder.Entity<CommunicationEventModel>().HasPartitionKey(c => c.CommunicationEventId);
            modelBuilder.Entity<CommunicationEventModel>().HasKey(c => c.CommunicationEventId);
            modelBuilder.Entity<CommunicationEventModel>()
               .Property(c => c.CommunicationMethod)
               .HasConversion(
                   v => JsonSerializer.Serialize(v, (JsonSerializerOptions) null),
                   v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions) null),
                   new ValueComparer<Dictionary<string, string>>(
                       (c1, c2) => c1.SequenceEqual(c2),
                       c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                       c => new Dictionary<string, string>(c)
                   ));
            modelBuilder.Entity<CommunicationEventModel>()
               .Property(c => c.RelatedEntities)
               .HasConversion(
                   v => JsonSerializer.Serialize(v, (JsonSerializerOptions) null),
                   v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions) null),
                   new ValueComparer<Dictionary<string, string>>(
                       (c1, c2) => c1.SequenceEqual(c2),
                       c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                       c => new Dictionary<string, string>(c)
                   ));
            modelBuilder.Entity<CommunicationEventModel>()
               .Property(c => c.Metadata)
               .HasConversion(
                   v => JsonSerializer.Serialize(v, (JsonSerializerOptions) null),
                   v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions) null),
                   new ValueComparer<Dictionary<string, string>>(
                       (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                       c => c != null ? c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())) : 0,
                       c => c != null ? new Dictionary<string, string>(c) : null
                   ));
            modelBuilder.Entity<CommunicationEventModel>()
                .Property(c => c.CommunicationMethodJson)
                .HasJsonConversion();

            modelBuilder.Entity<CommunicationEventModel>()
                .Property(c => c.RelatedEntitiesJson)
                .HasJsonConversion();

            modelBuilder.Entity<CommunicationEventModel>()
                .Property(c => c.MetadataJson)
                .HasJsonConversion();
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

    [ExcludeFromCodeCoverage]
    public class AuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check if the endpoint (action) has the Authorize attribute
            var hasAuthorizeAttribute = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>().Any() ||
                context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

            if (hasAuthorizeAttribute)
            {
                // If the endpoint has [Authorize] attribute, display the "Authorize" button
                operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });

                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}
                        }
                    }
                };
            }
        }
    }

    [ExcludeFromCodeCoverage]
    public class RequireRoleAttribute : AuthorizeAttribute
    {
        public RequireRoleAttribute(params string[] roles)
        {
            Roles = string.Join(",", roles);
        }
    }

    [ExcludeFromCodeCoverage]
    public class AddCustomModelsFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(ElectionResults) ||
                context.Type == typeof(RaceResult) ||
                context.Type == typeof(CandidateResult))
            {
                // This will force Swagger to generate a schema for these types
                _ = schema.Properties;
            }
        }
    }

    [ExcludeFromCodeCoverage]
    public class UTCDateTimeFormatted : ScalarType<DateTime, StringValueNode>
    {
        private const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss";

        public UTCDateTimeFormatted() : base("DateTime")
        {
            Description = "Custom UTC DateTime scalar with consistent formatting";
        }

        public override IValueNode ParseResult(object? resultValue)
        {
            if (resultValue is DateTime dateTime)
            {
                return new StringValueNode(dateTime.ToString(DateTimeFormat, CultureInfo.InvariantCulture));
            }
            throw new SerializationException($"Cannot parse result '{resultValue}' to DateTime.", this);
        }

        protected override DateTime ParseLiteral(StringValueNode valueSyntax)
        {
            if (DateTime.TryParse(valueSyntax.Value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dateTime))
            {
                return dateTime;
            }
            throw new SerializationException($"Cannot parse literal '{valueSyntax}' to DateTime.", this);
        }

        protected override StringValueNode ParseValue(DateTime runtimeValue)
        {
            return new StringValueNode(runtimeValue.ToString(DateTimeFormat, CultureInfo.InvariantCulture));
        }

        public override object? Serialize(object? runtimeValue)
        {
            if (runtimeValue is DateTime dateTime)
            {
                return dateTime.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
            }
            return null;
        }
    }

    [ExcludeFromCodeCoverage]
    public class QueryStringModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context.Metadata.IsComplexType)
            {
                return new QueryStringModelBinder();
            }

            return null;
        }
    }

    [ExcludeFromCodeCoverage]
    public class QueryStringModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            ArgumentNullException.ThrowIfNull(bindingContext);

            var model = Activator.CreateInstance(bindingContext.ModelType);

            foreach (var property in bindingContext.ModelType.GetProperties())
            {
                var key = property.Name;
                var value = bindingContext.ValueProvider.GetValue(key).FirstValue;

                if (!string.IsNullOrEmpty(value))
                {
                    var convertedValue = ConvertValue(value, property.PropertyType);
                    property.SetValue(model, convertedValue);
                }
            }

            bindingContext.Result = ModelBindingResult.Success(model);
            return Task.CompletedTask;
        }

        private static object ConvertValue(string value, Type targetType)
        {
            if (targetType == typeof(string))
            {
                return value;
            }

            var converter = TypeDescriptor.GetConverter(targetType);
            if (converter != null && converter.CanConvertFrom(typeof(string)))
            {
                return converter.ConvertFromString(value);
            }

            throw new InvalidOperationException($"Unable to convert value '{value}' to type {targetType}");
        }
    }

    [ExcludeFromCodeCoverage]
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DatabaseHealthCheck(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ITrueVoteDbContext>();
                await dbContext.EnsureCreatedAsync();
                return HealthCheckResult.Healthy("Database connection is healthy");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Database connection is unhealthy", ex);
            }
        }
    }

    [ExcludeFromCodeCoverage]
    public class ServiceBusHealthCheck : IHealthCheck
    {
        private readonly ILogger<ServiceBusHealthCheck> _logger;
        private readonly IConfiguration _configuration;

        public ServiceBusHealthCheck(ILogger<ServiceBusHealthCheck> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("ServiceBusConnectionString");
                var queueName = _configuration["ServiceBusHealthCheckQueueName"]!;

                _logger.LogDebug($"Attempting health check on queue: {queueName}");

                // Create a new client and sender for each check
                await using var client = new ServiceBusClient(connectionString);
                await using var sender = client.CreateSender(queueName);

                // Create test message with debug info
                var messageContent = new
                {
                    Type = "HealthCheck",
                    Source = "ServiceBusHealthCheck",
                    Timestamp = DateTime.UtcNow,
                    QueueName = queueName
                };

                var message = new ServiceBusMessage(BinaryData.FromString(JsonSerializer.Serialize(messageContent)))
                {
                    Subject = "HealthCheck",
                    ContentType = "application/json",
                    TimeToLive = TimeSpan.FromMinutes(5),
                    MessageId = Guid.NewGuid().ToString()
                };

                _logger.LogDebug($"Sending health check message with ID: {message.MessageId}");

                // Use a short timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

                await sender.SendMessageAsync(message, cts.Token);

                _logger.LogDebug($"Successfully sent health check message: {message.MessageId}");

                return HealthCheckResult.Healthy($"Service Bus health check successful. MessageId: {message.MessageId}", new Dictionary<string, object>
                {
                    { "MessageId", message.MessageId },
                    { "Queue", queueName },
                    { "Timestamp", DateTime.UtcNow }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service Bus health check failed");
                return HealthCheckResult.Unhealthy("Service Bus connection failed", ex);
            }
        }
    }
}
#pragma warning restore IDE0046 // Convert to conditional expression
