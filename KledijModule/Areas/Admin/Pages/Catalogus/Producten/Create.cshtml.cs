using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Features.Producten.Commands;
using ProjectCore.Shared.Exceptions;
using System.ComponentModel;
using static ProjectCore.Features.Categories.Queries.GetCategoriesList;

namespace KledijModule.Areas.Admin.Pages.Catalogus.Producten
{
    public class CreateModel : PageModel
    {
        #region Properties
        private readonly IMediator _mediator;
        [BindProperty]
        public CreateProductViewModel Product { get; set; }
        #endregion Properties

        #region ViewModel
        public class CreateProductViewModel
        {
            public string Naam { get; set; } = string.Empty;
            public string? Beschrijving { get; set; }
            public bool Beschikbaar { get; set; } = false;
            public int Punten { get; set; }
            public Geslacht Geslacht { get; set; }
            public int? ArtikelNummer { get; set; }

            [DisplayName("Categorieën")]
            public Guid CategorieId { get; set; }
            [DisplayName("Maximum aantal bestelbaar")]
            public int MaxAantalBestelbaar { get; set; }

            [DisplayName("Per aantal jaar")]
            public int PerAantalJaar { get; set; }
        }
        #endregion ViewModel

        #region ctor
        public CreateModel(IMediator mediator)
        {
            _mediator = mediator;
        }
        #endregion ctor

        public async Task OnGetAsync()
        {
            await LoadCategoriesAsync();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            // Adapt the viewmodel naar command.
            var result = await _mediator.Send(Product.Adapt<CreateProduct.Command>());
            if (result is SuccessResult<Guid> successResult)
            {
                // Indien alles OK : Redirect to index page
                return RedirectToPage("./Edit", new { Id = successResult.Data });
            }

            if (result is ValidationErrorResult validationErrorResult)
            {
                foreach (ValidationError error in validationErrorResult.Errors)
                {
                    string modelStateKey = $"{nameof(Product)}.{error.PropertyName}";
                    ModelState.AddModelError(modelStateKey, error.Details);
                }
            }
            if (result is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }

            await LoadCategoriesAsync();
            return Page();
        }


        private async Task LoadCategoriesAsync()
        {
            var result = await _mediator.Send(new Query());
            if (result is SuccessResult<List<CategoriesListVm>> successResult)
            {
                ViewData["Categories"] = new SelectList(successResult.Data, "Id", "Naam");
            }
            if (result is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
        }

    }
}