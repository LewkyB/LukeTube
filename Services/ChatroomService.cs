using System.Collections.Generic;
using System.Threading.Tasks;
using luke_site_mvc.Data;
using Microsoft.Extensions.Logging;

namespace luke_site_mvc.Services
{
    public interface IChatroomService
    {
        Task<IReadOnlyList<string>> GetAllChatNames();
        Task<IReadOnlyList<Chatroom>> GetAllLinks();
        Task<IReadOnlyList<string>> GetChatLinksByChat(string chatName);
    }

    // TODO: make this have more of a point than just returning readonlylists
    // TODO: move repository functions to ChatroomContext and just use that here? point of repository pattern?
    public class ChatroomService : IChatroomService
    {
        private readonly IDataRepository _dataRepository;
        private readonly ILogger<ChatroomService> _logger;

        public ChatroomService(ILogger<ChatroomService> logger, IDataRepository dataRepository)
        {
            _dataRepository = dataRepository;

            _logger = logger;
            _logger.LogDebug(1, "NLog injected into ChatroomService");
        }

        public async Task<IReadOnlyList<string>> GetAllChatNames()
        {
            return await _dataRepository.GetAllChatNames();
        }

        public async Task<IReadOnlyList<Chatroom>> GetAllLinks()
        {
            return await _dataRepository.GetAllLinks();
        }

        public async Task<IReadOnlyList<string>> GetChatLinksByChat(string chatName)
        {
            return await _dataRepository.GetChatLinksByChat(chatName);

        }
    }
}
