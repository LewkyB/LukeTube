using System;
using System.IO;
using System.Reflection;
using System.Text;
using luke_site_mvc.Data;
using luke_site_mvc.Services;
using luke_site_mvc.Tests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

// TODO: feature flag or something to turn this on and off? or is env.IsDevelopment() ok?
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
            services.AddHttpClient<SeedDataRepository>();

            services.AddHttpClient("timeout", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            services.AddHttpContextAccessor();

            services.AddScoped<IDataRepository, DataRepository>();
            services.AddScoped<IChatroomService, ChatroomService>();
            //services.AddTransient<ISeedDataRepository, SeedDataRepository>();

            services.AddControllersWithViews();

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = Configuration.GetConnectionString("Redis");
                options.InstanceName = "IRCTube_";
            });

            services.AddDbContext<ChatroomContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
                options.EnableSensitiveDataLogging();
                //options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });
            //ServiceLifetime.Transient);
            //services.AddTransient<SeedDataRepository>();

            services.AddSwaggerGen(options =>
           {
               options.SwaggerDoc("v1", new OpenApiInfo { Title = "luke_site_mvc", Version = "v1" });

               var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
               var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
               options.IncludeXmlComments(xmlPath);
           });

            services.AddMiniProfiler(options =>
            {
                // All of this is optional. You can simply call .AddMiniProfiler() for all defaults

                // (Optional) Path to use for profiler URLs, default is /mini-profiler-resources
                options.RouteBasePath = "/profiler";

                // (Optional) Control storage
                // (default is 30 minutes in MemoryCacheStorage)
                // Note: MiniProfiler will not work if a SizeLimit is set on MemoryCache!
                //   See: https://github.com/MiniProfiler/dotnet/issues/501 for details
                (options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes(60);

                // (Optional) Control which SQL formatter to use, InlineFormatter is the default
                options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();

                // (Optional) You can disable "Connection Open()", "Connection Close()" (and async variant) tracking.
                // (defaults to true, and connection opening/closing is tracked)
                options.TrackConnectionOpenClose = true;

                // (Optional) Use something other than the "light" color scheme.
                // (defaults to "light")
                options.ColorScheme = StackExchange.Profiling.ColorScheme.Auto;

                options.PopupRenderPosition = StackExchange.Profiling.RenderPosition.BottomLeft;
                options.EnableServerTimingHeader = true;

                // The below are newer options, available in .NET Core 3.0 and above:

                // (Optional) You can disable MVC filter profiling
                // (defaults to true, and filters are profiled)
                options.EnableMvcFilterProfiling = true;
                // ...or only save filters that take over a certain millisecond duration (including their children)
                // (defaults to null, and all filters are profiled)
                // options.MvcFilterMinimumSaveMs = 1.0m;

                // (Optional) You can disable MVC view profiling
                // (defaults to true, and views are profiled)
                options.EnableMvcViewProfiling = true;
                // ...or only save views that take over a certain millisecond duration (including their children)
                // (defaults to null, and all views are profiled)
                // options.MvcViewMinimumSaveMs = 1.0m;

                // (Optional) listen to any errors that occur within MiniProfiler itself
                //options.OnInternalError = e => MyExceptionLogger(e);

                // (Optional - not recommended) You can enable a heavy debug mode with stacks and tooltips when using memory storage
                // It has a lot of overhead vs. normal profiling and should only be used with that in mind
                // (defaults to false, debug/heavy mode is off)
                //options.EnableDebugMode = true;
                //options.Storage = new SqlServerStorage(Configuration.GetConnectionString("MiniProfilerLogDbConnection"));
                options.Storage = new SqlServerStorage(Configuration.GetConnectionString("DefaultConnection"));
            }).AddEntityFramework();

            services.AddResponseCaching();

            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IHostApplicationLifetime lifetime, IDistributedCache cache,
            ChatroomContext chatroomContext)
        {
            lifetime.ApplicationStarted.Register(() =>
            {
                var currentTimeUTC = DateTime.UtcNow.ToString();

                byte[] encodedCurrentTimeUTC = Encoding.UTF8.GetBytes(currentTimeUTC);

                var options = new DistributedCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(20));

                cache.Set("cachedTimeUTC", encodedCurrentTimeUTC, options);
            });

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
                    new { controller = "Chatroom", action = "Index" });

                endpoints.MapRazorPages();
            });

            // force database seeding to execute
            chatroomContext.Database.EnsureCreated();
        }
    }
}
