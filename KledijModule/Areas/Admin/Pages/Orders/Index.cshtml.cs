using Htmx;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Graph.Models.Security;
using ProjectCore.Features.Orders.Queries;
using ProjectCore.Shared.Exceptions;
using System.ComponentModel;

namespace KledijModule.Areas.Admin.Pages.Orders
{
    public class IndexModel : PageModel
    {
        private readonly IMediator _mediator;
        private string _currentTab;
        public List<OrdersIndexListViewModel> Orders { get; set; } = new List<OrdersIndexListViewModel>();
        public IEnumerable<string> Items { get; } = new[] { "Openstaande bestellingen", "Afgesloten bestellingen" };

        [BindProperty(Name = "tab", SupportsGet = true)]
        public string CurrentTab
        {
            get => !string.IsNullOrEmpty(_currentTab) ? _currentTab : "Openstaande bestellingen";
            set => _currentTab = value;
        }

        public IndexModel(IMediator mediator)
        {
            _mediator = mediator;
        }


        public async Task<IActionResult> OnGetAsync()
        {
            var ordersResult = await _mediator.Send(new GetOrdersSummaryList.Query() { Status = CurrentTab});
            if (ordersResult is SuccessResult<List<GetOrdersSummaryList.OrdersSummaryListVm>> SuccessResult)
            {
                Orders = SuccessResult.Data.Adapt<List<OrdersIndexListViewModel>>();
            }
            if (ordersResult is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }

            return Request.IsHtmx() ? Partial("_IndexPartial", this) : Page();
        }

        public class OrdersIndexListViewModel
        {
            public Guid Id { get; set; }

            public string UserId { get; set; }
            [DisplayName("Gebruiker")]
            public string UserNaam { get; set; }
            [DisplayName("Aangemaakt")]
            public DateTime CreatedDate { get; set; }
            [DisplayName("Totaal")]
            public int TotaalPrijs { get; set; }

            [DisplayName("Status")]
            public string OrderStatus { get; set; }

        }


        public string IsSelectedCss(string tab, string cssClass)
        {
            var temp = tab == CurrentTab ? cssClass : string.Empty;
            return temp;
        }

        public string IsSelectedAria(string tab)
        {
            var temp = tab == CurrentTab ? "page" : "false";
            return temp;
        }

    }
}
