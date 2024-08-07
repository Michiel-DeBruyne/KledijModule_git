using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Shared.Exceptions;
using static ProjectCore.Features.ProductAttributen.Maten.Queries.GetMatenList;

namespace KledijModule.Areas.Admin.Pages.Attributen.Maten
{
    public class IndexModel : PageModel
    {
        #region Properties
        private readonly IMediator _mediator;
        public List<GetMatenListViewModel> Maten { get; set; } = new List<GetMatenListViewModel>();

        #endregion Properties

        #region ctor
        public IndexModel(IMediator mediator)
        {
            _mediator = mediator;
        }
        #endregion ctor

        #region ViewModel
        public class GetMatenListViewModel
        {
            public Guid Id { get; set; }
            public string Maat { get; set; }
        }
        #endregion ViewModel

        public async Task OnGetAsync()
        {
            var result = await _mediator.Send(new GetMatenListQuery());

            if (result is SuccessResult<List<GetMatenListVm>> successResult)
            {
                Maten = successResult.Data.Adapt<List<GetMatenListViewModel>>();
            }
            else if (result is ErrorResult errorResult)
            {
                // Handle error result
                TempData["ErrorMessage"] = errorResult.Message;
            }
        }
    }
}
