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

            modelBuilder.Entity<ApiUser>().HasMany(u => u.Posts).WithOne(p => p.User);
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Post> Posts => Set<Post>();
    }
}
