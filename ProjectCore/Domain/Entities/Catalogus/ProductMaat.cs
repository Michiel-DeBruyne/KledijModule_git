using ProjectCore.Domain.Common;
using ProjectCore.Domain.contract;

namespace ProjectCore.Domain.Entities.Catalogus
{
    public class ProductMaat : AuditableEntity, IEntity
    {
        public Guid Id { get; set; }
        public string Maat { get; set; }
        public List<Product> AssociatedProducts { get; set; } = new List<Product>();

    }
}
