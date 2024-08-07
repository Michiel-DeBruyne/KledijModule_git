using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectCore.Domain.Entities.Catalogus;

namespace ProjectCore.Data.Configuraties
{
    internal class FotoConfiguration : IEntityTypeConfiguration<Foto>
    {
        public void Configure(EntityTypeBuilder<Foto> builder)
        {
            builder.HasOne(f => f.Product)
                   .WithMany(p => p.Fotos)
                   .HasForeignKey(f => f.ProductId);
        }
    }
}
