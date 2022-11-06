using LukeTube.Models.Pushshift;
using Microsoft.EntityFrameworkCore;

namespace LukeTube.Data
{
    public sealed class PushshiftContext : DbContext
    {
        public PushshiftContext(DbContextOptions<PushshiftContext> options)
            : base(options)
        {
        }

        public DbSet<RedditComment> RedditComments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RedditComment>()
                .HasIndex(c => new { c.Subreddit, c.YoutubeLinkId })
                .IsUnique();
        }
    }
}
