using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectCore.Features.Webshop.Queries;
using ProjectCore.Shared.Exceptions;

namespace KledijModule.Areas.Admin.Pages.Shared.Components.IsWebShopOpen
{
    public class IsWebShopOpenViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;
        public bool IsWebShopOpen { get; set; } = false;
        public IsWebShopOpenViewComponent(IMediator mediator) => _mediator = mediator;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            await IsWebshopOpen();
            return View(IsWebShopOpen);
        }

        private async Task IsWebshopOpen()
        {
            var isWebShopOpenresult = await _mediator.Send(new GetIsWebShopOpen.Query());
            if (isWebShopOpenresult is SuccessResult<bool> successResult)
            {
                IsWebShopOpen = successResult.Data;
            }
            if (isWebShopOpenresult is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
        }
    }
}
