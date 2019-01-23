using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Configuration;
using Jasper.Http;
using Jasper.Http.Model;
using Jasper.Http.Routing;
using Jasper.Http.Routing.Codegen;
using Jasper.Messaging;
using Lamar;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jasper
{
    public static class WebHostBuilderExtensions
    {
        /// <summary>
        /// Add Jasper to an ASP.Net Core application using a custom JasperOptionsBuilder (or JasperRegistry) type
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="overrides"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IWebHostBuilder UseJasper<T>(this IWebHostBuilder builder, Action<T> overrides = null) where T : JasperOptionsBuilder, new ()
        {
            var registry = new T();
            overrides?.Invoke(registry);
            return builder.UseJasper(registry);
        }

        /// <summary>
        /// Add Jasper to an ASP.Net Core application with optional configuration to Jasper
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="overrides">Programmatically configure Jasper options</param>
        /// <param name="configure">Programmatically configure Jasper options using the application's IConfiguration and IHostingEnvironment</param>
        /// <returns></returns>
        public static IWebHostBuilder UseJasper(this IWebHostBuilder builder, Action<JasperOptionsBuilder> overrides = null, Action<WebHostBuilderContext, JasperOptions> configure = null)
        {
            var registry = new JasperOptionsBuilder();
            overrides?.Invoke(registry);

            if (configure != null)
            {
                registry.Settings.Messaging(configure);
            }

            return builder.UseJasper(registry);
        }

        /// <summary>
        /// Add Jasper to an ASP.Net Core application with a pre-built JasperOptionsBuilder
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="jasperBuilder"></param>
        /// <returns></returns>
        public static IWebHostBuilder UseJasper(this IWebHostBuilder builder, JasperOptionsBuilder jasperBuilder)
        {
            JasperRuntime.ApplyExtensions(jasperBuilder);

            jasperBuilder.HttpRoutes.StartFindingRoutes(jasperBuilder.ApplicationAssembly);
            jasperBuilder.Messaging.StartCompiling(jasperBuilder);

            jasperBuilder.Settings.Apply(jasperBuilder.Services);

            builder.ConfigureServices(s =>
            {

                if (jasperBuilder.HttpRoutes.AspNetCoreCompliance == ComplianceMode.GoFaster)
                {
                    s.RemoveAll(x => x.ServiceType == typeof(IStartupFilter) && x.ImplementationType == typeof(AutoRequestServicesStartupFilter));
                }

                s.AddSingleton<IHostedService, JasperActivator>();

                s.AddRange(jasperBuilder.CombineServices());
                s.AddSingleton(jasperBuilder);

                s.AddSingleton<IServiceProviderFactory<ServiceRegistry>, LamarServiceProviderFactory>();
                s.AddSingleton<IServiceProviderFactory<IServiceCollection>, LamarServiceProviderFactory>();

                s.AddSingleton<IStartupFilter>(new RegisterJasperStartupFilter());
            });

            return builder;
        }

        public static readonly string JasperHasBeenApplied = "JasperHasBeenApplied";


        /// <summary>
        /// Add Jasper's middleware to the application's RequestDelegate pipeline
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IApplicationBuilder UseJasper(this IApplicationBuilder app)
        {
            if (app.HasJasperBeenApplied())
                throw new InvalidOperationException("Jasper has already been applied to this web application");

            return Router.BuildOut(app);


        }

        internal static void MarkJasperHasBeenApplied(this IApplicationBuilder builder)
        {
            if (!builder.Properties.ContainsKey(JasperHasBeenApplied))
                builder.Properties.Add(JasperHasBeenApplied, true);
        }

        internal static bool HasJasperBeenApplied(this IApplicationBuilder builder)
        {
            return builder.Properties.ContainsKey(JasperHasBeenApplied);
        }
    }

    internal class RegisterJasperStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                var logger = app.ApplicationServices.GetRequiredService<ILogger<HttpSettings>>();

                app.Use(inner =>
                {
                    return c =>
                    {
                        try
                        {
                            return inner(c);
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e, $"Failed during an HTTP request for {c.Request.Method}: {c.Request.Path}");
                            c.Response.StatusCode = 500;
                            return c.Response.WriteAsync(e.ToString());
                        }
                    };
                });
                next(app);
                if (!app.HasJasperBeenApplied())
                {
                    Router.BuildOut(app).Run(c =>
                    {
                        c.Response.StatusCode = 404;
                        c.Response.Headers["status-description"] = "Resource Not Found";
                        return c.Response.WriteAsync("Resource Not Found");
                    });
                }
            };

        }
    }

    internal class JasperActivator : IHostedService
    {
        private readonly JasperOptionsBuilder _registry;
        private readonly IMessagingRoot _root;
        private readonly IContainer _container;

        public JasperActivator(JasperOptionsBuilder registry, IMessagingRoot root, IContainer container)
        {
            _registry = registry;
            _root = root;
            _container = container;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _registry.Messaging.Compiling;

            _root.Activate(_registry.Messaging.LocalWorker, _registry.CodeGeneration, _container);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
