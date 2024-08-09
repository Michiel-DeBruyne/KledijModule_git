using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Features.Gebruikers.Commands;
using ProjectCore.Features.Gebruikers.Queries;
using ProjectCore.Shared.Exceptions;

namespace KledijModule.Areas.Admin.Pages.Gebruikers
{
    public class IndexModel : PageModel
    {
        private readonly IMediator _mediator;
        public List<GetusersListViewModel> Users { get; set; } = new List<GetusersListViewModel>();

        public IndexModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task OnGet()
        {
            var result = await _mediator.Send(new GetUsersList.Query());

            if (result is SuccessResult<List<GetUsersList.GetusersListVm>> successResult)
            {
                Users = successResult.Data.Adapt<List<GetusersListViewModel>>();
            }
            else if (result is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
        }

        public class GetusersListViewModel
        {
            public string Id { get; set; }
            public string VoorNaam { get; set; } = string.Empty;
            public string AchterNaam { get; set; } = string.Empty;

            public string FullName => VoorNaam + " " + AchterNaam;

            public string Email { get; set; }
            public int Balans { get; set; }
        }
    }
}
