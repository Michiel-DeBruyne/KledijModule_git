﻿using ProjectCore.Domain.Common;
using ProjectCore.Domain.contract;

namespace ProjectCore.Domain.Entities.WinkelMand
{
    public class ShoppingCart : AuditableEntity, IEntity
    {
        public Guid Id { get; set; }

        public string GebruikerId { get; set; }

        public List<ShoppingCartItem> ShoppingCartItems { get; set; } = new List<ShoppingCartItem>();
    }
}

