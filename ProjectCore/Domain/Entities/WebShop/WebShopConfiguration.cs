using ProjectCore.Domain.Common;
using ProjectCore.Domain.contract;

namespace ProjectCore.Domain.Entities.WebShop
{
    public class WebShopConfiguration : AuditableEntity, IEntity
    {
        public Guid Id { get; set; }
        public DateTime OpeningDate { get; set; }
        public DateTime ClosingDate { get; set; }
        public bool IsWebshopOpen()
        {
            DateTime currentDate = DateTime.Now;
            return currentDate >= OpeningDate && currentDate <= ClosingDate;
        }
    }
}
