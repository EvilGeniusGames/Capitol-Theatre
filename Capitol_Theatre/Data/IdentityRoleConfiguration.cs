using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.AspNetCore.Identity;

namespace Capitol_Theatre.Data
{
    public class IdentityRoleConfiguration : IEntityTypeConfiguration<IdentityRole>
    {
        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            builder.ToTable("AspNetRoles");

            builder.Property(r => r.Id).HasColumnType("TEXT");
            builder.Property(r => r.Name).HasColumnType("TEXT").HasMaxLength(256);
            builder.Property(r => r.NormalizedName).HasColumnType("TEXT").HasMaxLength(256);
            builder.Property(r => r.ConcurrencyStamp).HasColumnType("TEXT");
        }
    }
}
