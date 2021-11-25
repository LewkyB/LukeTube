using luke_site_mvc.Data;
using luke_site_mvc.Services;
using luke_site_mvc.Services.BackgroundServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using StackExchange.Profiling.Storage;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;

namespace luke_site_mvc
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                                                                            retryAttempt)));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddHttpClient<IDatabaseSeeder>();

            // figure out how to rate limit calls to pushshift to 120/min
            services.AddHttpClient<IPsawService>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(2)).AddPolicyHandler(GetRetryPolicy());

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

            // TODO: add a feature toggle for this
            services.AddHostedService<PushshiftBackgroundService>();

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

            services.AddW3CLogging(logging =>
            {
                logging.LoggingFields = W3CLoggingFields.All;

                logging.FileSizeLimit = 5 * 1024 * 1024;
                logging.RetainedFileCountLimit = 2;
                logging.FileName = "MyLogFile";
                logging.LogDirectory = @"C:\logs";
                logging.FlushInterval = TimeSpan.FromSeconds(2);
            });

            services.AddResponseCaching();

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddRazorPages();
        }

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

                app.UseW3CLogging();

                app.UseStatusCodePages();
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

                endpoints.MapRazorPages();
            });
        }
    }
}
