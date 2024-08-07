using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectCore.Data;
using ProjectCore.Features.OrderItems.Queries;
using ProjectCore.Shared.Exceptions;

namespace KledijModule.Areas.Admin.Pages.Shared.Components.OrderItemsStatusCountList
{
    public class OrderItemsStatusCountListViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;
        public OrderItemSummaryListViewModel OrderItemsStatusCountModel { get; set; } = new OrderItemSummaryListViewModel();

        public OrderItemsStatusCountListViewComponent(IMediator mediator) => _mediator = mediator;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var result = await _mediator.Send(new GetOrderItemsStatusCountList.Query());
            if(result is SuccessResult<GetOrderItemsStatusCountList.OrderItemSummaryListViewModel> successResult) {
                OrderItemsStatusCountModel = successResult.Data.Adapt<OrderItemSummaryListViewModel>();
            }
            if(result is ErrorResult errorResult)
            {
                OrderItemsStatusCountModel.Error = errorResult.Message;
            }
            return View(OrderItemsStatusCountModel);
        }

        public class OrderItemSummaryListViewModel
        {
            public string? Error { get; set; }
            public int OpenCount { get; set; }
            public int BesteldCount { get; set; }
            public int OpTeHalenCount { get; set; }
        }
    }
}
