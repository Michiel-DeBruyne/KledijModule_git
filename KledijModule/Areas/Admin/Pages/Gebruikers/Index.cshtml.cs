using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Features.Gebruikers.Commands;
using ProjectCore.Features.Gebruikers.Queries;
using ProjectCore.Shared.Exceptions;

namespace KledijModule.Areas.Admin.Pages.Gebruikers
{
    public class IndexModel : PageModel
    {
        #region Properties
        private readonly IMediator _mediator;
        public List<GetusersListViewModel> Users { get; set; } = new List<GetusersListViewModel>();
        [BindProperty]
        public EditGebruikerViewModel GebruikerMetBalans { get; set; }
        #endregion Properties

        #region ctor
        public IndexModel(IMediator mediator)
        {
            _mediator = mediator;
        }
        #endregion ctor

        #region CommandModel
        public class EditGebruikerViewModel
        {
            public string Id { get; set; }
            public int Balans { get; set; }
        }
        #endregion CommandModel

        #region ViewModel
        public class GetusersListViewModel
        {
            public string Id { get; set; }
            public string VoorNaam { get; set; } = string.Empty;
            public string AchterNaam { get; set; } = string.Empty;

            public string FullName => VoorNaam + " " + AchterNaam;

            public string Email { get; set; }
            public int Balans { get; set; }
        }
        #endregion ViewModel
        public async Task OnGet()
        {
            var result = await _mediator.Send(new GetUsersList.Query());

            if (result is SuccessResult<List<GetUsersList.GetusersListVm>> successResult)
            {
                Users = successResult.Data.Adapt<List<GetusersListViewModel>>();
            }
            else if (result is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
        }

        public async Task<IActionResult> OnGetShowModal(GetUserBalans.Query query)
        {
            if (query == null)
            {
                TempData["Errors"] = "Gebruiker is leeg, geef een waarde mee";
                return RedirectToPage(nameof(Index));
            }
            var result = await _mediator.Send(query);
            if (result is SuccessResult<GetUserBalans.GetUserBalanceVm> gebruikersmetbalansResult)
            {
                GebruikerMetBalans = gebruikersmetbalansResult.Data.Adapt<EditGebruikerViewModel>();
            }
            else if (result is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
            return Partial("_EditGebruikerBalansModal", this); // geef de class IndexModel mee als model van de partial view. zo kan de data binnen deze view gebruikt worden.
        }

        //Krijgt model binnen via de property met data annotatie [BindProperty] Dit gebruik je dan in de form met asp-for
        public async Task<IActionResult> OnPostUpdateBalansAsync()
        {
            var result = await _mediator.Send(GebruikerMetBalans.Adapt<UpdateUserBalance.Command>());
            if (result is SuccessResult<UpdateUserBalance.GebruikerVm> successResult)
            {
                // Indien alles OK : return gebruiker partial met updated data
                var userdata = successResult.Data.Adapt<GetusersListViewModel>();
                return Partial("_Gebruiker", userdata);
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
    }
}
