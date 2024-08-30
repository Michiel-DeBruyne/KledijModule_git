using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Domain.Entities.Bestellingen;

namespace ProjectCore.Features.Orders.Queries
{
    public class GetOrdersForUserDetailed
    {
        public record Query : IRequest<List<OrdersSummaryListDetailedVm>>
        {
            public string UserId { get; set; }
        }


        public class QueryHandler : IRequestHandler<Query, List<OrdersSummaryListDetailedVm>>
        {

            private readonly ApplicationDbContext _context;

            public QueryHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<List<OrdersSummaryListDetailedVm>> Handle(Query request, CancellationToken cancellationToken)
            {

                var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == request.UserId)
                .OrderByDescending(o => o.CreatedDate)
                .ProjectToType<OrdersSummaryListDetailedVm>()
                .ToListAsync();

                return orders;


            }
        }
        public class OrdersSummaryListDetailedVm
        {
            public Guid Id { get; set; }
            public string UserId { get; set; }
            public string UserNaam { get; set; }
            public DateTime CreatedDate { get; set; }

            public int TotaalPunten { get; set; }
            public string OrderStatus { get; set; }

            public List<OrderItem> OrderItems { get; set; }

            public record OrderItem
            {
                public Guid Id { get; set; }
                public string ProductNaam { get; set; }
                public string? Maat { get; set; }
                public string? Kleur { get; set; }
                public Guid ProductId { get; set; }
                public int Punten { get; set; }
                public int Hoeveelheid { get; set; }
                public OrderStatus OrderStatus { get; set; }
                public string? Opmerkingen { get; set; }
            }

        }
    }
}
