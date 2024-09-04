using ProjectCore.Domain.Entities.Catalogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCore.Data
{
    public static class DatabaseInitializer
    {
        public static void Seed(ApplicationDbContext context)
        {
            if(!context.ProductKleuren.Any(k => k.Kleur == "Standaard"))
            {
                context.ProductKleuren.Add(new ProductKleur { Id = Guid.NewGuid(), Kleur = "Standaard" });
            }

            if (!context.ProductMaten.Any(m => m.Maat == "Standaard"))
            {
                context.ProductMaten.Add(new ProductMaat { Id = Guid.NewGuid(), Maat = "Standaard" });
            }

            context.SaveChanges();
        }
    }
}
