using API.Extensions;
using Application.Services.Identity;
using Application.Shared.UserProvider;
using Domain.Enums;
using Domain.Extensions;
using Domain.Shared.UserProvider;
using Elastic.Apm.NetCoreAll;
using Infrastructure.Installers;
using Infrastructure.Logging;
using Infrastructure.Translations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ZNetCS.AspNetCore.Authentication.Basic.Events;

namespace API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Log.Logger = LoggerFactory.CreateLogger(Configuration, "API");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = SigningOptions.SignIssuer,
                        ValidAudience = SigningOptions.SignAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SigningOptions.SignKey)),
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero
                    };
                })
                .AddBasicAuthentication(options =>
                {
                    options.Realm = "TMS Roust OpenAPI";
                    options.Events = new BasicAuthenticationEvents
                    {
                        OnValidatePrincipal = BasicPrincipalValidation.Validate
                    };
                });

            var permissions = (RolePermissions[])Enum.GetValues(typeof(RolePermissions));

            services.AddAuthorization(options =>
            {
                permissions.ToList().ForEach(permission =>
                {
                    options.AddPolicy(permission.GetPermissionName(),
                        policy => policy.RequireClaim(RolePermissionExtension.ClaimType, permission.GetPermissionName()));
                });

                options.AddPolicy(ApiExtensions.BasicApiPolicy,
                    policy => policy.RequireClaim(ApiExtensions.ApiLevelClaim, ApiExtensions.ApiLevel.Basic.ToString()));
                options.AddPolicy(ApiExtensions.OpenApiPolicy,
                    policy => policy.RequireClaim(ApiExtensions.ApiLevelClaim, ApiExtensions.ApiLevel.Open.ToString()));
            });

            string version = GetMajorVersion();

            services.AddMvc(options => 
            {
                options.Conventions.Add(new AuthorizeByDefaultConvention());
            })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc($"v{version}", new Info
                {
                    Version = $"v{version}",
                    Title = "Artlogic TMS API",
                    Description = "API for Artlogic TMS"
                });

                c.IncludeXmlComments(GetXmlCommentsPath());
            });
            
            services.AddHttpContextAccessor();

            services.AddDomain(Configuration, true);

            services.SyncTranslations();

            services.AddScoped<IUserProvider, UserProvider>();
        }

        private static string GetXmlCommentsPath()
        {
            return Path.Combine(AppContext.BaseDirectory, "Swagger.XML");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
        {
            app.UseAllElasticApm(Configuration);

            if (env.IsDevelopment()) 
                app.UseDeveloperExceptionPage();

            app.UseAuthentication();

            app.UseRequestLogging();

            app.UseMvc((routes) =>
            {
                routes.MapRoute(
                    name: "DefaultApi",
                    template: "api/{controller}/{action}");
            });

            app.UseStaticFiles("/api/static");

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                string version = GetMajorVersion();
                c.SwaggerEndpoint($"/swagger/v{version}/swagger.json", $"Artlogic TMS API v{version}");
            });

            lifetime.ApplicationStopped.Register(OnAppStopped);
        }

        public void OnAppStopped()
        {
            Log.CloseAndFlush();
        }

        private string GetMajorVersion()
        {
            string versionString = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
            return versionString;
        }
    }
}
