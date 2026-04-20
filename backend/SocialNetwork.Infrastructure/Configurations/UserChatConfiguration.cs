using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetwork.Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Infrastructure.Configurations
{
    public class UserChatConfiguration: IEntityTypeConfiguration<UserChat>
    {
        public void Configure(EntityTypeBuilder<UserChat> builder)
        {
            builder.HasKey(uc => new 
            { 
                uc.UserId, 
                uc.ChatId 
            });

            builder.Property(uc => uc.JoinedAt)
                   .IsRequired();

            builder.Property(uc => uc.Role)
                   .IsRequired();

            builder.Property(uc => uc.Rights)
                   .IsRequired();

            builder.HasOne(uc => uc.User)
                   .WithMany(u => u.UserChats)
                   .HasForeignKey(uc => uc.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(uc => uc.Chat)
                   .WithMany(c => c.UserChats)
                   .HasForeignKey(uc => uc.ChatId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
