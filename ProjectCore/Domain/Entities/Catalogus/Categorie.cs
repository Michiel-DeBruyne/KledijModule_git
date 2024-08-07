using ProjectCore.Domain.Common;
using ProjectCore.Domain.contract;
using System.ComponentModel.DataAnnotations;

namespace ProjectCore.Domain.Entities.Catalogus
{
    public class Categorie : AuditableEntity, IEntity
    {
        public Guid Id { get; set; }
        [Required]
        public string Naam { get; set; }
        public string? Beschrijving { get; set; }

        public Guid? ParentCategorieId { get; set; }

        public Categorie? ParentCategorie { get; set; }


        //Navigation property
        public ICollection<Product> Products { get; set; } = new List<Product>();

        public ICollection<Categorie> SubCategorieën { get; set; } = new List<Categorie>();
    }
}
