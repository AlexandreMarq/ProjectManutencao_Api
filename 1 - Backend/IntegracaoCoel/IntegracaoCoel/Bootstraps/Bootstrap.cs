using AppCoel.Core.API.Auth;
using AppCoel.Core.API.Middlewares;
using AppCoel.Core.Contracts;
using AppCoel.Core.Infra.Database;
using AppCoel.Core.Infra.Database.Entities.Auth;
using AppCoel.Core.Infra.Database.Mapper;
using AppCoel.Core.Services;
using AppCoel.Exceptions;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Globalization;
using System.Reflection;
using System.Security.Claims;
using System.Threading.RateLimiting;

namespace AppCoel.Core.API.Bootstraps
{
    public static class Bootstrap
    {
        private const string DefaultCorsPolicyName = "DefaultCorsPolicy";
        public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
        {
            var mvcBuilder = builder.Services.AddControllers();

            ConfigControllers(mvcBuilder);
            ConfigLocalization(builder, mvcBuilder);

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                ConfigValidations(options);
            });

            ConfigServices(builder);
            ConfigUsers(builder);
            ConfigMapper(builder);
            ConfigDatabase(builder);
            ConfigAuthentication(builder);
            ConfigSwagger(builder);
            ConfigCache(builder);
            ConfigLog(builder);
            ConfigCors(builder);
            ConfigRateLimiting(builder);

            return builder;
        }

        public static WebApplication UseApiPipeline(this WebApplication app)
        {
            app.UseRequestLocalization();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();

                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");

                    var auth0Options = new Auth0Options();
                    app.Configuration.GetSection("Auth0").Bind(auth0Options);

                    c.OAuthClientId(auth0Options.ClientId);
                    c.OAuthAppName("API - Swagger");
                    c.OAuthUsePkce();
                    c.OAuthScopeSeparator(" ");

                    c.OAuthScopes("openid", "profile", "email", "offline_access");

                    c.OAuthAdditionalQueryStringParams(new Dictionary<string, string>
                    {
                        { "audience", auth0Options.Audience }
                    });
                });
            }
            else
            {
                app.UseHsts();
            }
            app.Use(async (context, next) =>
            {
                context.Response.Headers.XContentTypeOptions = "nosniff";
                context.Response.Headers.XFrameOptions = "DENY";
                context.Response.Headers.XXSSProtection = "1; mode=block";
                context.Response.Headers["Referrer-Policy"] = "no-referrer";
                context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

                await next();
            });

            app.UseMiddleware<ExceptionMiddlewares>();
            app.UseHttpsRedirection();
            app.UseCors(DefaultCorsPolicyName);
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.UseMiddleware<SerilogUserEnricherMiddleware>();

            return app;
        }

        // Utilizado para iniciar o banco de dados e criar tabelas
        public static async Task InitializeDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await applicationDbContext.Database.MigrateAsync();
        }

        public static async Task CreateOrUpdateSystemAdminUserAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var systemAdminUserOptions = new SystemAdminUserOptions();
            app.Configuration.GetSection("SystemAdminUser").Bind(systemAdminUserOptions);

            if (string.IsNullOrWhiteSpace(systemAdminUserOptions.Name) || string.IsNullOrWhiteSpace(systemAdminUserOptions.Email))
            {
                throw new ArgumentException("SystemAdminUser configuration is not set properly.");
            }

            var user = await applicationDbContext.Users.FirstOrDefaultAsync(u => u.IsSystemAdmin);

            if (user == null)
            {
                var adminUserId = Guid.NewGuid();

                user = new TbUser
                {
                    Id = adminUserId,
                    Name = systemAdminUserOptions.Name,
                    Email = systemAdminUserOptions.Email,
                    IsSystemAdmin = true,
                    CreatedAt = DateTime.Now,
                    CreatedByUserId = adminUserId,
                    CreatedByUserName = systemAdminUserOptions.Name!
                };

                applicationDbContext.Users.Add(user);
                await applicationDbContext.SaveChangesAsync();
            }
            else if (user.Name != systemAdminUserOptions.Name || user.Email != systemAdminUserOptions.Email)
            {
                user.Name = systemAdminUserOptions.Name!;
                user.Email = systemAdminUserOptions.Email!;
                user.UpdatedAt = DateTime.Now;
                user.UpdatedByUserId = user.Id;
                user.UpdatedByUserName = systemAdminUserOptions.Name!;

                applicationDbContext.Users.Update(user);
                await applicationDbContext.SaveChangesAsync();
            }

            var hybridCache = scope.ServiceProvider.GetRequiredService<HybridCache>();
            await hybridCache.RemoveAsync(CacheKeys.SystemAdminUserKeys);
        }

        private static void ConfigControllers(IMvcBuilder mvcBuilder)
        {
            foreach (var asembly in GetControllerAssemblies())
            {
                mvcBuilder.AddApplicationPart(asembly);
            }
        }

        private static void ConfigLocalization(WebApplicationBuilder builder, IMvcBuilder mvcBuilder)
        {
            mvcBuilder.AddDataAnnotationsLocalization();
            builder.Services.AddLocalization();

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportCulture = new[] { "en-US", "pt-BR", "es-ES" };
                options.DefaultRequestCulture = new RequestCulture("en-US");
                options.SupportedCultures = supportCulture.Select(c => new CultureInfo(c)).ToList();
                options.SupportedUICultures = supportCulture.Select(c => new CultureInfo(c)).ToList();
                options.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new QueryStringRequestCultureProvider(),
                    new CookieRequestCultureProvider(),
                    new AcceptLanguageHeaderRequestCultureProvider()
                };
            });
        }

        private static void ConfigValidations(ApiBehaviorOptions options)
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var modelErros = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors.Select(er => er.ErrorMessage));

                var erros = modelErros.Any() ? string.Join(" ", modelErros) : null;

                throw new AppException(ExceptionCode.RequestValidation, erros);
            };
        }

        private static void ConfigServices(WebApplicationBuilder builder)
        {
            builder.Services.Scan(x =>
                x.FromAssemblies(GetServiceAssemblies())
                .AddClasses(y =>
                    y.AssignableTo<ITransientService>())
                .AsImplementedInterfaces()
                .WithTransientLifetime());

            builder.Services.Scan(x =>
                x.FromAssemblies(GetServiceAssemblies())
                .AddClasses(y =>
                    y.AssignableTo<IScopedServices>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            builder.Services.Scan(x =>
                x.FromAssemblies(GetServiceAssemblies())
                .AddClasses(y =>
                    y.AssignableTo<ISingletonService>())
                .AsImplementedInterfaces()
                .WithSingletonLifetime());
        }

        private static void ConfigUsers(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IUserContext, UserContext>();
            builder.Services.AddHttpContextAccessor();
        }

        private static void ConfigMapper(WebApplicationBuilder builder)
        {
            builder.Services.AddMapster();

            builder.Services.Scan(scan => scan
                .FromAssemblyOf<IRegisterMapsterConfig>()
                .AddClasses(classes => classes.AssignableTo<IRegisterMapsterConfig>())
                .As<IRegisterMapsterConfig>()
                .WithSingletonLifetime());

            var provider = builder.Services.BuildServiceProvider();
            var configs = provider.GetServices<IRegisterMapsterConfig>();

            var config = TypeAdapterConfig.GlobalSettings;

            foreach ( var mapConfig in configs)
            {
                mapConfig.Register(config);
            }

            builder.Services.AddSingleton(config);
        }

        private static void ConfigDatabase(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IDatabaseRepository, DatabaseRepository>();

            var connectionStringsOptions = new ConnectionStringsOptions();
            builder.Configuration.GetSection("ConnectionStrings").Bind(connectionStringsOptions);

            var databaseConnection = connectionStringsOptions.DatabaseConnection;
            if (string.IsNullOrWhiteSpace(databaseConnection))
            {
                throw new ArgumentException("Database connection string is not configured");
            }

            builder.Services.AddDbContext<ApplicationDbContext>(x =>
                x.UseSqlServer(
                    databaseConnection,
                    y => y.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                );
        }

        private static void ConfigAuthentication(WebApplicationBuilder builder)
        {
            builder.Services.AddAuthentication();

            var auth0Options = new Auth0Options();
            builder.Configuration.GetSection("Auth0").Bind(auth0Options);

            var authority = auth0Options.Authority;
            var audience = auth0Options.Audience;

            if (string.IsNullOrEmpty(authority) || string.IsNullOrEmpty(audience) )
            {
                throw new ArgumentException("Auth- configuration is not set properly.");
            }
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = audience;

                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = authority,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            });
        }

        private static void ConfigSwagger(WebApplicationBuilder builder)
        {
            builder.Services.AddOpenApi();
            var auth0Options = new Auth0Options();
            builder.Configuration.GetSection("Auth0").Bind(auth0Options);

            var authority = auth0Options.Authority;

            if (string.IsNullOrWhiteSpace(authority))
            {
                throw new ArgumentException("Auth0 configurantion is not set properly.");
            }

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Api Coel",
                    Version = "v1"
                });

                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{auth0Options.Authority}/authorize?prompt=login"),
                            TokenUrl = new Uri($"{auth0Options.Authority}/oauth/token"),
                            Scopes = new Dictionary<string, string>
                        {
                            { "openid", "OpenID" },
                            { "profile", "Profile" },
                            { "email", "Email" },
                            { "offline_access", "Offline Access" }
                        }
                        }
                    },
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "OAuth2 with Auth0"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "oauth2"
                            }
                        },
                        new[] { "openid", "profile", "email", "offline_access" }
                    }
                });
            });
        }

        private static void ConfigCache(WebApplicationBuilder builder)
        {
            builder.Services.AddHybridCache();

            // Can confiigura IDistributedCache here if needed and connect with Redis or other cache providers.
        }

        private static void ConfigLog(WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Logging.AddSerilog(Log.Logger);

            // Can configure Application Insights
        }

        private static void ConfigCors(WebApplicationBuilder builder)
        {
            var corsOptions = new CorsOptions();
            builder.Configuration.GetSection("Cors").Bind(corsOptions);

            if (corsOptions.AllowedOrigins?.Any() == false)
            {
                throw new ArgumentException("CORS allowed origins are not configured.");
            }
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(DefaultCorsPolicyName, policy =>
                {
                    policy.WithOrigins(corsOptions?.AllowedOrigins ?? Array.Empty<string>())
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                });
            });
        }

        private static void ConfigRateLimiting(WebApplicationBuilder builder)
        {
            var rateOptions = new RateLimitingOptions();
            builder.Configuration.GetSection("RateLimiting").Bind(rateOptions);

            if (rateOptions.PermitLimit <= 0 || rateOptions.WindowSeconds <= 0 || rateOptions.QueueLimit < 0)
            {
                throw new ArgumentException("Rate limiting options are not configured properly.");
            }

            builder.Services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = rateOptions.PermitLimit,
                            Window = TimeSpan.FromSeconds(rateOptions.WindowSeconds),
                            QueueLimit = rateOptions.QueueLimit,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                        }
                    ));
            });
        }

        private static IEnumerable<Assembly> GetControllerAssemblies() =>
            [
                Assembly.Load("AppCoel.Core.Controllers.General"),
            ];

        private static IEnumerable<Assembly> GetServiceAssemblies() =>
            [
                Assembly.Load("AppCoel.Core.Services"),
                Assembly.Load("AppCoel.Core.Services.General")
            ];

        private class ConnectionStringsOptions
        {
            public string? DatabaseConnection { get; set; }
        }

        private class Auth0Options
        {
            public string? Authority { get; set; }
            public string? ClientId { get; set; }
            public string? Audience { get; set; }
        }
        private class SystemAdminUserOptions
        {
            public string? Name { get; set; }
            public string? Email { get; set; }
        }

        private class CorsOptions
        {
            public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
        }

        private class RateLimitingOptions
        {
            public int PermitLimit { get; set; }
            public int WindowSeconds { get; set; }
            public int QueueLimit { get; set; }
        }
    }
}
