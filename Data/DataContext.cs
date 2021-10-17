using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace luke_site_mvc.Data
{
    public class DataContext : DbContext
    {
        private readonly IConfiguration _config;
        private readonly ILogger<DataContext> _logger;
        
        public DataContext(IConfiguration config, ILogger<DataContext> logger)
        {
            _config = config;

            _logger = logger;
            _logger.LogDebug(1, "NLog injected into DataContext");
        }

        public DbSet<Chatroom> Chatrooms { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseSqlServer(_config["ConnectionStrings:DataContextDb"]);
        }
    }
}
