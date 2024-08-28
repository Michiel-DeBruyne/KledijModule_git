using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Graph.Models;
using ProjectCore.Domain.Entities.Bestellingen;
using ProjectCore.Domain.Entities.Gebruiker;
using ProjectCore.Features.OrderItems.Commands;
using ProjectCore.Features.Orders.Queries;
using System.Security.Claims;

namespace KledijModule.Areas.Identity.Pages.Account.Orders
{
    public class IndexModel : PageModel
    {
        #region Properties
        private readonly IMediator _mediator;

        public List<OrdersSummaryListDetailedViewModel> Orders { get; set; }

        [BindProperty]
        public CommandModel Data { get; set; }

        #endregion Properties

        #region ctor
        public IndexModel(IMediator mediator)
        {
            _mediator = mediator;
        }
        #endregion ctor

        #region ViewModel
        public class OrdersSummaryListDetailedViewModel
        {
            public Guid Id { get; set; }
            public string UserId { get; set; }
            public ApplicationUser User { get; set; }
            public DateTime CreatedDate { get; set; }

            public int TotaalPrijs { get; set; }
            public string OrderStatus { get; set; }

            public List<OrderItem> OrderItems { get; set; }

            public record OrderItem
            {
                public Guid Id { get; set; }
                public string ProductNaam { get; set; }
                public string? Maat { get; set; }
                public string? Kleur { get; set; }
                public Guid ProductId { get; set; }
                public int Prijs { get; set; }
                public int Hoeveelheid { get; set; }
                public OrderStatus OrderStatus { get; set; }
                public string? Opmerkingen { get; set; }
            }

        }

        #endregion ViewModel

        #region CommandModel
        public class CommandModel   
        {
            public List<Guid> OrderItems { get; set; } // Verkregen via GUI
            public int OrderStatusValue { get; set; } = (int)OrderStatus.Geannuleerd;
        }
        #endregion CommandModel

        #region Methods
        public async Task<IActionResult> OnGetAsync()
        {
            var user = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{user}'.");
            }
            var ordersDetailed = await _mediator.Send(new GetOrdersForUserDetailed.Query() { UserId = user });
            Orders = ordersDetailed.Adapt<List<OrdersSummaryListDetailedViewModel>>();
            return Page();
        }

        public async Task<IActionResult> OnPostCancelAsync(Guid OrderItemId)
        {
            var cancelResult = await _mediator.Send(new UpdateOrderItems.Command() { OrderItems = Data.OrderItems, OrderStatus = Data.OrderStatusValue});
            return Content($"<span>{OrderStatus.Geannuleerd}</span>", "text/html");
        }

        #endregion Methods
    }



}
