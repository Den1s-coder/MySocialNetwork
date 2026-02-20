using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetwork.Domain.Entities.Reactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Infrastructure.Configurations
{
    public class ReactionConfiguration : IEntityTypeConfiguration<ReactionType>
    {
        public void Configure(EntityTypeBuilder<ReactionType> builder)
        {
            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.Id)
                    .ValueGeneratedOnAdd();

            builder.Property(rt => rt.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(rt => rt.Symbol)
                .HasMaxLength(10);

            builder.Property(rt => rt.ImageUrl);

            builder.Property(rt => rt.SortOrder);
        }
    }
}
