using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Features.Images.Commands;
using ProjectCore.Shared.Exceptions;

namespace KledijModule.Areas.Admin.Pages.Catalogus.Producten
{
    public class DeleteImageFromProductModel : PageModel
    {
        private readonly IMediator _mediator;

        public DeleteImageFromProductModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(Guid FotoId)
        {
            if(FotoId == Guid.Empty)
            {
                return BadRequest("Er werd geen product meegegeven");
            }
            var deleteResult = await _mediator.Send(new DeleteImage.Command { Id = FotoId });
            if (deleteResult is SuccessResult<int> successResult)
            {
                if(successResult.Data > 0)
                {
                    return Content(""); // om database call te besparen return je hier lege content en dan wordt de rij verwijderd
                }
                return StatusCode(204); // Er werd niets verwijderd. Statuscode 204 zorgt ervoor dat htmx niets verwijderd

            }
            if(deleteResult is ErrorResult errorResult)
            {
                return BadRequest(errorResult.Message);
            }
            return BadRequest("Er liep iets onverwacht mis bij het verwijderen van de foto");
        }
    }
}
