using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Features.Orders.Commands;
using ProjectCore.Shared.Exceptions;

namespace KledijModule.Areas.Admin.Pages.Orders
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

        public async Task<IActionResult> OnPost(List<Guid> Ids)
        {
            var deleteBestellingResult = await _mediator.Send(new DeleteOrders.Command() { Orders = Ids });

            if (deleteBestellingResult.Failure)
            {
                if (deleteBestellingResult is ErrorResult errorResult)
                {
                    TempData["Errors"] = errorResult.Message;
                }
            }
            return RedirectToPage(nameof(Index));
        }
    }
}
