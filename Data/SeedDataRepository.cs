using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;
using StackExchange.Profiling.Storage;

namespace luke_site_mvc.Data
{
    //public interface ISeedDataRepository
    //{
    //    Task<IEnumerable<Chatroom>> DownloadJSON();
    //    Task Initialize();
    //    Task ResetDatabase();
    //    Task SeedDatabase(IReadOnlyList<Chatroom> chatrooms);
    //    Task SetupTablesMiniProfiler();
    //}
    //public class SeedDataRepository : ISeedDataRepository
    public class SeedDataRepository
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _client;

        //private readonly HttpClient _client = new HttpClient();

        private readonly string _database_seed_json_url = "https://www.lukebrown.us/database_seed.json/";
        private readonly ChatroomContext _chatroomContext;

        public SeedDataRepository(ChatroomContext chatroomContext, HttpClient client, IConfiguration config)
        {
            _chatroomContext = chatroomContext;
            _client = client;
            _config = config;
        }

        //private readonly HttpClient _client = new HttpClient();
        //private  IConfiguration _config;
        public async Task Initialize()
        //public async Task Initialize()
        {
            await _chatroomContext.Database.EnsureCreatedAsync();

            //var existing = await _chatroomContext.Chatrooms.

            if (!_chatroomContext.Chatrooms.Any())
            {
                //await ResetDatabase();

                //_chatroomContext.

                var chatrooms = await DownloadJSON();

                _chatroomContext.Chatrooms.AddRange(chatrooms);
                //await _chatroomContext.Chatrooms.

                //await SeedDatabase(chatrooms);

                _chatroomContext.SaveChanges();

            }

            //var config = serviceProvider.GetRequiredService(IConfiguration);
            //var client = serviceProvider.GetRequiredService<>
            //_config = serviceProvider.GetRequiredService<IConfiguration>();


            //await SetupTablesMiniProfiler();
        }

        public async Task ResetDatabase()
        {
            string sql =
            @$"
                DROP TABLE Chatrooms;
                
                CREATE TABLE Chatrooms
                (Id     INT NOT NULL PRIMARY KEY,
                Name   VARCHAR(100),
                Link    VARCHAR(15));
            ";

            using IDbConnection connection = new ProfiledDbConnection(
                new SqlConnection(_config.GetConnectionString("DefaultConnection")),
                MiniProfiler.Current
                );

            await connection.ExecuteAsync(sql);
        }

        public async Task SeedDatabase(IReadOnlyList<Chatroom> chatrooms)
        {
            string sql = @"INSERT INTO Chatrooms (Id,Name,Link) VALUES (@Id, @Name, @Link)";

            using IDbConnection connection = new ProfiledDbConnection(
                new SqlConnection(_config.GetConnectionString("DefaultConnection")),
                MiniProfiler.Current
                );

            foreach (var chatroom in chatrooms)
            {
                await connection.ExecuteAsync(sql, new
                {
                    Id = chatroom.Id,
                    Name = chatroom.Name,
                    Link = chatroom.Link
                });
            }
        }

        // TODO: move into it's own class?
        public async Task SetupTablesMiniProfiler()
        {
            SqlServerStorage sqlServerStorage = new SqlServerStorage("");
            List<string> miniProfilerTableCreationScripts = sqlServerStorage.TableCreationScripts;

            //string createDatabaseCmd = @"CREATE DATABASE MiniProfilerLogs";

            using IDbConnection connection = new ProfiledDbConnection(
                new SqlConnection(_config.GetConnectionString("DefaultConnection")),
                MiniProfiler.Current
                );

            //await connection.ExecuteAsync(createDatabaseCmd);

            // list of commands needs to be made into a single string
            var tableCreateCmd = String.Join(" ", miniProfilerTableCreationScripts);

            await connection.ExecuteAsync(tableCreateCmd);
        }
        public async Task<IEnumerable<Chatroom>> DownloadJSON()
        {
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
