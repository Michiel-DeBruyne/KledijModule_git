using ProjectCore.Domain.Common;
using ProjectCore.Domain.contract;
using ProjectCore.Domain.Entities.Gebruiker;
using System.ComponentModel;

namespace ProjectCore.Domain.Entities.Bestellingen
{
    public class Order : AuditableEntity, IEntity
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        //public ApplicationUser User { get; set; }

        public string UserNaam { get; set; }

        [DisplayName("Totaal")]
        public int TotaalPrijs { get; set; } // onnodige complexiteit
        public List<OrderItem> OrderItems { get; set; }

    }
}
