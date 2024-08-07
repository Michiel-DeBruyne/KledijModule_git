using Htmx;
using Mapster;
using MediatR;
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

        public async Task<IActionResult> OnGetEdit()
        {
            var orderItemResult = await _mediator.Send(new GetOrderItemDetails.Query() { Id = Id });
            if (orderItemResult is SuccessResult<GetOrderItemDetails.OrderItemDetailsVm> successResult)
            {
                return Partial("_OrderItemEditRow", successResult.Data.Adapt<OrderDetailsViewModel.OrderItem>());
            }
            if (orderItemResult.Failure)
            {
                if (orderItemResult is ErrorResult errorResult)
                {
                    TempData["Errors"] = errorResult.Message;
                }
            }
            return RedirectToAction("Get");
        }
        public async Task<IActionResult> OnGetRow()
        {
            var orderItemResult = await _mediator.Send(new GetOrderItemDetails.Query() { Id = Id });
            if (orderItemResult is SuccessResult<GetOrderItemDetails.OrderItemDetailsVm> successResult)
            {
                return Partial("_OrderItemRow", successResult.Data.Adapt<OrderDetailsViewModel.OrderItem>());
            }
            if (orderItemResult.Failure)
            {
                if (orderItemResult is ErrorResult errorResult)
                {
                    TempData["Errors"] = errorResult.Message;
                }
            }
            return RedirectToAction("Get");
        }

        // Normaal zou je hier zetten [FromForm] int Hoeveelheid, int OrderStatusId, om tegen overposting te beschermen, maar de back-end laat enkel de 2 properties toe, dus kan niet zo kwaad denk ik. OrderItem blijven gebruiken kan toekomstige aanpassingen vergemakkelijken.
        public async Task<IActionResult> OnPostUpdate([FromForm] OrderDetailsViewModel.OrderItem OrderItem)
        {
            var updateResult = await _mediator.Send(new UpdateOrderItem.Command() { Id = Id, OrderStatus = (int)OrderItem.OrderStatus });
            if (updateResult.Success)
            {
                var orderItemResult = await _mediator.Send(new GetOrderItemDetails.Query() { Id = Id });
                if (orderItemResult is SuccessResult<GetOrderItemDetails.OrderItemDetailsVm> successResult)
                {
                    return Request.IsHtmx()
                            ? Partial("_OrderItemRow", successResult.Data.Adapt<OrderDetailsViewModel.OrderItem>())
                            : RedirectToPage("Details", new { OrderItem.Id });
                }
                if (orderItemResult is ErrorResult orderItemErrorResult)
                {
                    TempData["Errors"] = orderItemErrorResult.Message;
                }
            }
            if (updateResult is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
            return RedirectToPage("Details", new { OrderItem.Id });
        }
    }
}

