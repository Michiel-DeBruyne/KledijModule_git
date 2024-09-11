using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Features.ProductAttributen.Maten.Commands;
using ProjectCore.Features.ProductAttributen.Maten.Queries;
using ProjectCore.Shared.Exceptions;
using static ProjectCore.Features.ProductAttributen.Maten.Queries.GetMaat;

namespace KledijModule.Areas.Admin.Pages.Attributen.Maten
{
    public class EditModel : PageModel
    {
        #region Properties
        private readonly IMediator _mediator;

        [BindProperty]
        public EditMaatViewModel Size { get; set; }
        #endregion Properties

        #region ctor
        public EditModel(IMediator mediator)
        {
            _mediator = mediator;
        }
        #endregion ctor

        #region ViewModel
        public class EditMaatViewModel
        {
            public Guid Id { get; set; }
            // string.empty gaat zo nooit null error smijten. Validatie zorgt dan dat de waarde niet leeg kan zijn.
            public string Maat { get; set; } = string.Empty;
        }
        #endregion ViewModel

        public async Task OnGetAsync(GetMaat.Query query)
        {
            if (Size.Maat.Contains("."))
            {
                Size.Maat.Replace(".", ",");
            }
            var result = await _mediator.Send(query);

            if (result is SuccessResult<GetMaatVm> successResult)
            {
                Size = successResult.Data.Adapt<EditMaatViewModel>();
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
            var result = await _mediator.Send(Size.Adapt<EditSize.Command>());

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
