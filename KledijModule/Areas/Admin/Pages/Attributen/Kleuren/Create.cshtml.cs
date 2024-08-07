using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Features.ProductAttributen.Kleuren.Commands;
using ProjectCore.Shared.Exceptions;

namespace KledijModule.Areas.Admin.Pages.Attributen.Kleuren
{
    public class CreateModel : PageModel
    {
        #region Properties
        private readonly IMediator _mediator;

        [BindProperty]
        public CreateKleurViewModel Color { get; set; }
        #endregion Properties

        #region ctor
        public CreateModel(IMediator mediator)
        {
            _mediator = mediator;
        }
        #endregion ctor

        #region ViewModel
        public class CreateKleurViewModel
        {
            // string.empty gaat zo nooit null error smijten. Validatie zorgt dan dat de waarde niet leeg kan zijn.
            public string Kleur { get; set; } = string.Empty;
        }
        #endregion ViewModel

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var result = await _mediator.Send(Color.Adapt<AddColor.AddColorCommand>());
            if (result is SuccessResult<Guid> successResult)
            {
                return RedirectToPage(nameof(Index));
            }
            if (result is ValidationErrorResult validationErrorResult)
            {
                foreach (ValidationError error in validationErrorResult.Errors)
                {
                    string modelStateKey = $"{nameof(CreateKleurViewModel)}.{error.PropertyName}";
                    ModelState.AddModelError(modelStateKey, error.Details);
                }
            }
            else if (result is ErrorResult errorResult)
            {
                TempData["ErrorMessage"] = errorResult.Message;
            }
            // Als je hier komt heb je een probleem.
            return Page();
        }


    }
}
