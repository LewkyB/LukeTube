using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;

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
            // TODO: RELOCATE TO BASE REPOSITORY
            using IDbConnection connection = new ProfiledDbConnection(
                new SqlConnection(_config.GetConnectionString("DefaultConnection")),
                MiniProfiler.Current
                );

            return connection.Query<Chatroom>("SELECT * FROM Chatrooms");
        }
        public IEnumerable<string> GetAllChatNames()
        {
            // TODO: RELOCATE TO BASE REPOSITORY
            using IDbConnection connection = new ProfiledDbConnection(
                new SqlConnection(_config.GetConnectionString("DefaultConnection")),
                MiniProfiler.Current
                );

            return connection.Query<string>("SELECT DISTINCT Name FROM Chatrooms WHERE NOT Name=''");
        }

        public IEnumerable<string> GetChatLinksByChat(string chatName)
        {
            // TODO: RELOCATE TO BASE REPOSITORY
            using IDbConnection connection = new ProfiledDbConnection(
                new SqlConnection(_config.GetConnectionString("DefaultConnection")),
                MiniProfiler.Current
                );

            var parameters = new { chatName = chatName };

            // order by descending so that most recent videos appear at the top of the page
            string sql = "SELECT Link FROM Chatrooms WHERE Name = @chatName ORDER BY Chatrooms.Id DESC;";

            return connection.Query<string>(sql, parameters);
        }
    }
}
