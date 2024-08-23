using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TrueVote.Api.Services;
using TrueVote.Api.Interfaces;
using TrueVote.Api.Models;
using System.Text.Json;
using HotChocolate.Types.Descriptors;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TrueVote.Api.Helpers;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Microsoft.Extensions.FileProviders;
using Path = System.IO.Path;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HotChocolate.Language;
using System.Globalization;

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
            services.AddSingleton<ValidateUserIdFilter>();
            services.AddControllers().AddNewtonsoftJson(jsonoptions =>
            {
                jsonoptions.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.IsoDateTimeConverter());
                jsonoptions.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                jsonoptions.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                jsonoptions.SerializerSettings.ContractResolver = new DefaultContractResolver();
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
            });

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
            services.TryAddScoped<IServiceBus, ServiceBus>();
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug)
                       .AddProvider(new CustomLoggerProvider(builder));
            });
            services.TryAddScoped<Query, Query>();
            services.TryAddScoped<IJwtHandler, JwtHandler>();
            services.TryAddScoped<IRecursiveValidator, RecursiveValidator>();
            services.TryAddSingleton<INamingConventions, TrueVoteNamingConventions>();
            services.TryAddSingleton(new Uri("https://a.pool.opentimestamps.org")); // TODO Need to pull the Timestamp URL from Config. Also, TrueVote needs to stand up its own Timestamp servers.
            services.AddHttpClient<IOpenTimestampsClient, OpenTimestampsClient>().ConfigureHttpClient((provider, client) =>
            {
                var uri = provider.GetRequiredService<Uri>();
                client.BaseAddress = uri;
            });
            services.TryAddScoped<IBallotValidator, BallotValidator>();
            services.AddLogging();
            services.AddSingleton(typeof(ILogger), typeof(Logger<Startup>));
            services.AddGraphQLServer().AddQueryType<Query>().BindRuntimeType<DateTime, UTCDateTimeFormatted>();
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

            // Redirect /swagger and /swagger/index.html to /
            // Unfortunately, browser renders as /index.html
            // Ideally it would just stop at / and not show /index.html
            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/swagger" || context.Request.Path == "/swagger/index.html")
                {
                    context.Response.Redirect("/");
                    return;
                }
                await next();
            });

            app.UseSwagger();
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

            app.UseHttpsRedirection();

            app.UseExceptionHandler();

            app.UseRouting();

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
            public virtual DbSet<FeedbackModel> Feedbacks { get; set; }
            public virtual DbSet<AccessCodeModel> ElectionAccessCodes { get; set; }
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
                        new ValueComparer<List<RaceModel>>(
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
                        new ValueComparer<List<CandidateModel>>(
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

                modelBuilder.HasDefaultContainer("Feedbacks");
                modelBuilder.Entity<FeedbackModel>().ToContainer("Feedbacks");
                modelBuilder.Entity<FeedbackModel>().HasNoDiscriminator();

                modelBuilder.HasDefaultContainer("ElectionAccessCodes");
                modelBuilder.Entity<AccessCodeModel>().ToContainer("ElectionAccessCodes");
                modelBuilder.Entity<AccessCodeModel>().HasNoDiscriminator();
                modelBuilder.Entity<AccessCodeModel>().HasKey(ac => new { ac.RequestId, ac.AccessCode, ac.ElectionId });
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
    public class ValidateUserIdFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = Guid.Empty;

            // Get the user ID from the JWT token
            // Dereference the ClaimTypes in an odd way because JwtRegisteredClaimNames doesn't work well.
            // Instead, getting this value from token creation code in JwtAuth.cs:
            // claims.Add(new Claim(JwtRegisteredClaimNames.NameId, userId));
            var nameIdentifierList = context.HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).ToList();
            foreach (var claim in nameIdentifierList)
            {
                if (claim.Value != null)
                {
                    var isValid = Guid.TryParse(claim.Value, out userId);

                    if (isValid)
                        break;
                }
            }

            //var userId = context.HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Skip(1).Take(1).Select(c => c.Value).FirstOrDefault() ?? null;
            if (userId == Guid.Empty)
            {
                context.Result = new ForbidResult();
                return;
            }

            // Check if the action argument is a model with a UserId property
            foreach (var model in context.ActionArguments.Values.Where(v => v != null))
            {
                switch (model)
                {
                    case UserModel userModel:
                    {
                        ValidateUserId(context, userModel.UserId, userId.ToString());
                        break;
                    }

                    case FeedbackModel feedbackModel:
                    {
                        ValidateUserId(context, feedbackModel.UserId, userId.ToString());
                        break;
                    }

                    case AccessCodesRequest accessCodesRequest:
                    {
                        ValidateUserId(context, accessCodesRequest.UserId, userId.ToString());
                        break;
                    }

                    case CheckCodeRequest checkCodeRequest:
                    {
                        ValidateUserId(context, checkCodeRequest.UserId, userId.ToString());
                        break;
                    }
                    // Add more cases for other models with UserId property

                    default:
                        break;
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Not needed for this filter
        }

        private void ValidateUserId(ActionExecutingContext context, string modelUserId, string tokenUserId)
        {
            if (modelUserId != tokenUserId)
            {
                context.Result = new ForbidResult();
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
}
#pragma warning restore IDE0046 // Convert to conditional expression
