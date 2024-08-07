using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectCore.Features.Categories.Commands;
using ProjectCore.Features.Categories.Queries;
using ProjectCore.Shared.Exceptions;
using System.ComponentModel;

namespace KledijModule.Areas.Admin.Pages.Catalogus.Categories
{
    public class EditModel : PageModel
    {
        #region Properties
        private readonly IMediator _mediator;

        [BindProperty]
        public EditCategoryViewModel Categorie { get; set; }

        #endregion Properties

        #region ctor
        public EditModel(IMediator mediator) => _mediator = mediator;
        #endregion ctor

        #region ViewModel
        public class EditCategoryViewModel
        {
            public Guid Id { get; set; }
            public string Naam { get; set; } = string.Empty;
            public string? Beschrijving { get; set; }
            [DisplayName("Hoofd categorie")]
            public Guid? ParentCategorieId { get; set; }
        }
        #endregion ViewModel

        public async Task OnGetAsync(GetCategorieMetDetails.Query query)
        {
            var result = await _mediator.Send(query);
            if (result is SuccessResult<GetCategorieMetDetails.GetCategorieMetDetailsVm> successResult)
            {
                Categorie = successResult.Data.Adapt<EditCategoryViewModel>();
                await LoadCategoriesAsync();
            }
            if (result is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
                RedirectToPage(nameof(Index));// Ga terug naar de index pagina want er is een error gebeurt, en toon de error dan op de index pagina
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return Page();
            }

            var result = await _mediator.Send(Categorie.Adapt<EditCategory.EditCategoryCommand>());
            if (result is SuccessResult successResult)
            {
                // Indien alles OK : Redirect to index page
                return RedirectToPage(nameof(Index));
            }
            if (result is ValidationErrorResult validationErrorResult)
            {
                foreach (ValidationError error in validationErrorResult.Errors)
                {
                    string modelStateKey = $"{nameof(Categorie)}.{error.PropertyName}";
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
            var result = await _mediator.Send(new GetCategoriesList.Query());
            if (result is SuccessResult<List<GetCategoriesList.CategoriesListVm>> successResult)
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
