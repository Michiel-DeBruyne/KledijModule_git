using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Features.Gebruikers.Queries;
using ProjectCore.Features.WinkelMand.Commands;
using ProjectCore.Features.WinkelMand.Queries;
using ProjectCore.Shared.Exceptions;
using System.Security.Claims;

namespace KledijModule.Pages.Winkelmand
{
    public class IndexModel : PageModel
    {
        #region Properties
        private readonly IMediator _mediator;
        public ShoppingCartIndexViewModel ShoppingCart { get; set; }

        public UserBalanceViewModel GebruikerBalans { get; set; }

        #endregion Properties

        #region ctor
        public IndexModel(IMediator mediator)
        {
            _mediator = mediator;
        }
        #endregion ctor

        #region ViewModel
        public class ShoppingCartIndexViewModel
        {
            public Guid Id { get; set; }
            public string UserId { get; set; }
            public List<ShoppingCartItem> ShoppingCartItems { get; set; } = new List<ShoppingCartItem>();
            public record ShoppingCartItem
            {
                public Guid Id { get; set; }
                public Product Product { get; set; }
                public int Hoeveelheid { get; set; }
                public string? Kleur { get; set; }
                public string? Maat { get; set; }
            }
            public int Totaal { get; set; } = 0;
        }
        #endregion ViewModel

        public async Task OnGet()
        {
            await getShoppingCart();
            await getUserBalance();
        }

        public async Task<IActionResult> OnPostUpdateShoppingCartItemAsync(Guid ShoppingCartItemId, int Hoeveelheid)
        {
            var updateShoppingCartItemResult = await _mediator.Send(new UpdateShoppingCartItem.Command() { Id = ShoppingCartItemId, Hoeveelheid = Hoeveelheid });

            if (updateShoppingCartItemResult.Success)
            {
                await getShoppingCart();
                return Partial("_ShoppingCartItems", this);
            }
            else if (updateShoppingCartItemResult is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
            // als iets misging, ga terug naar get pagehandler, tempdata errors zal weergegeven worden op pagina (Normaal toch :P)
            return RedirectToAction("Get");
        }

        public async Task getShoppingCart()
        {
            var result = await _mediator.Send(new GetShoppingCartList.Query() { UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value });
            if (result is SuccessResult<GetShoppingCartList.GetShoppingCartListVm> successResult)
            {
                ShoppingCart = successResult.Data.Adapt<ShoppingCartIndexViewModel>();
                ShoppingCart.Totaal = ShoppingCart.ShoppingCartItems.Sum(p => p.Hoeveelheid * p.Product.Prijs);
            }
            else if (result is ErrorResult getshoppingCartErrorResult)
            {
                TempData["Errors"] = getshoppingCartErrorResult.Message;
            }
        }

        public async Task<ActionResult> OnPostRemoveFromShoppingCartAsync(Guid ShoppingCart, Guid ShoppingCartItem)
        {
            var removeFromShoppingCartResult = await _mediator.Send(new RemoveFromShoppingCart.Command() { ShoppingCartId = ShoppingCart, ShoppingCartItemId = ShoppingCartItem });
            if (removeFromShoppingCartResult.Success)
            {
                await getShoppingCart();
                return Partial("_ShoppingCartItems", this);
            }
            if (removeFromShoppingCartResult is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
            // als iets misging, ga terug naar get pagehandler, tempdata errors zal weergegeven worden op pagina (Normaal toch :P)
            return RedirectToAction("Get");
        }

        private async Task getUserBalance()
        {
            var result = await _mediator.Send(new GetUserBalans.Query() { Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value });
            if (result is SuccessResult<GetUserBalans.GetUserBalanceVm> successResult)
            {
                GebruikerBalans = successResult.Data.Adapt<UserBalanceViewModel>();
            }
            else if (result is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
        }

        public class UserBalanceViewModel
        {
            public int Balans { get; set; } = 0;
        }

    }
}
