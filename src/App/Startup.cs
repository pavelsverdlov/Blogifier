using Askmethat.Aspnet.JsonLocalizer.Extensions;
using Core;
using Core.Data;
using Core.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Rewrite;
using App.Localization;
using Core.Services;
using Comments;
using Microsoft.AspNetCore.Http;
using Comments.Contracts;
using System.Threading.Tasks;
using App.Comments;

namespace App
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            //Log.Logger = new LoggerConfiguration()
            //  .Enrich.FromLogContext()
            //  .WriteTo.RollingFile("Logs/{Date}.txt", LogEventLevel.Warning)
            //  .CreateLogger();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var section = Configuration.GetSection("Blogifier");

            services.AddAppSettings<AppItem>(section);

            if (section.GetValue<string>("DbProvider") == "SqlServer")
            {
                AppSettings.DbOptions = options => options.UseSqlServer(section.GetValue<string>("ConnString"));
            }
            else if (section.GetValue<string>("DbProvider") == "MySql")
            {
                AppSettings.DbOptions = options => options.UseMySql(section.GetValue<string>("ConnString"));
            }
            else
            {
                AppSettings.DbOptions = options => options.UseSqlite(section.GetValue<string>("ConnString"));
            }


            services
                .AddDbContext<AppDbContext>(AppSettings.DbOptions, ServiceLifetime.Transient); 

            services.AddIdentity<AppUser, IdentityRole>(options => {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 4;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.User.AllowedUserNameCharacters = null;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            services.AddLogging(loggingBuilder =>
                loggingBuilder.AddSerilog(dispose: true));

            services.AddJsonLocalization(options => {
                //"Resources" by default
                //options.ResourcesPath;
            });

            services.ConfigureLocalizationOptions();

            services.AddRouting(options => {
                options.LowercaseUrls = true;
                options.AppendTrailingSlash = true;
            });

            services.AddMvc()
            .AddViewLocalization()
            .ConfigureApplicationPartManager(p =>
            {
                foreach (var assembly in AppConfig.GetAssemblies())
                {
                    p.ApplicationParts.Add(new AssemblyPart(assembly));
                }
            })
            .AddRazorPagesOptions(options =>
            {
                options.Conventions.AuthorizeFolder("/Admin");
            })
            .AddApplicationPart(typeof(Core.Api.AuthorsController).GetTypeInfo().Assembly)
            .AddControllersAsServices()
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSwaggerGen(setupAction => {
                setupAction.SwaggerDoc("spec",
                    new Microsoft.OpenApi.Models.OpenApiInfo()
                    {
                        Title = "Blogifier API",
                        Version = "1"
                    });
                setupAction.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "CoreAPI.xml"));
            });

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "Custom/themes/custom";
            });
            services.AddCors(c =>
            {
                c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin());
            });

            services.AddAppServices();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            app.UseHttpsRedirection();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            // app.UseResponseCompression();

            //custom middleware
            app.UseCustomComments();
            app.UseRequestLocalization();
            app.UseMiddleware<LocalizationMiddleware>(new SectionsToIgnoreLocalization("/admin/", "/api/", "/account/"));

            app.UseSwagger();
            app.UseSwaggerUI(setupAction =>
            {
                setupAction.SwaggerEndpoint(
                    "/swagger/spec/swagger.json",
                    "Blogifier API"
                );
            });

            AppSettings.WebRootPath = env.WebRootPath;
            AppSettings.ContentRootPath = env.ContentRootPath;
            //app.UseMvc();

            app.UseMvc(routes => {
                routes.MapRoute(
                    name: "default",
                    template: "{culture}/{controller}/{action}/{id?}",
                    defaults: new {
                        culture = "en",
                        controller = "Blog",
                        action = "Index"
                    }
                );
            });

            app.UseSpa(spa => { });
            app.UseCors(options => options.AllowAnyOrigin());
        }
    }

    
}