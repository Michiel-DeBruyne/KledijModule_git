using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Features.Webshop.Queries;
using ProjectCore.Shared.Exceptions;

namespace KledijModule.Areas.Admin.Pages.Configuratie
{
    public class IndexModel : PageModel
    {
        private readonly IMediator _mediator;

        public List<WebShopConfigIndexViewModel> Data { get; set; } = new List<WebShopConfigIndexViewModel>();

        #region ViewModel
        public record WebShopConfigIndexViewModel
        {
            public Guid Id { get; set; }
            public DateTime OpeningDate { get; set; }
            public DateTime ClosingDate { get; set; }
        }
        #endregion ViewModel
        public IndexModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task OnGet()
        {
            var getWebShopConfigResult = await _mediator.Send(new GetWebShopConfigList.Query());
            if (getWebShopConfigResult is SuccessResult<List<GetWebShopConfigList.WebShopConfigVm>> SuccessResult)
            {
                Data = SuccessResult.Data.Adapt<List<WebShopConfigIndexViewModel>>();
            }
            if (getWebShopConfigResult is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
        }
    }
}
