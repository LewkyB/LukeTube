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
        IEnumerable<string> GetAllChatNames();
        IEnumerable<string> GetChatLinksByChat(string chatName);
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
        
        public IEnumerable<Chatroom> GetAllLinks()
        { 
            // TODO: RELOCATE
            using IDbConnection connection =
                new SqlConnection(_config["ConnectionStrings:DataContextDb"]);

            IEnumerable<Chatroom> links = connection.Query<Chatroom>("SELECT * FROM Chatrooms");

            return links;
        }
        public IEnumerable<string> GetAllChatNames()
        {
            // TODO: RELOCATE
            using IDbConnection connection =
                new SqlConnection(_config["ConnectionStrings:DataContextDb"]);

            IEnumerable<string> chatnames = connection.Query<string>(
                "SELECT DISTINCT Name FROM Chatrooms WHERE NOT Name=''"
                );

            return chatnames;
        }

        public IEnumerable<string> GetChatLinksByChat(string chatName)
        {
            // TODO: RELOCATE
            using IDbConnection connection =
                new SqlConnection(_config["ConnectionStrings:DataContextDb"]);

            var parameters = new { chatName = chatName };
            string sql = "SELECT DISTINCT Link FROM Chatrooms WHERE Name LIKE CONCAT('%',@chatName,'%');";
            
            IEnumerable<string> chatnames = connection.Query<string>(sql, parameters);

            return chatnames;
        }
    }
}
