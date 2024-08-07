using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Features.Producten.Queries;
using ProjectCore.Features.WinkelMand.Commands;
using ProjectCore.Shared.Exceptions;
using System.Security.Claims;

namespace KledijModule.Pages.Catalogus
{
    public class ProductDetailsModel : PageModel
    {
        private readonly IMediator _mediator;

        public ProductDetailsViewModel Product { get; set; }

        [BindProperty]
        public ShoppingCartItemViewModel ShoppingCartItem { get; set; } = new ShoppingCartItemViewModel();

        public ProductDetailsModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task OnGetAsync(GetProductMetDetails.Query query)
        {
            var result = await _mediator.Send(query);
            if (result is SuccessResult<GetProductMetDetails.ProductMetDetailsVm> successResult)
            {
                Product = successResult.Data.Adapt<ProductDetailsViewModel>();
            }
            if (result is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                await _mediator.Send(new AddToShoppingCart.Command
                {
                    UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    Item = ShoppingCartItem.Adapt<AddToShoppingCart.Command.ShoppingCartItem>()
                });
                return RedirectToPage(nameof(Index));
            }
            catch (Exception ex)
            {
                var productMetDetails = await _mediator.Send(new GetProductMetDetails.Query() { Id = ShoppingCartItem.ProductId });
                Product = productMetDetails.Adapt<ProductDetailsViewModel>();
                ModelState.AddModelError("", ex.Message);
                return Page();
            }
        }

        public class ProductDetailsViewModel
        {
            public Guid Id { get; set; }
            public string Naam { get; set; } = string.Empty;
            public string? Beschrijving { get; set; }
            public int Prijs { get; set; }
            public Geslacht Geslacht { get; set; }
            public List<Foto> Fotos { get; set; } = new List<Foto>();
            public List<ProductMaat> Maten { get; set; } = new List<ProductMaat>();
            public List<ProductKleur> Kleuren { get; set; } = new List<ProductKleur>();

            public record Foto
            {
                public string ImageUrl { get; set; }
            }
            public record ProductMaat
            {
                public string Maat { get; set; }
            }
            public record ProductKleur
            {
                public string Kleur { get; set; }
            }
        }

        public class ShoppingCartItemViewModel
        {
            public Guid ProductId { get; set; }
            public int Hoeveelheid { get; set; }
            public string? Kleur { get; set; }
            public string? Maat { get; set; }
            public string? Opmerkingen { get; set; }
        }
    }
}
