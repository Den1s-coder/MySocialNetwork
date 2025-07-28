using Microsoft.EntityFrameworkCore;
using SocialNetwork.Domain.Entities;


namespace SocialNetwork.Infrastructure
{
    public class SocialDbContext:DbContext
    {
        public SocialDbContext(DbContextOptions<SocialDbContext> options):base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
    }
}
