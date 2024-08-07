using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Features.Producten.Commands;
using ProjectCore.Shared.Exceptions;

namespace KledijModule.Areas.Admin.Pages.Catalogus.Producten
{
    public class DeleteSizeFromProductModel : PageModel
    {
        #region Properties

        private readonly IMediator _mediator;

        #endregion Properties

        public DeleteSizeFromProductModel(IMediator mediator)
        {
            _mediator = mediator;
        }
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(DeleteSizeFromproduct.DeleteSizeFromproductCommand Command)
        {

            var result = await _mediator.Send(Command);

            if (result is SuccessResult successResult)
            {
                return ViewComponent("MatenForProductTable", Command.ProductId);
            }

            if (result is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;

            }
            //Als je hier komt is er fout gebeurt
            return RedirectToPage("./Edit", new { Id = Command.ProductId });
        }
    }
}
