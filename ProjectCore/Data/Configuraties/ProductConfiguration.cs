using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectCore.Domain.Entities.Catalogus;

namespace ProjectCore.Data.Configuraties
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasMany(p => p.Fotos)
               .WithOne(f => f.Product)
               .HasForeignKey(f => f.ProductId);

            builder.HasMany(p => p.Maten)
                   .WithMany(m => m.AssociatedProducts);

            builder.HasMany(p => p.Kleuren)
                   .WithMany(k => k.AssociatedProducts);

            builder.HasOne(p => p.Categorie)
                   .WithMany(c => c.Products)
                   .HasForeignKey(p => p.CategorieId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
