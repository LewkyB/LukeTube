using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace luke_site_mvc.Data
{
    public interface IDataRepository
    {
        IEnumerable<Chatroom> GetAllLinks();
        IEnumerable<Chatroom> GetLinksByChatroom(string name);
        IEnumerable<Chatroom> GetAllLinksDapper();
        IReadOnlyList<string> GetAllChatnames();
        IReadOnlyList<string> GetChatLinks(string chatname);
    }

    public class DataRepository : IDataRepository
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<DataRepository> _logger;
        private readonly IConfiguration _config;

        public DataRepository(DataContext dataContext, IConfiguration config, ILogger<DataRepository> logger)
        {
            _dataContext = dataContext;
            _config = config;

            _logger = logger;
            _logger.LogDebug(1, "NLog injected into DataRepository");
        }
        
        public IEnumerable<Chatroom> GetAllLinksDapper()
        { 
            // TODO: RELOCATE
            using IDbConnection connection =
                new SqlConnection(_config["ConnectionStrings:DataContextDb"]);

            var links = connection.Query<Chatroom>("SELECT * FROM Chatrooms");

            return links;
        }
        public IReadOnlyList<string> GetAllChatnames()
        {
            // TODO: RELOCATE
            using IDbConnection connection =
                new SqlConnection(_config["ConnectionStrings:DataContextDb"]);

            var chatnames = connection.Query<string>(
                "SELECT DISTINCT Name FROM Chatrooms WHERE NOT Name=''")
                .ToList()
                .AsReadOnly();

            return chatnames;
        }

        public IReadOnlyList<string> GetChatLinks(string chatname)
        {
            // TODO: RELOCATE
            using IDbConnection connection =
                new SqlConnection(_config["ConnectionStrings:DataContextDb"]);

            var parameters = new { Chatname = chatname };
            var sql = "SELECT DISTINCT Link FROM Chatrooms WHERE Name LIKE CONCAT('%',@Chatname,'%');";
            var chatnames = connection.Query<string>(sql, parameters).ToList().AsReadOnly();

            return chatnames;
        }

        // TODO: strip out EF?
        public IEnumerable<Chatroom> GetAllLinks()
        {
            return _dataContext.Chatrooms
                       .OrderBy(p => p.Id)
                       .ToList();
        }
        // TODO: strip out EF?
        public IEnumerable<Chatroom> GetLinksByChatroom(string name)
        {
            
            return _dataContext.Chatrooms
                       .Where(p => p.Name == name)
                       .ToList();
        }
        // TODO: strip out EF?
        public int GetLinksCount()
        {
            return _dataContext.Chatrooms.Count();
        }

    }
}
