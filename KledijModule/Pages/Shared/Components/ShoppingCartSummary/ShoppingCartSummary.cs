using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectCore.Data;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Features.WinkelMand.Queries;
using ProjectCore.Shared.Exceptions;

namespace KledijModule.Pages.Shared.Components.ShoppingCartSummary
{
    public class ShoppingCartSummary : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IMediator _mediator;

        public ShoppingCartSummary(ApplicationDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<IViewComponentResult> InvokeAsync(string userId)
        {
            var items = await _mediator.Send(new GetShoppingCartList.Query() { UserId = userId });
            var data = new ShoppingCartListViewModel(); // Initialize Data property
            if (items.Success)
            {
                if (items is SuccessResult<GetShoppingCartList.GetShoppingCartListVm> succesResult)
                {
                    data.ShoppingCartItems = succesResult.Data.ShoppingCartItems.Adapt<List<ShoppingCartListViewModel.ShoppingCartItem>>();
                }
            }

            return View(data);
        }
    }

    public class ShoppingCartListViewModel
    {
        public List<ShoppingCartItem> ShoppingCartItems { get; set; } = new List<ShoppingCartItem>();

        public record ShoppingCartItem
        {
            public Guid Id { get; set; }
            public Product Product { get; set; }
            public int Hoeveelheid { get; set; } = 0;
            public string? Kleur { get; set; }
            public string? Maat { get; set; }
        }
    }
}
