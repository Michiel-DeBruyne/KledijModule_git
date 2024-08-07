using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Features.Producten.Commands;
using ProjectCore.Shared.Exceptions;

namespace KledijModule.Areas.Admin.Pages.Catalogus.Producten
{
    public class DeleteColorFromProductModel : PageModel
    {
        #region Properties

        private readonly IMediator _mediator;

        #endregion Properties

        public DeleteColorFromProductModel(IMediator mediator)
        {
            _mediator = mediator;
        }
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost(DeleteColorFromProduct.DeleteColorFromProductCommand command)
        {
            var result = await _mediator.Send(command);

            if (result is SuccessResult successResult)
            {
                return ViewComponent("KleurenForProductTable", command.ProductId);
            }

            if (result is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;

            }
            //Als je hier komt is er fout gebeurt
            return RedirectToPage("./Edit", new { Id = command.ProductId });
        }
    }
}
