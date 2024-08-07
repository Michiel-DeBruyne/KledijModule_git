using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Features.ProductAttributen.Maten.Queries;
using ProjectCore.Features.Producten.Commands;
using ProjectCore.Shared.Exceptions;

namespace KledijModule.Areas.Admin.Pages.Catalogus.Producten
{
    public class MapSizeToProductModel : PageModel
    {
        #region properties
        private readonly IMediator _mediator;
        // Property met private set om wijzigingen van buiten de klasse te beperken
        public SizeMappingViewModel MapSizeToProductViewModel { get; private set; }
        [TempData]
        public bool RefreshPage { get; set; }
        [BindProperty]
        public CommandData Data { get; set; }

        #endregion properties

        #region ctor
        public MapSizeToProductModel(IMediator mediator)
        {
            _mediator = mediator;
        }
        #endregion ctor

        #region ViewModel
        // Definitie van het SizeMappingViewModel record
        public record SizeMappingViewModel
        {
            // Auto-geïmplementeerde properties met alleen getters (standaard private set)
            public Guid ProductId { get; }
            public string ProductNaam { get; }
            public List<size> Maten { get; set; }

            // Constructor om het record te initialiseren
            public SizeMappingViewModel(Guid productId, string productNaam)
            {
                ProductId = productId;
                ProductNaam = productNaam;
            }
            public record size
            {
                public Guid Id { get; set; }
                public string Maat { get; set; }
            }
        }
        #endregion ViewModel

        public async Task OnGetAsync(Guid ProductId, string ProductNaam)
        {
            // Initialiseren van het viewmodel met behulp van object-initialisatiesyntax
            MapSizeToProductViewModel = new SizeMappingViewModel(ProductId, ProductNaam);
            var beschikbareMaten = await _mediator.Send(new GetMatenList.GetMatenListQuery() { ProductId = ProductId });
            if (beschikbareMaten is SuccessResult<List<GetMatenList.GetMatenListVm>> beschikbareMatenResult)
            {
                MapSizeToProductViewModel.Maten = beschikbareMatenResult.Data.Adapt<List<SizeMappingViewModel.size>>();
            }
            if (beschikbareMaten is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
        }


        public async Task<IActionResult> OnPostAsync()
        {
            var result = await _mediator.Send(Data.Adapt<MapSizeToProduct.Command>());
            if (result.Success)
            {
                RefreshPage = true;
                //TODO: Hier zal productNaam ook moeten meegegeven worden...
                return RedirectToPage("./MapSizeToProduct", new { ProductId = Data.Product, ProductNaam = Data.ProductNaam });
            }

            if (result is ValidationErrorResult validationErrorResult)
            {
                foreach (ValidationError error in validationErrorResult.Errors)
                {
                    string modelStateKey = $"{nameof(Product)}.{error.PropertyName}"; // TODO: dit is misschien wat overkill, bespreken met Caitlin.
                    ModelState.AddModelError(modelStateKey, error.Details);
                    //ModelState.AddModelError(error.PropertyName, error.Details);
                }
            }
            if (result is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
            return Page();
        }


        #region CommandModel
        public class CommandData
        {
            public Guid Product { get; set; }
            public string ProductNaam { get; set; }
            public List<Guid> Maten { get; set; } = new List<Guid>();
        }
        #endregion CommandModel
    }
}
