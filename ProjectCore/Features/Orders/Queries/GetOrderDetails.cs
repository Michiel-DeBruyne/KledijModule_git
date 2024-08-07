using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Domain.Entities.Bestellingen;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.Orders.Queries
{
    public class GetOrderDetails
    {
        public record Query : IRequest<Result>
        {
            public Guid Id { get; set; }
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
                    var order = await _context.Orders
                                .Include(o => o.OrderItems)
                                    .ThenInclude(oi => oi.Product)
                                .FirstOrDefaultAsync(m => m.Id == request.Id);
                    if (order == null)
                    {
                        return new ErrorResult("Bestelling kon niet gevonden worden.");
                    }
                    else
                    {
                        return new SuccessResult<Order>(order);
                    }
                }
                catch (Exception ex)
                {
                    // Handle unexpected exceptions
                    return new ErrorResult("Er is een onverwachte fout opgetreden bij het ophalen van de bestellingdetails. Probeer later opnieuw!");
                }
            }
        }

    }
}
