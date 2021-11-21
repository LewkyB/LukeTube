using System;
using System.IO;
using System.Reflection;
using luke_site_mvc.Data;
using luke_site_mvc.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using StackExchange.Profiling.Storage;

namespace luke_site_mvc
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddHttpClient<IDatabaseSeeder>();
            services.AddHttpClient<IPsawService>();

            services.AddHttpClient("timeout", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            services.AddHttpContextAccessor();

            services.AddScoped<ISubredditRepository, SubredditRepository>();
            services.AddScoped<ISubredditService, SubredditService>();
            services.AddScoped<IPushshiftService, PushshiftService>();
            services.AddScoped<IPsawService, PsawService>();
            services.AddTransient<IDatabaseSeeder, DatabaseSeeder>();

            services.AddControllersWithViews();

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = Configuration.GetConnectionString("Redis");
                options.InstanceName = "IRCTube_";
            });

            services.AddDbContext<SubredditContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
                options.EnableSensitiveDataLogging();
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            },
            ServiceLifetime.Transient);

            services.AddSwaggerGen(options =>
           {
               options.SwaggerDoc("v1", new OpenApiInfo { Title = "luke_site_mvc", Version = "v1" });

               var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
               var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
               options.IncludeXmlComments(xmlPath);
           });

            services.AddMiniProfiler(options =>
            {
                options.RouteBasePath = "/profiler";
                (options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes(60);
                options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();
                options.TrackConnectionOpenClose = true;
                options.ColorScheme = StackExchange.Profiling.ColorScheme.Auto;
                options.PopupRenderPosition = StackExchange.Profiling.RenderPosition.BottomLeft;
                options.EnableServerTimingHeader = true;
                options.EnableMvcFilterProfiling = true;
                options.EnableMvcViewProfiling = true;
                options.Storage = new SqlServerStorage(Configuration.GetConnectionString("DefaultConnection"));
            }).AddEntityFramework();

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddResponseCaching();

            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IHostApplicationLifetime lifetime, IDistributedCache cache,
            SubredditContext chatroomContext)
        {
            //lifetime.ApplicationStarted.Register(() =>
            //{
            //    var currentTimeUTC = DateTime.UtcNow.ToString();

            //    byte[] encodedCurrentTimeUTC = Encoding.UTF8.GetBytes(currentTimeUTC);

            //    var options = new DistributedCacheEntryOptions()
            //        .SetSlidingExpiration(TimeSpan.FromSeconds(20));

            //    cache.Set("cachedTimeUTC", encodedCurrentTimeUTC, options);
            //});

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();

                app.UseMiniProfiler();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseResponseCaching();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("Fallback",
                    "{controller}/{action}/{id?}",
                    new { controller = "Subreddit", action = "Index" });
                //endpoints.MapDefaultControllerRoute

                endpoints.MapRazorPages();
            });
        }
    }
}
