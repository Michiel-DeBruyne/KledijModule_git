using ProjectCore.Domain.Common;
using ProjectCore.Domain.contract;
using ProjectCore.Domain.Entities.Gebruiker;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCore.Domain.Entities.Catalogus
{
    public class Favoriet : AuditableEntity, IEntity
    {
        public Guid Id { get; set; }

        public string GebruikerId { get;set; }

        [ForeignKey("GebruikerId")]
        public ApplicationUser? Gebruiker { get; set; }

        public Guid ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
    }
}
