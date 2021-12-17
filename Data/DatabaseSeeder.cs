using Microsoft.EntityFrameworkCore;
using StackExchange.Profiling.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public DatabaseSeeder(SubredditContext SubredditContext)
        {
            _subredditContext = SubredditContext;
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
            PostgreSqlStorage postgreSqlStorage = new PostgreSqlStorage("");
            List<string> miniProfilerTableCreationScripts = postgreSqlStorage.TableCreationScripts;

            // list of commands needs to be made into a single string
            var tableCreateCmd = String.Join(" ", miniProfilerTableCreationScripts);

            await context.Database.ExecuteSqlRawAsync(tableCreateCmd);
        }
    }
}
