using ProjectCore.Domain.Common;
using ProjectCore.Domain.contract;
using ProjectCore.Domain.Entities.Gebruiker;

namespace ProjectCore.Domain.Entities.WinkelMand
{
    public class ShoppingCart : AuditableEntity, IEntity
    {
        public Guid Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser Gebruiker { get; set; }

        public List<ShoppingCartItem> ShoppingCartItems { get; set; } = new List<ShoppingCartItem>();
    }
}
