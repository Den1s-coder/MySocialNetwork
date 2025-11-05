using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Infrastructure.Configurations
{
    public class FriendshipConfiguration : IEntityTypeConfiguration<Friendship>
    {
        public void Configure(EntityTypeBuilder<Friendship> builder)
        {
            builder.HasKey(f => f.Id);

            builder.Property(f => f.CreatedAt)
                   .IsRequired();

            builder.HasOne(f => f.Requester)
                   .WithMany(u => u.RequestedFriendship)
                   .HasForeignKey(f => f.RequesterId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(f => f.Addressee)
                   .WithMany(u => u.ReceivedFriendship)
                   .HasForeignKey(f => f.AddresseeId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(f => new { f.RequesterId, f.AddresseeId }).IsUnique();
        }
    }
}
