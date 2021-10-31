using System.Collections.Generic;
using luke_site_mvc.Data;
using Xunit;
using System.Data;
using Dapper;
using System.Data.SqlClient;
using System.Linq;

namespace luke_site_mvc
{
    public class DatabaseTests
    {
        // TODO: should I inject in service and repository here?
        //[Fact]
        //public void GetChatLinks_CheckIfDatabasePopulated()
        //{
        //    var result = GetChatLinks("bash").AsList().Count();
        //    Assert.NotEqual(0, result);
        //}
        ////public IReadOnlyList<string> GetChatLinks(string chatname)
        //public IEnumerable<Chatroom> GetChatLinks(string chatname)
        //{
        //    // TODO: RELOCATE
        //    using IDbConnection connection =
        //        new SqlConnection("Data Source=localhost;Initial Catalog=ChatDB;Integrated Security=True;MultipleActiveResultSets = true");

        //    var parameters = new { Chatname = chatname };
        //    var sql = "SELECT DISTINCT Link FROM Chatrooms WHERE Name LIKE CONCAT('%',@Chatname,'%');";
        //    //var chatnames = connection.Query<string>(sql, parameters).ToList().AsReadOnly();
        //    var chatnames = connection.Query<Chatroom>(sql, parameters);

        //    //var list = chatnames.Where(x => x.)

        //    return chatnames;
        //}
    }
}
