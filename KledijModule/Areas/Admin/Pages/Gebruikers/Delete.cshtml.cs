using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Features.Gebruikers.Commands;
using ProjectCore.Shared.Exceptions;

namespace KledijModule.Areas.Admin.Pages.Gebruikers
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

        //TODO: ?? DeleteSizes behouden of nederlands maken en de entiteit veranderen van string Maat naar string Waarde fz??
        public async Task<IActionResult> OnPost(DeleteUser.Command DeleteCommand)
        {
            var result = await _mediator.Send(DeleteCommand);

            //if(result.Success)
            //{
            //    return RedirectToPage(nameof(Index));
            //}

            if (result is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
            return RedirectToPage(nameof(Index));
        }
    }
}
