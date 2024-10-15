using HotChocolate.Language;
using HotChocolate.Types.Descriptors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
using System.Security.Claims;
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
                o.SchemaFilter<AddCustomModelsFilter>();

                // Explicitly tell Swagger to include these models
                o.DocumentFilter<CustomModelDocumentFilter<ElectionResults>>();
                o.DocumentFilter<CustomModelDocumentFilter<RaceResult>>();
                o.DocumentFilter<CustomModelDocumentFilter<CandidateResult>>();

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
            services.TryAddScoped<Ballot>();
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug)
                       .AddProvider(new CustomLoggerProvider(builder));
            });
            services.TryAddScoped<Query, Query>();
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
            services.AddGraphQLServer().AddQueryType<Query>().BindRuntimeType<DateTime, UTCDateTimeFormatted>();
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();
            services.AddHostedService<TimerJobs>();
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

            dbContext.EnsureCreatedAsync().GetAwaiter().GetResult();

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
            public virtual DbSet<UsedAccessCodeModel> UsedAccessCodes { get; set; }
            public virtual DbSet<ElectionUserBindingModel> ElectionUserBindings { get; set; }

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
    public class CustomModelDocumentFilter<T> : IDocumentFilter where T : class
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            context.SchemaGenerator.GenerateSchema(typeof(T), context.SchemaRepository);
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
}
#pragma warning restore IDE0046 // Convert to conditional expression
