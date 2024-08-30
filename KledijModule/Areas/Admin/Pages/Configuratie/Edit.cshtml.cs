using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Features.Webshop.Commands;
using ProjectCore.Features.Webshop.Queries;
using ProjectCore.Shared.Exceptions;

namespace KledijModule.Areas.Admin.Pages.Configuratie
{
    public class EditModel : PageModel
    {
        #region Properties
        [BindProperty]
        public WebShopConfigEditViewModel BestelPeriode { get; set; }

        public IMediator _mediator;
        #endregion Properties

        #region ViewModel
        public record WebShopConfigEditViewModel
        {
            public Guid Id { get; set; }
            public DateTime OpeningDate { get; set; }
            public DateTime ClosingDate { get; set; }
        }


        #endregion ViewModel

        public EditModel(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task OnGet(GetWebShopConfig.Query query)
        {
            var result = await _mediator.Send(query);
            if (result is SuccessResult<GetWebShopConfig.WebShopConfigVm> successResult)
            {
                BestelPeriode = successResult.Data.Adapt<WebShopConfigEditViewModel>();
            }
            else if (result is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var updateBestelPeriodeResult = await _mediator.Send(BestelPeriode.Adapt<EditBestelPeriode.Command>());
            if (updateBestelPeriodeResult.Success)
            {
                return RedirectToPage(nameof(Index));
            }
            if (updateBestelPeriodeResult is ValidationErrorResult validationErrorResult)
            {
                foreach (ValidationError error in validationErrorResult.Errors)
                {
                    string modelStateKey = $"{nameof(Product)}.{error.PropertyName}";
                    ModelState.AddModelError(modelStateKey, error.Details);
                }
            }
            if (updateBestelPeriodeResult is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
            // iets is misgegaan
            return Page();
        }
    }
}
