using Microsoft.EntityFrameworkCore;

namespace luke_site_mvc.Data
{
    public class ChatroomContext : DbContext
    {
        public ChatroomContext(DbContextOptions<ChatroomContext> options) 
            : base(options)
        {
        }

        public DbSet<Chatroom> Chatrooms { get; set; }
    }
}
