using Htmx;
using KledijModule.Common.Authorization;
using KledijModule.Pages.Shared.Components.CategoryNavigation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Features.Favorieten;
using ProjectCore.Features.OrderItems.Commands;
using ProjectCore.Features.Producten.Queries;
using ProjectCore.Shared.Exceptions;
using System.ComponentModel;

namespace KledijModule.Pages.Catalogus
{
    public class IndexModel : PageModel
    {
        #region Properties
        private readonly IMediator _mediator;

        public List<ProductenListIndexViewModel> ProductenList { get; set; } = new List<ProductenListIndexViewModel>();


        [BindProperty(SupportsGet = true)]
        public string? Query { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? Categorie { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool OnlyFavorites { get; set; } = false;
        [BindProperty(SupportsGet = true)]
        public bool OnlyCalog { get; set; } = false;
        #endregion Properties

        #region ctor
        public IndexModel(IMediator mediator)
        {
            _mediator = mediator;
        }
        #endregion ctor

        #region ViewModel
        public class ProductenListIndexViewModel
        {
            public Guid Id { get; set; }
            public string Naam { get; set; } = string.Empty;
            public bool IsFavoriet { get; set; } 

            public Category Categorie { get; set; }
            public int Punten { get; set; }
            [DisplayName("Foto")]
            public List<ProductImage> Fotos { get; init; }

            public record ProductImage
            {
                public string ImageUrl { get; init; }
            }

            public record Category
            {
                public string Naam { get; set; } = string.Empty;
            }
        }
        #endregion ViewModel


        public async Task<IActionResult> OnGetAsync(string? Categorie, string? Query, bool? OnlyFavorites)
        {
            this.Categorie = Categorie;
            this.Query = Query;
            this.OnlyFavorites = OnlyFavorites.HasValue ? OnlyFavorites.Value : false; // omdat je anders conflict hebt met nullable to niet nullable gebruik je hier hasvalue en value
            this.OnlyCalog = User.IsInRole(Roles.Calog);
            var result = await _mediator.Send(new GetProductList.GetProductsListQuery() { Categorie = Categorie, SearchQuery = Query, OnlyPublished = true, IncludeSubCategorieProducts = true, EnkelFavorieten = OnlyFavorites,  EnkelCalog = OnlyCalog });;

            if (result is SuccessResult<List<GetProductList.ProductsListVm>> successResult) ProductenList = successResult.Data.Adapt<List<ProductenListIndexViewModel>>();
            if (result is ErrorResult errorResult) TempData["Errors"] = errorResult.Message;

            if (!Request.IsHtmx())
            {
                return Page();
            }
            return Partial("_ProductenLijst", this);

        }

        public async Task<IActionResult> OnGetCategoriesListAsync(Guid? Id = default)
        {
            return ViewComponent(typeof(CategoryNavigationViewComponent), Id);
        }

        public class CategorienavViewModel
        {
            public Guid Id { get; set; }
            public string Naam { get; set; }
        }

        public async Task<IActionResult> OnPostSetFavoriteAsync(Guid ProductId, string UserId)
        {
            
            // Update de bestelling met de nieuwe gegevens
            var updateResult = await _mediator.Send(new UpdateFavorite.Command()
            {
                ProductId = ProductId,
                UserId = UserId
            });

            if (updateResult.Success)
            {
                // Geef een successtatus terug
                return new JsonResult(new { success = true });
            }
            else if (updateResult is ErrorResult errorResult)
            {
                // Verzend een foutmelding terug
                return BadRequest(errorResult.Message);
            }

            // Standaard response bij een onbepaalde fout
            return StatusCode(500, "Internal server error.");
        }
    }
}
