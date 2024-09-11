using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Features.ProductAttributen.Maten.Commands;
using ProjectCore.Shared.Exceptions;

namespace KledijModule.Areas.Admin.Pages.Attributen.Maten
{
    public class CreateModel : PageModel
    {
        private readonly IMediator _mediator;

        [BindProperty]
        public CreateMaatViewModel Size { get; set; }

        public CreateModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Size.Maat.Contains('.'))
            {
                Size.Maat = Size.Maat.Replace('.', ',');
            }
            var result = await _mediator.Send(Size.Adapt<AddSize.AddSizeCommand>());

            if (result.Success)
            {
                return RedirectToPage(nameof(Index));
            }
            if (result is ValidationErrorResult validationErrorResult)
            {
                foreach (ValidationError error in validationErrorResult.Errors)
                {
                    string modelStateKey = $"{nameof(CreateMaatViewModel)}.{error.PropertyName}";
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

        public class CreateMaatViewModel
        {
            // string.empty gaat zo nooit null error smijten. Validatie zorgt dan dat de waarde niet leeg kan zijn.
            public string Maat { get; set; } = string.Empty;
        }
    }
}
