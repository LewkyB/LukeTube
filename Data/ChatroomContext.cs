using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace luke_site_mvc.Data
{
    public class ChatroomContext : DbContext
    {
        static readonly HttpClient client = new HttpClient();
        private readonly string _database_seed_json_url = "https://www.lukebrown.us/database_seed.json/";

        public ChatroomContext(DbContextOptions<ChatroomContext> options) : base(options)
        {
        }

        public DbSet<Chatroom> Chatrooms { get; set; }

        protected override async void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);
            ChatroomData chatroomData;

            //chatroomData = await DownloadJSON();

            using (var response = await client.GetAsync(_database_seed_json_url, HttpCompletionOption.ResponseHeadersRead))
            using (var responseStream = await response.Content.ReadAsStreamAsync())
            using (var textReader = new StreamReader(responseStream))
            using (var reader = new JsonTextReader(textReader))
            {
                var serializer = new JsonSerializer();
                chatroomData = serializer.Deserialize<ChatroomData>(reader);
            }

            modelBuilder.Entity<Chatroom>(o =>
           {
               o.HasData( new ChatroomData() { chatroom = chatroomData.chatroom });
           });

            //modelBuilder.Entity<Chatroom>();

            //base.OnModelCreating(modelBuilder);
            }

        }
        //public async Task<ChatroomData> DownloadJSON()
        //{
        //    using (var response = await client.GetAsync(_database_seed_json_url, HttpCompletionOption.ResponseHeadersRead))
        //    {
        //        var responseStream = await response.Content.ReadAsStreamAsync();

        //        var textReader = new StreamReader(responseStream);
        //        var reader = new JsonTextReader(textReader);
                
        //        var serializer = new JsonSerializer();

        //        var result = await JToken.ReadFromAsync(reader);

        //        return result.ToObject<ChatroomData>(serializer);
        //    }
        //}
    }

