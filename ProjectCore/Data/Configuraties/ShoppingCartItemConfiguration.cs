using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectCore.Domain.Entities.WinkelMand;

namespace ProjectCore.Data.Configuraties
{
    public class ShoppingCartItemConfiguration : IEntityTypeConfiguration<ShoppingCartItem>
    {
        public void Configure(EntityTypeBuilder<ShoppingCartItem> builder)
        {
            builder.HasOne(sci => sci.Product)
                   .WithMany()
                   .HasForeignKey(sci => sci.ProductId);
        }
    }
}
