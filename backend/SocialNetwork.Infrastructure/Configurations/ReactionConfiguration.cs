using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetwork.Domain.Entities.Reactions;

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

            builder.HasData(
                new ReactionType { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Code = "like",  Symbol = "👍", SortOrder = 10 },
                new ReactionType { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), Code = "love",  Symbol = "❤️", SortOrder = 20 },
                new ReactionType { Id = Guid.Parse("00000000-0000-0000-0000-000000000003"), Code = "laugh", Symbol = "😂", SortOrder = 30 },
                new ReactionType { Id = Guid.Parse("00000000-0000-0000-0000-000000000004"), Code = "sad",   Symbol = "😢", SortOrder = 40 },
                new ReactionType { Id = Guid.Parse("00000000-0000-0000-0000-000000000005"), Code = "angry", Symbol = "😡", SortOrder = 50 }
            );
        }
    }
}
