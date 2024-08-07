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
    public class CreateModel : PageModel
    {
        #region Properties
        private readonly IMediator _mediator;
        [BindProperty]
        public CreateCategoryViewModel Categorie { get; set; }
        #endregion Properties

        //[BindProperty]
        //public CreateCategory.CreateCategoryCommand Categorie { get; set; } // Dit zou op zich ook werken.

        #region ctor
        public CreateModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        #endregion ctor

        #region ViewModel
        public class CreateCategoryViewModel
        {
            public string Naam { get; set; } = string.Empty;
            public string? Beschrijving { get; set; }
            [DisplayName("Hoofd categorie")]
            public Guid? ParentCategorieId { get; set; }
        }
        #endregion ViewModel

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadCategoriesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return Page();
            }
            var result = await _mediator.Send(Categorie.Adapt<CreateCategory.CreateCategoryCommand>());
            if (result is SuccessResult<Guid> successResult)
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
