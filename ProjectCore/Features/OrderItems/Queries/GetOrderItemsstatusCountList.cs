using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Domain.Entities.Bestellingen;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.OrderItems.Queries
{
    public class GetOrderItemsStatusCountList
    {
        public record Query : IRequest<Result>
        {

        }

        public class QueryHandler : IRequestHandler<Query, Result>
        {
            private readonly ApplicationDbContext _context;

            public QueryHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var summary = new OrderItemSummaryListViewModel();

                    summary.OpenCount = await _context.OrderItems.CountAsync(oi => oi.OrderStatus == OrderStatus.Open);
                    summary.BesteldCount = await _context.OrderItems.CountAsync(oi => oi.OrderStatus == OrderStatus.Besteld);
                    summary.OpTeHalenCount = await _context.OrderItems.CountAsync(oi => oi.OrderStatus == OrderStatus.OpTeHalen);

                    return new SuccessResult<OrderItemSummaryListViewModel>(summary);
                }
                catch (Exception ex)
                {
                    return new ErrorResult(ex.Message);
                }
            }
        }

        public class OrderItemSummaryListViewModel
        {
            public int OpenCount { get; set; }
            public int BesteldCount { get; set; }
            public int OpTeHalenCount { get; set; }
        }
    }
}
