using System;
using System.Linq;
using System.Collections.Generic;
using MediatR;
using AlpineSkiHouse.Data;
using AlpineSkiHouse.Models;
using AlpineSkiHouse.Services;
using AlpineSkiHouse.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Reflection;
using AlpineSkiHouse.Configuration;
using Serilog;
using Serilog.Filters;
using Serilog.Core;
using AlpineSkiHouse.Configuration.Models;
using Microsoft.Extensions.Options;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using AlpineSkiHouse.Conventions;
using Microsoft.Extensions.Hosting;

namespace AlpineSkiHouse
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            Configuration = AlpineConfigurationBuilder.Build(env);
            CurrentEnvironment = env;
        }

        public IConfigurationRoot Configuration { get; }

        public IWebHostEnvironment CurrentEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Startup).Assembly));

            services.AddDbContext<ApplicationUserContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDbContext<SkiCardContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDbContext<PassContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDbContext<PassTypeContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDbContext<ResortContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationUserContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication()
                .AddFacebook(options =>
                {
                    options.AppId = Configuration["Authentication:Facebook:AppId"];
                    options.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
                })
                .AddTwitter(options =>
                {
                    options.ConsumerKey = Configuration["Authentication:Twitter:ConsumerKey"];
                    options.ConsumerSecret = Configuration["Authentication:Twitter:ConsumerSecret"];
                });

            services.Configure<AzureStorageSettings>(Configuration.GetSection("MicrosoftAzureStorage"));
            services.AddTransient<IBlobFileUploadService, BlobFileUploadService>();

            services.AddSingleton<IAuthorizationHandler, EditSkiCardAuthorizationHandler>();

            services.AddOptions();
            services.Configure<CsrInformationOptions>(Configuration.GetSection("CsrInformationOptions"));
            services.AddScoped<ICsrInformationService, CsrInformationService>();

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddMvc(mvcOptions =>
            {
                mvcOptions.Conventions.Add(new AutoValidateAntiForgeryTokenModelConvention());
            })
                .AddViewLocalization()
                .AddDataAnnotationsLocalization();

            var supportedCultures = new[]
            {
                new CultureInfo("en-CA"),
                new CultureInfo("fr-CA")
            };
            
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("en-CA");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            if (!CurrentEnvironment.IsDevelopment())
            {
                services.Configure<MvcOptions>(options =>
                {
                    options.Filters.Add(new RequireHttpsAttribute());
                });
            }
            
            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IHostApplicationLifetime applicationLifetime, IOptions<RequestLocalizationOptions> requestLocalizationOptions)
        {
            // Serilog config
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()                
                .MinimumLevel.Override("AlpineSkiHouse", Serilog.Events.LogEventLevel.Debug)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Seq("http://localhost:5341")
                .CreateLogger();

            applicationLifetime.ApplicationStopped.Register(Log.CloseAndFlush);
            // end of Serilog config

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Add external authentication verification
            if (Configuration["Authentication:Facebook:AppId"] == null ||
                Configuration["Authentication:Facebook:AppSecret"] == null ||
                Configuration["Authentication:Twitter:ConsumerKey"] == null ||
                Configuration["Authentication:Twitter:ConsumerSecret"] == null)
            {
                throw new KeyNotFoundException("A configuration value is missing for authentication against Facebook and Twitter. While you don't need to get tokens for these you do need to set up your user secrets as described in the readme.");
            }
            
            app.UseRequestLocalization(requestLocalizationOptions.Value);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
