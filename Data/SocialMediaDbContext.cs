using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialMediaAPI.Models;

namespace SocialMediaAPI.Data
{
    public class SocialMediaDbContext : IdentityDbContext<ApiUser>
    {
        public SocialMediaDbContext(DbContextOptions<SocialMediaDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Post>()
                .HasMany(p => p.LikedByUsers)
                .WithMany(u => u.LikedPosts)
                .UsingEntity(j => j.ToTable("Post_Likes"));

            modelBuilder
                .Entity<ApiUser>()
                .HasMany(u => u.Posts)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .IsRequired();

            modelBuilder
                .Entity<ApiUser>()
                .HasMany(u => u.ReceivedMessages)
                .WithOne(m => m.Receiver)
                .HasForeignKey(m => m.ReceiverId)
                .IsRequired();

            modelBuilder
                .Entity<ApiUser>()
                .HasMany(u => u.SentMessages)
                .WithOne(m => m.Sender)
                .HasForeignKey(m => m.SenderId)
                .IsRequired();

            modelBuilder
                .Entity<ApiUser>()
                .HasMany(u => u.Comments)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .IsRequired();

            modelBuilder
                .Entity<ApiUser>()
                .HasMany(u => u.Followers)
                .WithMany(u => u.Following)
                .UsingEntity<Dictionary<string, ApiUser>>(
                    "UserFollowers",
                    j =>
                        j.HasOne<ApiUser>()
                            .WithMany()
                            .HasForeignKey("FollowerId")
                            .OnDelete(DeleteBehavior.Restrict),
                    j =>
                        j.HasOne<ApiUser>()
                            .WithMany()
                            .HasForeignKey("FollowingId")
                            .OnDelete(DeleteBehavior.Restrict),
                    j =>
                    {
                        j.HasKey("FollowerId", "FollowingId");
                        j.ToTable("UserFollowers");
                    }
                );
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<ChatMessage> Messages => Set<ChatMessage>();
    }
}
