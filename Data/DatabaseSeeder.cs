﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StackExchange.Profiling.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace luke_site_mvc.Data
{
    public interface IDatabaseSeeder
    {
        Task InitializeAsync();
        Task SetupTablesMiniProfiler(DbContext context);
    }
    public class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly SubredditContext _subredditContext;
        private readonly HttpClient _client;

        public DatabaseSeeder(SubredditContext SubredditContext, HttpClient client, IConfiguration config, IWebHostEnvironment webHostEnvironment, IServiceProvider serviceProvider)
        {
            _subredditContext = SubredditContext;
            _client = client;
        }

        public async Task InitializeAsync()
        {
            _subredditContext.Database.EnsureCreated();

            if (!_subredditContext.RedditComments.Any())
            {
                // create tables that allow MiniProfiler to log to database
                await SetupTablesMiniProfiler(_subredditContext);

                await _subredditContext.SaveChangesAsync();
            }
        }

        // TODO: figure out how to prevent table already exists error, but let previous data persist
        public async Task SetupTablesMiniProfiler(DbContext context)
        {
            string dropTablesCmd = @"DROP TABLE IF EXISTS MiniProfilers, MiniProfilerClientTimings, MiniProfilerTimings";
            await context.Database.ExecuteSqlRawAsync(dropTablesCmd);

            // get table creation sql commands from MiniProfiler
            SqlServerStorage sqlServerStorage = new SqlServerStorage("");
            List<string> miniProfilerTableCreationScripts = sqlServerStorage.TableCreationScripts;

            // list of commands needs to be made into a single string
            var tableCreateCmd = String.Join(" ", miniProfilerTableCreationScripts);

            await context.Database.ExecuteSqlRawAsync(tableCreateCmd);
        }
    }
}
