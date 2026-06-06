using AppCoel.Core.API.Auth;
using AppCoel.Core.API.Middlewares;
using AppCoel.Core.Contracts;
using AppCoel.Core.Infra.Database;
using AppCoel.Core.Infra.Database.Mapper;
using AppCoel.Core.Services;
using AppCoel.Exceptions;
using Mapster;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Reflection;

namespace AppCoel.Core.API.Bootstraps
{
    public static class Bootstrap
    {
        public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
        {
            var mvcBuilder = builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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

            return builder;
        }

        public static WebApplication UseApiPipeline(this WebApplication app)
        {
            app.UseRequestLocalization();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<ExceptionMiddlewares>();
            app.UseHttpsRedirection();
            app.MapControllers();

            return app;
        }

        // Utilizado para iniciar o banco de dados e criar tabelas
        public static async Task InitializeDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await applicationDbContext.Database.MigrateAsync();
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
    }
}
