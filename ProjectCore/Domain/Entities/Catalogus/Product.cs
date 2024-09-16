using ProjectCore.Domain.Common;
using ProjectCore.Domain.contract;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectCore.Domain.Entities.Catalogus
{
    public class Product : AuditableEntity, IEntity
    {
        public Guid Id { get; set; }
        public string Naam { get; set; } = string.Empty;
        public string? Beschrijving { get; set; }
        public int? ArtikelNummer { get; set; }
        public bool Beschikbaar { get; set; } = false;
        // punten zijn altijd gehele getallen. Int is groot genoeg
        public int Punten { get; set; }
        public Geslacht Geslacht { get; set; }

        //Dit misschien naar IEntity verhuizen zodat het bij elke entiteit bijgehouden wordt.
        public DateTime Aangemaakt { get; set; }
        public DateTime Gewijzigd { get; set; }

        public bool BeschikbaarVoorCalog { get; set; } = false;

        #region Categorie
        [Required(ErrorMessage = "Categorie is required.")]
        public Guid CategorieId { get; set; }

        [ForeignKey("CategorieId")]
        public Categorie? Categorie { get; set; } = default!;
        #endregion Categorie

        #region VervangingsTermijn

        public int MaxAantalBestelbaar { get; set; }

        public int PerAantalJaar { get; set; }

        public string DisplayVervangingsTermijn
        {
            get
            {
                return $"{MaxAantalBestelbaar} per {PerAantalJaar} {(PerAantalJaar > 1 ? "jaren" : "jaar")}";
            }
        }

        #endregion VervangingsTermijn


        public List<Foto> Fotos { get; set; } = new List<Foto>();
        public List<ProductMaat> Maten { get; set; } = new List<ProductMaat>();
        public List<ProductKleur> Kleuren { get; set; } = new List<ProductKleur>();
    }

    public enum Geslacht
    {
        Man = 0,
        Vrouw = 1,
        Unisex = 2
    }
}
