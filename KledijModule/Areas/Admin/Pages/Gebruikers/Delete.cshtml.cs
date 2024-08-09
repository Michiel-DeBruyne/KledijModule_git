using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Features.Gebruikers.Commands;
using ProjectCore.Features.ProductAttributen.Maten.Commands;
using ProjectCore.Shared.Exceptions;

namespace KledijModule.Areas.Admin.Pages.Gebruikers
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

        public async Task<IActionResult> OnPost(DeleteUser.Command deleteCommand)
        {
            var deleteResult = await _mediator.Send(deleteCommand);
            //else if(deleteResult is ValidationErrorResult validationError)
            //{
            //   foreach (ValidationError error in validationError.Errors)
            //    {
            //        string modelStateKey = $"{nameof(Product)}.{error.PropertyName}"; // TODO: dit is misschien wat overkill, bespreken met Caitlin.
            //        ModelState.AddModelError(modelStateKey, error.Details);
            //    }
            //}
            if (deleteResult is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
            return RedirectToPage(nameof(Index));
        }
    }
}
