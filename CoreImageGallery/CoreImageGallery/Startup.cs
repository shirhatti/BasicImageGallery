﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CoreImageGallery.Data;
using CoreImageGallery.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.ApplicationInsights.Extensibility.EventCounterCollector;

namespace CoreImageGallery
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //for asp.net core 3.0 MVC
            services.AddControllers();

            services.AddControllersWithViews();
            services.AddRazorPages();


            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc()
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizeFolder("/Account/Manage");
                    options.Conventions.AuthorizePage("/Account/Logout");
                    //options.Conventions.AuthorizePage("/Upload");
                });

            services.AddScoped<IStorageService, AzStorageService>();
            //services.AddScoped<IStorageService, FileStorageService>();
            services.AddScoped<IImageProvider, WatermarkedImageProvider>();

            // Register no-op EmailSender used by account confirmation and password reset during development
            // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=532713
            services.AddSingleton<IEmailSender, EmailSender>();

            services.AddHttpClient();
            services.AddApplicationInsightsTelemetry();

            services.ConfigureTelemetryModule<EventCounterCollectionModule>((module, options) =>
            {
                module.Counters.Add(new EventCounterCollectionRequest("CoreImageGallery", "images-downloaded"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");

                //Run Entity Core migrations on start
                using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
                {
                    scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
                }
            }
            //asp.net 3.0 fixes for MVC and routing
            app.UseRouting();

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseHttpsRedirection();

            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();

                //endpoints.MapHub<ChatHub>("/chat");
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

            //app.UseMvc();
        }
    }
}
