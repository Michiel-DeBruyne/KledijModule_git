using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Features.ProductAttributen.Kleuren.Commands;
using ProjectCore.Shared.Exceptions;
using static ProjectCore.Features.ProductAttributen.Kleuren.Queries.GetKleur;

namespace KledijModule.Areas.Admin.Pages.Attributen.Kleuren
{
    public class EditModel : PageModel
    {
        #region Properties
        private readonly IMediator _mediator;

        [BindProperty]
        public EditKleurViewModel Color { get; set; }

        #endregion Properties

        #region ctor
        public EditModel(IMediator mediator)
        {
            _mediator = mediator;
        }
        #endregion ctor

        #region ViewModel
        public class EditKleurViewModel
        {
            public Guid Id { get; set; }
            // string.empty gaat zo nooit null error smijten. Validatie zorgt dan dat de waarde niet leeg kan zijn.
            public string Kleur { get; set; } = string.Empty;
        }
        #endregion ViewModel

        public async Task OnGetAsync(Query query)
        {
            var result = await _mediator.Send(query);

            if (result is SuccessResult<GetKleurVm> successResult)
            {
                Color = successResult.Data.Adapt<EditKleurViewModel>();
            }
            else if (result is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var result = await _mediator.Send(Color.Adapt<EditColor.EditColorCommand>());

            if (result.Success)
            {
                return RedirectToPage(nameof(Index));
            }
            else if (result is ErrorResult errorResult)
            {
                TempData["ErrorMessage"] = errorResult.Message;
            }

            return Page();
        }
    }
}
