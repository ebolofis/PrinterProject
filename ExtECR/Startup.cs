using System;
using System.IO;
using System.Reflection;
using ExtECR.Filters;
using ExtECRMainLogic.Classes;
using ExtECRMainLogic.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ExtECR
{
    public class Startup
    {
        private readonly string CurrentPath;
        public readonly IConfiguration Configuration;

        public Startup(IConfiguration configuration)
        {
            CurrentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);
            });

            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
            })
                .SetCompatibilityVersion(CompatibilityVersion.Latest)
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                    options.JsonSerializerOptions.DictionaryKeyPolicy = null;
                });

            services.AddControllersWithViews(options =>
            {
                options.Filters.Add(typeof(VersionFilter));
            }).AddRazorRuntimeCompilation();


            services.AddSignalR();

            services.AddSingleton<PersistManager>();
            services.AddSingleton<ExtECRInitializer>();
            services.AddSingleton<ExtECRDriver>();
            services.AddSingleton<ExtECRDisplayer>();
            services.AddSingleton<LocalHubInvoker>();

            services.AddScoped<AuthorizationHelper>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, PersistManager pm)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Login}/{id?}");
                endpoints.MapHub<LocalHub>("/LocalHub");
            });

            pm.SetApplicationData(CurrentPath, app);
        }
    }
}