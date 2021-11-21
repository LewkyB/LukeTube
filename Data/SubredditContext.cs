using luke_site_mvc.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace luke_site_mvc.Data
{
    public class SubredditContext : DbContext
    {
        public SubredditContext(DbContextOptions<SubredditContext> options)
            : base(options)
        {
        }

        public DbSet<RedditComment> RedditComments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<RedditComment>()
            //    .Property<string>("Subreddit");

            //modelBuilder.Entity<RedditComment>()
            //    .Property<string>("YoutubeLinkId");

            //modelBuilder.Entity<RedditComment>()
            //    .HasKey("Subreddit", "YoutubeLinkId");
            modelBuilder.Entity<RedditComment>()
                .HasKey(c => new { c.Subreddit, c.YoutubeLinkId });
            modelBuilder.Entity<RedditComment>()
                .HasIndex(c => new { c.Subreddit, c.YoutubeLinkId }).IsUnique();
        }

        // TODO: need a way to dynamically change connection string without the need to inject
        // used to enable newing up DbContext, but
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer("Data Source=localhost;Initial Catalog=ChatDB;Trusted_Connection=True;MultipleActiveResultSets=true");
        //}
    }
}
