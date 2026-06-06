using AppCoel.Core.API.Middlewares;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Reflection;
using AppCoel.Exceptions;

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

        private static IEnumerable<Assembly> GetControllerAssemblies() =>
            [
                Assembly.Load("AppCoel.Core.Controllers.General"),
            ];
    }
}
