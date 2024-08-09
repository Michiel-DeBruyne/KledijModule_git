using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Features.Gebruikers.Commands;
using ProjectCore.Features.Gebruikers.Queries;
using ProjectCore.Features.Producten.Commands;
using ProjectCore.Features.Producten.Queries;
using ProjectCore.Shared.Exceptions;
using static KledijModule.Areas.Admin.Pages.Catalogus.Producten.EditModel;
using static ProjectCore.Features.Producten.Queries.GetProductMetDetails;

namespace KledijModule.Areas.Admin.Pages.Gebruikers
{
    public class EditModel : PageModel
    {
        private readonly IMediator _mediator;
        [BindProperty]
        public EditGebruikerViewModel GebruikerMetBalans { get; set; }

        public EditModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task OnGet(GetUserBalans.Query query)
        {
            var result = await _mediator.Send(query);
            if (result is SuccessResult<GetUserBalans.GetUserBalanceVm> gebruikersmetbalansResult)
            {
                GebruikerMetBalans = gebruikersmetbalansResult.Data.Adapt<EditGebruikerViewModel>();
            }
            else if (result is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var result = await _mediator.Send(GebruikerMetBalans.Adapt<UpdateUserBalance.Command>());
            if (result is SuccessResult successResult)
            {
                // Indien alles OK : Redirect to index page
                return RedirectToPage(nameof(Index));
            }

            if (result is ValidationErrorResult validationErrorResult)
            {
                foreach (ValidationError error in validationErrorResult.Errors)
                {
                    string modelStateKey = $"{nameof(Product)}.{error.PropertyName}"; // TODO: dit is misschien wat overkill, bespreken met Caitlin.
                    ModelState.AddModelError(modelStateKey, error.Details);
                }
            }
            if (result is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
            //Iets misgelopen, return page met huidige data
            return Page();
        }

        public class EditGebruikerViewModel
        {
            public string Id { get; set; }
            public string FullName { get; set; }
            public int Balans { get; set;}
        }
    }
}
