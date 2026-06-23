using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetwork.Domain.Entities.Chats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Infrastructure.Configurations
{
    public class MessageReactionConfiguration : IEntityTypeConfiguration<MessageReaction>
    {
        public void Configure(EntityTypeBuilder<MessageReaction> builder)
        {
            builder.ToTable("MessageReactions");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                    .ValueGeneratedOnAdd();

            builder.Property(r => r.MessageId)
                    .IsRequired();

            builder.Property(r => r.UserId)
                    .IsRequired();

            builder.Property(r => r.ReactionTypeId)
                    .IsRequired();

            builder.HasOne(r => r.Message)
                    .WithMany(m => m.Reactions)
                    .HasForeignKey(r => r.MessageId)
                    .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.User)
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.ReactionType)
                    .WithMany()
                    .HasForeignKey(r => r.ReactionTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(r => new { r.MessageId, r.UserId })
                    .IsUnique();
        }
    }
}
