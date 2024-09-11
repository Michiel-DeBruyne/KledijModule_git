using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Features.ProductAttributen.Kleuren.Queries;
using ProjectCore.Features.Producten.Commands;
using ProjectCore.Shared.Exceptions;

namespace KledijModule.Areas.Admin.Pages.Catalogus.Producten
{
    public class MapColorToProductModel : PageModel
    {
        #region properties
        private readonly IMediator _mediator;
        // Property met private set om wijzigingen van buiten de klasse te beperken
        public ColorMappingViewModel MapColorToProductViewModel { get; private set; }
        [TempData]
        public bool RefreshPage { get; set; }

        [BindProperty]
        public CommandData Data { get; set; }

        #endregion properties

        #region ctor
        public MapColorToProductModel(IMediator mediator)
        {
            _mediator = mediator;
        }
        #endregion ctor

        #region ViewModel
        // Definitie van het SizeMappingViewModel record
        public record ColorMappingViewModel
        {
            // Auto-geïmplementeerde properties met alleen getters (standaard private set)
            public Guid ProductId { get; }
            public string ProductNaam { get; }
            public List<color> Kleuren { get; set; }

            // Constructor om het record te initialiseren
            public ColorMappingViewModel(Guid productId, string productNaam)
            {
                ProductId = productId;
                ProductNaam = productNaam;
            }
            public record color
            {
                public Guid Id { get; set; }
                public string Kleur { get; set; }
            }
        }
        #endregion ViewModel

        public async Task<IActionResult> OnGetAsync(Guid ProductId, string ProductNaam)
        {
            if(ProductId == Guid.Empty)
            {
                return BadRequest("Geen product meegegeven om een kleur aan te koppelen");
            }
            // Initialiseren van het viewmodel met behulp van object-initialisatiesyntax
            MapColorToProductViewModel = new ColorMappingViewModel(ProductId, ProductNaam);
            var beschikbareKleuren = await _mediator.Send(new GetKleurenList.GetKleurenListQuery() { ProductId = ProductId });
            if (beschikbareKleuren is SuccessResult<List<GetKleurenList.GetKleurenListVm>> beschikbarekleurenResult)
            {
                MapColorToProductViewModel.Kleuren = beschikbarekleurenResult.Data.Adapt<List<ColorMappingViewModel.color>>();
            }
            if (beschikbareKleuren is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
            return Partial("_MapColorToProductModal", this);

        }


        public async Task<IActionResult> OnPostAsync()
        {
            var result = await _mediator.Send(Data.Adapt<MapColorToProduct.Command>());
            if (result.Success)
            {
                return ViewComponent("KleurenForProductTable", new { ProductId = Data.Product });
            }
            //ALs je hier geraakt was iets niet juist
            MapColorToProductViewModel = new ColorMappingViewModel(Data.Product, Data.ProductNaam);
            var beschikbareKleuren = await _mediator.Send(new GetKleurenList.GetKleurenListQuery() { ProductId = Data.Product });
            MapColorToProductViewModel.Kleuren = beschikbareKleuren.Adapt<List<ColorMappingViewModel.color>>();
            if (result is ValidationErrorResult validationErrorResult)
            {
                foreach (ValidationError error in validationErrorResult.Errors)
                {
                    string modelStateKey = $"{nameof(Product)}.{error.PropertyName}"; // TODO: dit is misschien wat overkill, bespreken met Caitlin.
                    ModelState.AddModelError(modelStateKey, error.Details);
                    return BadRequest(validationErrorResult.Errors.Select(err => err.Details.ToString()));
                }
            }
            if (result is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
            return BadRequest("Onverwachte fout tijdens het koppelen van de kleuren aan het product, contacteer ICT");
        }




        #region CommandModel
        public class CommandData
        {
            public Guid Product { get; set; }
            public string ProductNaam { get; set; }
            public List<Guid> Kleuren { get; set; } = new List<Guid>();
        }
        #endregion CommandModel
    }
}
