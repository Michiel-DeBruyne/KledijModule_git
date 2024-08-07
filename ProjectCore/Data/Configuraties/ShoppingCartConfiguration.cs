using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectCore.Domain.Entities.WinkelMand;

namespace ProjectCore.Data.Configuraties
{
    public class ShoppingCartConfiguration : IEntityTypeConfiguration<ShoppingCart>
    {
        public void Configure(EntityTypeBuilder<ShoppingCart> builder)
        {
            builder.HasMany(sc => sc.ShoppingCartItems)
                   .WithOne(sci => sci.ShoppingCart)
                   .HasForeignKey(sci => sci.ShoppingCartId);
        }
    }
}
