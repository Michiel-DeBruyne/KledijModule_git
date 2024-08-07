using Htmx;
using KledijModule.Pages.Shared.Components.CategoryNavigation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

            public Category Categorie { get; set; }
            public int Prijs { get; set; }
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

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await _mediator.Send(new GetProductList.GetProductsListQuery() { Categorie = Categorie, SearchQuery = Query, OnlyPublished = true, IncludeSubCategorieProducts = true });

            if (result is SuccessResult<List<GetProductList.ProductsListVm>> successResult) ProductenList = successResult.Data.Adapt<List<ProductenListIndexViewModel>>();
            if (result is ErrorResult errorResult) TempData["Errors"] = errorResult.Message;

            if (!Request.IsHtmx())
            {
                return Page();
            }
            Response.Htmx(h =>
            {
                h.Push(Request.GetEncodedUrl());
            });
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
    }
}
