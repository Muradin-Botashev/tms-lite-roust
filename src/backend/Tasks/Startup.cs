using Domain.Shared.UserProvider;
using Infrastructure.Installers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Tasks.Common;
using Tasks.Notifications;
using Tasks.Notifications.Generators;
using Tasks.Orders;
using Tasks.Services;
using Tasks.Shippings;
using Tasks.Statistics;
using Tasks.SystemTasks;
using ZNetCS.AspNetCore.Authentication.Basic;
using ZNetCS.AspNetCore.Authentication.Basic.Events;

namespace Tasks
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var configuration = CreateConfiguration();

            Log.Logger = Infrastructure.Logging.LoggerFactory.CreateLogger(configuration, "Tasks");

            services.AddDomain(configuration, false);
            services.AddScoped<IUserProvider, TasksUserProvider>();

            services.AddHostedService<ScheduleWorker>();

            services.AddScoped<INotificationGenerator, AddOrdersToShippingGenerator>();
            services.AddScoped<INotificationGenerator, CancelShippingGenerator>();
            services.AddScoped<INotificationGenerator, RejectShippingRequestGenerator>();
            services.AddScoped<INotificationGenerator, RemoveOrdersFromShippingGenerator>();
            services.AddScoped<INotificationGenerator, SendRequestToCarrierGenerator>();
            services.AddScoped<INotificationGenerator, UpdateShippingRequestDataGenerator>();

            //services.AddScoped<IScheduledTask, ImportOrderTask>();
            //services.AddScoped<IScheduledTask, ImportProductsTask>();
            services.AddScoped<IScheduledTask, SendNotificationsTask>();
            services.AddScoped<IScheduledTask, CheckPoolingSlotsTask>();
            services.AddScoped<IScheduledTask, ArchiveOrdersTask>();
            services.AddScoped<IScheduledTask, ClearObsoleteAutogroupingsTask>();
            services.AddScoped<IScheduledTask, SendNotificationsTask>();
            services.AddScoped<IScheduledTask, CheckRequestsOverdueTask>();

            services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
                .AddBasicAuthentication(options =>
                {
                    options.Realm = "TMS Roust";
                    options.Events = new BasicAuthenticationEvents
                    {
                        OnValidatePrincipal = context =>
                        {
                            var appConfiguration = context.HttpContext.RequestServices.GetService<IConfiguration>();
                            var username = appConfiguration.GetValue<string>("Web:Username");
                            var password = appConfiguration.GetValue<string>("Web:Password");

                            if ((context.UserName == username) && (context.Password == password))
                            {
                                var claims = new List<Claim>
                                {
                                    new Claim(ClaimTypes.Name, context.UserName, context.Options.ClaimsIssuer)
                                };
                                context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider, IConfiguration configuration, IApplicationLifetime applicationLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Map("/state", HandleStatistics);

            app.UseMvc();

            app.UseAuthentication();
        }

        private static void HandleStatistics(IApplicationBuilder app)
        {
            app.Run(StatisticsHandler.Execute);
        }

        private static IConfiguration CreateConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
