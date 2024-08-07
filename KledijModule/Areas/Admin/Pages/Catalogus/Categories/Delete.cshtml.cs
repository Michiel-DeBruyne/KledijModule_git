using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Features.Categories.Commands;
using ProjectCore.Shared.Exceptions;

namespace KledijModule.Areas.Admin.Pages.Catalogus.Categories
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

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost(DeleteCategories.DeleteCommand Command)
        {
            var result = await _mediator.Send(Command);

            if (result is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
            return RedirectToPage(nameof(Index));
        }
    }
}
