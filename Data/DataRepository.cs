using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace luke_site_mvc.Data
{
    public interface IDataRepository
    {
        Task<IReadOnlyList<Chatroom>> GetAllLinks();
        Task<IReadOnlyList<string>> GetAllChatNames();
        Task<IReadOnlyList<string>> GetChatLinksByChat(string chatName);
    }

    public class DataRepository : IDataRepository
    {
        private readonly ILogger<DataRepository> _logger;
        private readonly IConfiguration _config;
        private readonly ChatroomContext _chatroomContext;

        public DataRepository(IConfiguration config, ILogger<DataRepository> logger, ChatroomContext chatroomContext)
        {
            _config = config;
            _logger = logger;
            _chatroomContext = chatroomContext;
        }

        public async Task<IReadOnlyList<Chatroom>> GetAllLinks()
        {
            return await _chatroomContext.Chatrooms
                .ToListAsync();
        }
        public async Task<IReadOnlyList<string>> GetAllChatNames()
        {
            return await _chatroomContext.Chatrooms
                .Where(chatroom => chatroom.Name != " ")
                .Select(chatroom => chatroom.Name).Distinct()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<string>> GetChatLinksByChat(string chatName)
        {
            return await _chatroomContext.Chatrooms
                .OrderByDescending(chatroom => chatroom.Id)
                .Where(chatroom => chatroom.Name == chatName)
                .Select(chatroom => chatroom.Name)
                .ToListAsync();
        }

    }
}
