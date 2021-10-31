using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using luke_site_mvc.Data;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace luke_site_mvc.Services
{
    public interface IChatroomService
    {
        //Task<IReadOnlyList<Chatroom>> DownloadJSON();
        Task<IReadOnlyList<string>> GetAllChatNames();
        Task<IReadOnlyList<Chatroom>> GetAllLinks();
        Task<IReadOnlyList<string>> GetChatLinksByChat(string chatName);
        //Task InitializeDatabase();
    }

    // TODO: make this have more of a point than just returning readonlylists
    public class ChatroomService : IChatroomService
    {
        private readonly IDataRepository _dataRepository;
        private readonly ILogger<ChatroomService> _logger;
        //private readonly HttpClient _client;

        //private readonly string _database_seed_json_url = "https://www.lukebrown.us/database_seed.json/";

        //public ChatroomService(ILogger<ChatroomService> logger, IDataRepository dataRepository, HttpClient client)
        public ChatroomService(ILogger<ChatroomService> logger, IDataRepository dataRepository)
        {
            _dataRepository = dataRepository;

            _logger = logger;
            _logger.LogDebug(1, "NLog injected into ChatroomService");
        }

        public async Task<IReadOnlyList<string>> GetAllChatNames()
        {
            return (await _dataRepository.GetAllChatNames()).ToList();
        }

        public async Task<IReadOnlyList<Chatroom>> GetAllLinks()
        {
            return (await _dataRepository.GetAllLinks()).ToList();
        }

        public async Task<IReadOnlyList<string>> GetChatLinksByChat(string chatName)
        {
            return (await _dataRepository.GetChatLinksByChat(chatName)).ToList();

        }

        //public async Task InitializeDatabase()
        //{
        //    // drop tables and create tables
        //    await _dataRepository.ResetDatabase();

        //    await _dataRepository.SetupTablesMiniProfiler();

        //    // fetch data from json
        //    var json_chatroom_data = await DownloadJSON();

        //    // insert data into database
        //    await _dataRepository.SeedDatabase(json_chatroom_data);
        //}

        //public async Task<IReadOnlyList<Chatroom>> DownloadJSON()
        //{
        //    using (var response = await _client.GetAsync(_database_seed_json_url, HttpCompletionOption.ResponseHeadersRead))
        //    {
        //        var responseStream = await response.Content.ReadAsStreamAsync();

        //        var textReader = new StreamReader(responseStream);
        //        var reader = new JsonTextReader(textReader);
                
        //        var serializer = new JsonSerializer();

        //        var result = await JToken.ReadFromAsync(reader);

        //        return result.ToObject<IReadOnlyList<Chatroom>>(serializer);
        //    }
        //}

    }
}
