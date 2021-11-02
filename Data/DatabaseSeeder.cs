﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using StackExchange.Profiling.Storage;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace luke_site_mvc.Data
{
    public interface IDatabaseSeeder
    {
        Task<IEnumerable<Chatroom>> DownloadJSON();
        Task InitializeAsync();
        Task SetupTablesMiniProfiler(DbContext context);
    }
    public class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly ChatroomContext _chatroomContext;
        private readonly HttpClient _client;

        // links and chats collected from my weechat irc bouncer in json form
        private readonly string _database_seed_json_url = "https://www.lukebrown.us/database_seed.json/";

        public DatabaseSeeder(ChatroomContext chatroomContext, HttpClient client, IConfiguration config, IWebHostEnvironment webHostEnvironment, IServiceProvider serviceProvider)
        {
            _chatroomContext = chatroomContext;
            _client = client;
        }

        public async Task InitializeAsync()
        {
            _chatroomContext.Database.EnsureCreated();

            if (!_chatroomContext.Chatrooms.Any())
            {
                var chatrooms = await DownloadJSON();

                // load database with data
                await _chatroomContext.Chatrooms.AddRangeAsync(chatrooms.ToList());

                // create tables that allow MiniProfiler to log to database
                await SetupTablesMiniProfiler(_chatroomContext);

                await _chatroomContext.SaveChangesAsync();
            }
        }

        public async Task SetupTablesMiniProfiler(DbContext context)
        {
            // get table creation sql commands from MiniProfiler
            SqlServerStorage sqlServerStorage = new SqlServerStorage("");
            List<string> miniProfilerTableCreationScripts = sqlServerStorage.TableCreationScripts;

            // list of commands needs to be made into a single string
            var tableCreateCmd = String.Join(" ", miniProfilerTableCreationScripts);

            await context.Database.ExecuteSqlRawAsync(tableCreateCmd);
        }

        public async Task<IEnumerable<Chatroom>> DownloadJSON()
        {
            // This buffers the entire response into memory so that we don't
            // end up doing blocking IO when de-serializing the JSON. If the payload
            // is *HUGE* this could result in large allocations that lead to a Denial Of Service.
            using (var response = await _client.GetAsync(_database_seed_json_url, HttpCompletionOption.ResponseHeadersRead))
            {
                var responseStream = await response.Content.ReadAsStreamAsync();

                var textReader = new StreamReader(responseStream);
                var reader = new JsonTextReader(textReader);

                var serializer = new JsonSerializer();

                var result = await JToken.ReadFromAsync(reader);

                return result.ToObject<IEnumerable<Chatroom>>(serializer);
            }
        }
    }
}