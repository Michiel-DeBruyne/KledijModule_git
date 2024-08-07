using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Features.ProductAttributen.Kleuren.Commands;
using ProjectCore.Shared.Exceptions;

namespace KledijModule.Areas.Admin.Pages.Attributen.Kleuren
{
    public class DeleteModel : PageModel
    {

        #region Properties

        private readonly IMediator _mediator;

        #endregion Properties

        #region ctor
        public DeleteModel(IMediator mediator)
        {
            _mediator = mediator;
        }
        #endregion ctor

        // TODO: ?? , hier kan je eventueel dan een pagina weergeven die bevestiging vraagt. Voorlopig ga ik enkel de post implementeren
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost(DeleteColors.DeleteCommand deleteCommand)
        {
            var result = await _mediator.Send(deleteCommand);
            //if (result.Success)
            //{
            //    return RedirectToPage(nameof(Index));
            //}
            if (result is ErrorResult errorResult)
            {
                TempData["ErrorMessage"] = errorResult.Message;
            }
            //Raak je hier, was er een probleem.
            return RedirectToPage(nameof(Index));
        }
    }
}
