using System.Collections.Generic;
using System.Linq;
using luke_site_mvc.Data;
using Microsoft.Extensions.Logging;

namespace luke_site_mvc.Services
{
    public interface IChatroomService
    {
        IReadOnlyList<string> GetAllChatNames();
        IReadOnlyList<Chatroom> GetAllLinks();
        IReadOnlyList<string> GetChatLinksByChat(string chatName);
    }

    // TODO: make this have more of a point than just returning readonlylists
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

        public IReadOnlyList<string> GetAllChatNames()
        {
            return _dataRepository.GetAllChatNames().ToList();
        }

        public IReadOnlyList<Chatroom> GetAllLinks()
        {
            return _dataRepository.GetAllLinks().ToList();
        }

        public IReadOnlyList<string> GetChatLinksByChat(string chatName)
        {
            return _dataRepository.GetChatLinksByChat(chatName).ToList();
        }

    }
}
