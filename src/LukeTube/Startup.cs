using LukeTube.Data;
using LukeTube.Services;
using LukeTube.Services.BackgroundServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using StackExchange.Profiling.Storage;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using LukeTube.Services.PollyPolicies;

namespace LukeTube
{
    public class Startup
    {
        private string AllowSpecificOrigins = "_allowSpecificOrigins";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // rate limited to 60 requests per 60 seconds
            services.AddHttpClient("PushshiftClient")
                .SetHandlerLifetime(TimeSpan.FromMinutes(2))
                .AddPolicyHandler(PushshiftPolicies.GetWaitAndRetryPolicy())
                .AddPolicyHandler(PushshiftPolicies.GetRateLimitPolicy());

            services.AddScoped<ISubredditRepository, SubredditRepository>();
            services.AddScoped<ISubredditService, SubredditService>();
            services.AddScoped<IPushshiftService, PushshiftService>();
            services.AddScoped<IPsawService, PsawService>();
            services.AddTransient<IDatabaseSeeder, DatabaseSeeder>();

            services.AddHostedService<PushshiftBackgroundService>();

            //services.AddControllersWithViews();
            services.AddControllers();

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = Environment.GetEnvironmentVariable("CONNECTION_STRINGS__REDIS");
                options.InstanceName = "LukeTube_";
            });

            services.AddDbContext<SubredditContext>(options =>
            {
                options.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRINGS__POSTGRESQL"));
                options.EnableSensitiveDataLogging();
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            },
            ServiceLifetime.Transient);

            // services.AddSwaggerGen(options =>
            //{
            //    options.SwaggerDoc("v1", new OpenApiInfo { Title = "LukeTube", Version = "v1" });

            //    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            //    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            //    options.IncludeXmlComments(xmlPath);
            //});

            //services.AddMiniProfiler(options =>
            //{
            //    options.RouteBasePath = "/profiler";
            //    (options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes(60);
            //    options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();
            //    options.TrackConnectionOpenClose = true;
            //    options.ColorScheme = StackExchange.Profiling.ColorScheme.Auto;
            //    options.PopupRenderPosition = StackExchange.Profiling.RenderPosition.BottomLeft;
            //    options.EnableServerTimingHeader = true;
            //    options.EnableMvcFilterProfiling = true;
            //    options.EnableMvcViewProfiling = true;
            //    options.Storage = new PostgreSqlStorage(Environment.GetEnvironmentVariable("CONNECTION_STRINGS__POSTGRESQL"));

            //    // including these is not useful
            //    options.IgnoredPaths.Add("/js/");
            //    options.IgnoredPaths.Add("/css/");
            //    options.IgnoredPaths.Add("/lib/");
            //}).AddEntityFramework();

            //services.AddResponseCaching();

            services.AddCors(options =>
            {
                options.AddPolicy(name: AllowSpecificOrigins,
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:81/")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin();
                    });
            });

            services.AddDatabaseDeveloperPageExceptionFilter();

            //services.AddRazorPages();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IHostApplicationLifetime lifetime, IDistributedCache cache,
            SubredditContext chatroomContext)
        {
            lifetime.ApplicationStarted.Register(() =>
            {
                var currentTimeUtc = DateTime.UtcNow.ToString();

                var encodedCurrentTimeUtc = Encoding.UTF8.GetBytes(currentTimeUtc);

                var options = new DistributedCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(20));

                cache.Set("cachedTimeUTC", encodedCurrentTimeUtc, options);
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseSwagger();

                //app.UseMiniProfiler();

                app.UseStatusCodePages();
                app.UseCors(AllowSpecificOrigins);
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();

                // not sure if this is needed since nginx is handling https
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseResponseCaching();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
