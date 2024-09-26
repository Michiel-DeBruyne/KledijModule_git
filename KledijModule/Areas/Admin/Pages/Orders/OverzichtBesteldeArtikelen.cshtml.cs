using Htmx;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Graph.Education.Classes.Item.Assignments.Item.Submissions.Item.Return;
using ProjectCore.Domain.Entities.Bestellingen;
using ProjectCore.Features.OrderItems.Queries;
using ProjectCore.Shared.Exceptions;

namespace KledijModule.Areas.Admin.Pages.Orders
{
    public class OverzichtBesteldeArtikelenModel : PageModel
    {
        #region properties
        private readonly IMediator _mediator;

        //Om het mogelijk te maken afgesloten bestelartikelen weer te geven.standaard false;
        [BindProperty(SupportsGet = true)]
        public Boolean ToonAfgesloten { get; set; } = false;

        [BindProperty(SupportsGet = true)]
        public string? ProductSearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 100;

        public bool HasMoreItems { get; private set; }

        [BindProperty(SupportsGet = true)]
        public int Pagina { get; set; } = 1; //Page is een reserved word. Dus dit is de vervanger


        public List<OrderitemsListViewModel> OrderItemsList { get; set; } = new List<OrderitemsListViewModel>();

        #endregion properties

        #region ctor
        public OverzichtBesteldeArtikelenModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        #endregion ctor

        public async Task<IActionResult> OnGet()
        {
            var result = await _mediator.Send(new GetOrderItemsList.Query
            {
                ToonAfgesloten = this.ToonAfgesloten,
                ProductSearchString = this.ProductSearchString,
                Page = this.Pagina,
                PageSize = this.PageSize
            });
            if (result is SuccessResult<List<GetOrderItemsList.OrderitemsListVm>> succesResult)
            {
                OrderItemsList = succesResult.Data.Adapt<List<OrderitemsListViewModel>>();
                HasMoreItems = OrderItemsList.Count == PageSize; // om infinite loop te voorkomen hier check om te kijken of er mogelijks meer items zijn.
            }
            if (result is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
                if (Request.IsHtmx()) { return BadRequest(errorResult.Message); }
            }

            return Request.IsHtmx() ? Partial("_OverzichtBesteldeArtikel_ArtikelRow", this) : Page();
        }

            #region ViewModel
        public class OrderitemsListViewModel
        {
            public string UserNaam { get; set; }
            public Guid Id { get; set; }
            public string ProductNaam { get; set; }
            public string? Maat { get; set; }
            public string? Kleur { get; set; }
            public int Punten { get; set; }
            public int Hoeveelheid { get; set; }
            public OrderStatus OrderStatus { get; set; }
            public string? Opmerkingen { get; set; }
            public DateTime CreatedDate { get; set; }
        }

        #endregion ViewModel
    }
}
