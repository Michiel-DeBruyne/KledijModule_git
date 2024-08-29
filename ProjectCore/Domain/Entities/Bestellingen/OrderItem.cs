using ProjectCore.Domain.Common;
using ProjectCore.Domain.contract;
using ProjectCore.Domain.Entities.Catalogus;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectCore.Domain.Entities.Bestellingen
{
    public class OrderItem : AuditableEntity, IEntity
    {
        public Guid Id { get; set; }
        public string ProductNaam { get; set; }
        public string? Maat { get; set; }
        public string? Kleur { get; set; }
        public Guid? ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
        public int Punten { get; set; }

        public Guid OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        public int Hoeveelheid { get; set; }
        public OrderStatus OrderStatus { get; set; }

        public string? Opmerkingen { get; set; }

        // Add Opmerkingen field
    }

    public enum OrderStatus
    {
        Open = 1,
        Besteld = 2,
        [Display(Name = "Op te halen bij logistiek")]
        OpTeHalen = 3,
        Opgehaald = 4,
        Geannuleerd = 5,
        Retour = 6
    }
}
