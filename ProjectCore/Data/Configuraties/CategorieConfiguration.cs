using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectCore.Domain.Entities.Catalogus;

namespace ProjectCore.Data.Configuraties
{
    public class CategorieConfiguration : IEntityTypeConfiguration<Categorie>
    {
        public void Configure(EntityTypeBuilder<Categorie> builder)
        {
            builder.HasMany(c => c.SubCategorieën)
                   .WithOne(c => c.ParentCategorie)
                   .HasForeignKey(c => c.ParentCategorieId);

            builder.HasMany(c => c.Products)
                   .WithOne(c => c.Categorie)
                   .HasForeignKey(c => c.CategorieId);
        }
    }
}
