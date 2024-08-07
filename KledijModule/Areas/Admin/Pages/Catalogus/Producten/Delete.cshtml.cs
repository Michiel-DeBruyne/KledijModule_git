using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Features.Producten.Commands;
using ProjectCore.Shared.Exceptions;

namespace KledijModule.Areas.Admin.Pages.Catalogus.Producten
{
    public class DeleteModel : PageModel
    {

        #region Properties

        private readonly IMediator _mediator;

        #endregion Properties

        public DeleteModel(IMediator mediator)
        {
            _mediator = mediator;
        }
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost(DeleteProducts.DeleteProductsCommand Command)
        {
            var result = await _mediator.Send(Command);

            if (result is ErrorResult errorResult)
            {
                // Behandel foutresultaat
                TempData["ErrorMessage"] = errorResult.Message;
            }

            // Als het resultaat geen succes of fout is, keer terug naar de huidige pagina
            return RedirectToPage(nameof(Index));
        }
    }
}
