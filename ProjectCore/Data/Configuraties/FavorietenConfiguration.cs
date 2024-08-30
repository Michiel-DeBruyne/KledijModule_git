using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Domain.Entities.Catalogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCore.Data.Configuraties
{
    public class FavorietenConfiguration : IEntityTypeConfiguration<Favoriet>
    {
        public void Configure(EntityTypeBuilder<Favoriet> builder)
        {
            builder.ToTable("Favorieten");

            // Primary key
            builder.HasKey(f => f.Id);

            // Foreign Key Configuraties
            builder.HasOne(f => f.Gebruiker)
                   .WithMany(u => u.ProductFavorieten) // Navigatie-eigenschap in ApplicationUser
                   .HasForeignKey(f => f.GebruikerId)
                   .OnDelete(DeleteBehavior.Cascade); // Verwijder alle favorieten bij verwijderen van de gebruiker

            builder.HasOne(f => f.Product)
                   .WithMany() // Geen navigatie-eigenschap in Product
                   .HasForeignKey(f => f.ProductId)
                   .OnDelete(DeleteBehavior.Cascade); // Verwijder alle favorieten bij verwijderen van het product

            // Extra configuratie zoals indexen, etc.
            builder.HasIndex(f => new { f.GebruikerId, f.ProductId }).IsUnique();
        }
    }
}

