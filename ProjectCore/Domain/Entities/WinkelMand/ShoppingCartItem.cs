using ProjectCore.Domain.Common;
using ProjectCore.Domain.contract;
using ProjectCore.Domain.Entities.Catalogus;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectCore.Domain.Entities.WinkelMand
{
    public class ShoppingCartItem : AuditableEntity, IEntity
    {
        public Guid Id { get; set; }

        public Guid ShoppingCartId { get; set; }

        public ShoppingCart ShoppingCart { get; set; }
        public Guid ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
        public int Hoeveelheid { get; set; }
        public string? Kleur { get; set; }
        public string? Maat { get; set; }

        public string? Opmerkingen { get; set; }
    }
}
