using ProjectCore.Domain.Common;
using ProjectCore.Domain.contract;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectCore.Domain.Entities.Catalogus
{
    public class Foto : AuditableEntity, IEntity
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
        public string ImageUrl { get; set; }
        public int Volgorde { get; set; }
    }
}
