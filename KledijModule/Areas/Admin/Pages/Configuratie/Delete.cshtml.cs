using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Features.Webshop.Commands;
using ProjectCore.Shared.Exceptions;

namespace KledijModule.Areas.Admin.Pages.Configuratie
{
    public class DeleteModel : PageModel
    {
        private readonly IMediator _mediator;

        public DeleteModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost(DeleteBestelPeriode.Command deleteCommand)
        {
            var result = await _mediator.Send(deleteCommand);

            if (result is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
            return RedirectToPage(nameof(Index));
        }
    }
}
