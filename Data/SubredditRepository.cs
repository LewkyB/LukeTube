using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace luke_site_mvc.Data
{
    public interface ISubredditRepository
    {
        Task<IReadOnlyList<Chatroom>> GetAllLinks();
        Task<IReadOnlyList<string>> GetAllChatNames();
        Task<IReadOnlyList<string>> GetChatLinksByChat(string chatName);
    }

    public class SubredditRepository : ISubredditRepository
    {
        private readonly ILogger<SubredditRepository> _logger;
        private readonly IConfiguration _config;
        private readonly SubredditContext _subredditContext;

        public SubredditRepository(IConfiguration config, ILogger<SubredditRepository> logger, SubredditContext subredditContext)
        {
            _config = config;
            _logger = logger;
            _subredditContext = subredditContext;
        }

        public async Task<IReadOnlyList<Chatroom>> GetAllLinks()
        {
            return await _subredditContext.Chatrooms
                .ToListAsync();
        }
        public async Task<IReadOnlyList<string>> GetAllChatNames()
        {
            return await _subredditContext.Chatrooms
                .Where(chatroom => chatroom.Name != " ")
                .Select(chatroom => chatroom.Name).Distinct()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<string>> GetChatLinksByChat(string chatName)
        {
            return await _subredditContext.Chatrooms
                .OrderByDescending(chatroom => chatroom.Id)
                .Where(chatroom => chatroom.Name == chatName)
                .Select(chatroom => chatroom.Link)
                .ToListAsync();
        }

    }
}
