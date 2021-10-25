using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
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
        private readonly ILogger<DataRepository> _logger;
        private readonly IConfiguration _config;

        public DataRepository(IConfiguration config, ILogger<DataRepository> logger)
        {
            _config = config;

            _logger = logger;
        }
        
        public IEnumerable<Chatroom> GetAllLinks()
        { 
            // TODO: RELOCATE
            using IDbConnection connection =
                new SqlConnection(_config.GetConnectionString("DefaultConnection"));

            return connection.Query<Chatroom>("SELECT * FROM Chatrooms");
        }
        public IEnumerable<string> GetAllChatNames()
        {
            // TODO: RELOCATE
            using IDbConnection connection =
                new SqlConnection(_config.GetConnectionString("DefaultConnection"));

            return connection.Query<string>("SELECT DISTINCT Name FROM Chatrooms WHERE NOT Name=''");
        }

        public IEnumerable<string> GetChatLinksByChat(string chatName)
        {
            // TODO: RELOCATE
            using IDbConnection connection =
                new SqlConnection(_config.GetConnectionString("DefaultConnection"));

            var parameters = new { chatName = chatName };
            // TODO: order by newest at top of page
            string sql = "SELECT Link FROM Chatrooms WHERE Name = @chatName;";

            return connection.Query<string>(sql, parameters);
        }
    }
}
