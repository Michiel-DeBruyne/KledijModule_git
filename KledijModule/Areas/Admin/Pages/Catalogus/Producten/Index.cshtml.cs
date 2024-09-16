using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Features.Producten.Queries;
using ProjectCore.Shared.Exceptions;
using System.ComponentModel;

namespace KledijModule.Areas.Admin.Pages.Catalogus.Producten
{
    public class IndexModel : PageModel
    {
        #region Properties
        private readonly IMediator _mediator;
        public List<ProductViewModel> Producten { get; set; } = new List<ProductViewModel>();
        #endregion Properties

        #region ctor
        public IndexModel(IMediator mediator)
        {
            _mediator = mediator;
        }
        #endregion ctor

        #region ViewModel
        public class ProductViewModel
        {

            public Guid Id { get; set; }
            public string Naam { get; set; } = string.Empty;
            public Category Categorie { get; init; }

            [DisplayName("Calog?")]
            public bool BeschikbaarVoorCalog { get; init; }

            public record Category
            {
                public string Naam { get; set; }
            }
            public bool Beschikbaar { get; set; } = false;
            public int Punten { get; set; }
            public Geslacht Geslacht { get; set; }
            public int? ArtikelNummer { get; set; }
            [DisplayName("Foto")]
            public List<ProductImage> Fotos { get; init; }

            public record ProductImage
            {
                public string ImageUrl { get; init; }
            }
            public int MaxAantalBestelbaar { get; set; }

            public int PerAantalJaar { get; set; }

            [DisplayName("Vervangingstermijn")]
            public string DisplayVervangingsTermijn
            {
                get
                {
                    return $"{MaxAantalBestelbaar} per {PerAantalJaar} {(PerAantalJaar > 1 ? "jaren" : "jaar")}";
                }
            }
        }
        #endregion ViewModel

        public async Task OnGetAsync()
        {
            var result = await _mediator.Send(new GetProductList.GetProductsListQuery());

            if (result is SuccessResult<List<GetProductList.ProductsListVm>> successResult)
            {
                Producten = successResult.Data.Adapt<List<ProductViewModel>>();
            }
            else if (result is ErrorResult errorResult)
            {
                // Handle error result
                TempData["Errors"] = errorResult.Message;
            }
        }
    }
}
