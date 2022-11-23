using LukeTubeLib.Models.Pushshift;
using Microsoft.EntityFrameworkCore;

namespace LukeTubeLib.Repositories
{
    public sealed class PushshiftContext : DbContext
    {
        public PushshiftContext(DbContextOptions<PushshiftContext> options)
            : base(options)
        {
            // TODO: turn off in production?
            Database.EnsureCreated();
            // AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        public DbSet<RedditComment> RedditComments { get; set; }
        public DbSet<VideoModel> Videos { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Thumbnail> Thumbnails { get; set; }
        public DbSet<EngagementModel> EngagementModels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // modelBuilder.Entity<RedditComment>()
            //     .HasAlternateKey(x => x.YoutubeLinkId);
            // .HasIndex(c => new { c.Subreddit, c.YoutubeLinkId })
            // .IsUnique();

            // modelBuilder.Entity<Author>()
            //     .HasOne(x => x.VideoModel)
            //     .WithOne(x => x.Author);
            //
            // modelBuilder.Entity<EngagementModel>()
            //     .HasOne(x => x.VideoModel)
            //     .WithOne(x => x.EngagementModel);

            // modelBuilder.Entity<Thumbnail>().Property(x => x.ThumbnailId).UseIdentityColumn()
            // modelBuilder.Entity<Thumbnail>()
            //     .HasOne(x => x.VideoModel)
            //     .WithMany(x => x.Thumbnails)
            //     .HasForeignKey(x => x.ThumbnailId);
        }
    }
}