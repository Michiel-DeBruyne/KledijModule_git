using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Shared.Exceptions;
using static ProjectCore.Features.ProductAttributen.Kleuren.Queries.GetKleurenList;

namespace KledijModule.Areas.Admin.Pages.Attributen.Kleuren
{
    public class IndexModel : PageModel
    {
        #region Properties

        private readonly IMediator _mediator;
        public List<GetKleurenListViewModel> Kleuren { get; set; } = new List<GetKleurenListViewModel>();

        #endregion Properties

        #region ctor
        public IndexModel(IMediator mediator)
        {
            _mediator = mediator;
        }
        #endregion ctor

        #region ViewModel
        public class GetKleurenListViewModel
        {
            public Guid Id { get; set; }
            public string Kleur { get; set; }
        }
        #endregion ViewModel

        public async Task OnGetAsync()
        {
            var result = await _mediator.Send(new GetKleurenListQuery());

            if (result is SuccessResult<List<GetKleurenListVm>> successResult)
            {
                Kleuren = successResult.Data.Adapt<List<GetKleurenListViewModel>>();
            }
            else if (result is ErrorResult errorResult)
            {
                // Handle error result
                TempData["ErrorMessage"] = errorResult.Message;
            }
        }
    }
}
