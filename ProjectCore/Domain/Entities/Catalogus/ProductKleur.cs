using ProjectCore.Domain.Common;
using ProjectCore.Domain.contract;

namespace ProjectCore.Domain.Entities.Catalogus
{
    public class ProductKleur : AuditableEntity, IEntity
    {
        // TODO: Kleur mss veranderen door Naam of waarde fz?
        public Guid Id { get; set; }
        public string Kleur { get; set; } = string.Empty;

        public List<Product> AssociatedProducts { get; set; } = new List<Product>();
    }
}
