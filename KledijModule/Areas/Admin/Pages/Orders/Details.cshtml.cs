using Htmx;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Domain.Entities.Bestellingen;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Features.OrderItems.Commands;
using ProjectCore.Features.OrderItems.Queries;
using ProjectCore.Features.Orders.Queries;
using ProjectCore.Shared.Exceptions;
using System.ComponentModel;

namespace KledijModule.Areas.Admin.Pages.Orders
{
    public class DetailsModel : PageModel
    {
        #region Properties
        private readonly IMediator _mediator;
        public OrderDetailsViewModel OrderDetails { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public CommandModel Data { get; set; }

        #endregion Properties

        #region Ctor
        public DetailsModel(IMediator mediator)
        {
            _mediator = mediator;
        }
        #endregion Ctor

        #region ViewModel
        public class OrderDetailsViewModel
        {
            public Guid Id { get; set; }
            [DisplayName("Besteldatum")]
            public DateTime CreatedDate { get; set; }
            [DisplayName("Totaal")]
            public int TotaalPrijs { get; set; } // mogelijks onnodige complexiteit om het hier te steken.
            [DisplayName("Gebruiker")]

            public string UserNaam { get; set; }

            public string UserId { get; set; }
            public List<OrderItem> OrderItems { get; set; }
            public record OrderItem
            {
                public Guid Id { get; set; }
                public string ProductNaam { get; set; }
                public string? Maat { get; set; }
                public string? Kleur { get; set; }
                public Guid? ProductId { get; set; }
                public Product? Product { get; set; }
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
            public int OrderStatusValue { get; set; }
        }
        #endregion CommandModel

        public async Task OnGetAsync(GetOrderDetails.Query query)
        {
            var GetOrderDetailsresult = await _mediator.Send(query);
            if (GetOrderDetailsresult is SuccessResult<Order> successResult)
            {
                OrderDetails = successResult.Data.Adapt<OrderDetailsViewModel>();
            }
            if (GetOrderDetailsresult is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
        }

        public async Task<IActionResult> OnGetEditAsync()
        {
            return Partial("_UpdateOrderItemStatusModal",this);
        }

        // Normaal zou je hier zetten [FromForm] int Hoeveelheid, int OrderStatusId, om tegen overposting te beschermen, maar de back-end laat enkel de 2 properties toe, dus kan niet zo kwaad denk ik. OrderItem blijven gebruiken kan toekomstige aanpassingen vergemakkelijken.
        // Update methode voor de bestelling
        public async Task<IActionResult> OnPostUpdateAsync()
        {
            // Controleer of de ontvangen gegevens geldig zijn
            if (!ModelState.IsValid)
            {
                return BadRequest("Onjuiste data ontvangen.");
            }

            // Update de bestelling met de nieuwe gegevens
            var updateResult = await _mediator.Send(new UpdateOrderItems.Command()
            {
                OrderItems = Data.OrderItems,
                OrderStatus = Data.OrderStatusValue
            });

            if (updateResult.Success)
            {
                // Geef een successtatus terug
                return new JsonResult(new { success = true });
            }
            else if (updateResult is ErrorResult errorResult)
            {
                // Verzend een foutmelding terug
                return BadRequest(errorResult.Message);
            }

            // Standaard response bij een onbepaalde fout
            return StatusCode(500, "Internal server error.");
        }
    }
}

