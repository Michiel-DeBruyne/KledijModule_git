using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Features.Webshop.Commands;
using ProjectCore.Shared.Exceptions;

namespace KledijModule.Areas.Admin.Pages.Configuratie
{
    public class AddModel : PageModel
    {
        #region Properties
        [BindProperty]
        public WebShopConfigAddViewModel BestelPeriode { get; set; }

        public IMediator _mediator;
        #endregion Properties

        #region ViewModel
        public record WebShopConfigAddViewModel
        {
            public DateTime OpeningDate { get; set; }
            public DateTime ClosingDate { get; set; }
        }


        #endregion ViewModel

        public AddModel(IMediator mediator)
        {
            _mediator = mediator;
        }
        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostAsync()
        {
            var addBestelPeriodeResult = await _mediator.Send(BestelPeriode.Adapt<AddBestelPeriode.Command>());
            if (addBestelPeriodeResult.Success)
            {
                return RedirectToPage(nameof(Index));
            }
            if (addBestelPeriodeResult is ValidationErrorResult validationErrorResult)
            {
                foreach (ValidationError error in validationErrorResult.Errors)
                {
                    string modelStateKey = $"{nameof(Product)}.{error.PropertyName}";
                    ModelState.AddModelError(modelStateKey, error.Details);
                }
            }
            if (addBestelPeriodeResult is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
            // iets is misgegaan
            return Page();
        }
    }
}
