using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetwork.Domain.Entities.Comments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Infrastructure.Configurations
{
    internal class CommentReactionConfiguration : IEntityTypeConfiguration<CommentReaction>
    {
        public void Configure(EntityTypeBuilder<CommentReaction> builder)
        {
            builder.ToTable("CommentReactions");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                    .ValueGeneratedOnAdd();

            builder.Property(r => r.CommentId)
                    .IsRequired();

            builder.Property(r => r.UserId)
                    .IsRequired();

            builder.Property(r => r.ReactionTypeId)
                    .IsRequired();

            builder.HasOne(r => r.Comment)
                    .WithMany(c => c.Reactions)
                    .HasForeignKey(r => r.CommentId)
                    .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.User)
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.ReactionType)
                    .WithMany()
                    .HasForeignKey(r => r.ReactionTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(r => new { r.CommentId, r.UserId })
                    .IsUnique();
        }
    }
}
