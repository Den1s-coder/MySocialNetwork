using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetwork.Domain.Entities.Posts;

namespace SocialNetwork.Infrastructure.Configurations
{
    public class PostReactionConfiguration : IEntityTypeConfiguration<PostReaction>
    {
        public void Configure(EntityTypeBuilder<PostReaction> builder)
        {
            builder.ToTable("PostReactions");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(r => r.PostId)
                   .IsRequired();

            builder.Property(r => r.UserId)
                   .IsRequired();

            builder.Property(r => r.ReactionTypeId)
                   .IsRequired();

            builder.HasOne(r => r.Post)
                   .WithMany(p => p.Reactions)
                   .HasForeignKey(r => r.PostId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.User)
                   .WithMany()
                   .HasForeignKey(r => r.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.ReactionType)
                   .WithMany()
                   .HasForeignKey(r => r.ReactionTypeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(r => new { r.PostId, r.UserId })
                   .IsUnique();
        }
    }
}