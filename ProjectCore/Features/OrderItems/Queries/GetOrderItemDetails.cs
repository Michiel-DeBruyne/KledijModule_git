using Mapster;
using MediatR;
using ProjectCore.Data;
using ProjectCore.Domain.Entities.Bestellingen;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.OrderItems.Queries
{
    public class GetOrderItemDetails
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
                    var orderItem = await _context.OrderItems.FindAsync(request.Id, cancellationToken);
                    if (orderItem == null)
                    {
                        return new NotFoundErrorResult("Orderitem niet gevonden");
                    }
                    return new SuccessResult<OrderItemDetailsVm>(orderItem.Adapt<OrderItemDetailsVm>());
                }
                catch (Exception ex)
                {
                    // Handle unexpected exceptions
                    return new ErrorResult("Er is een onverwachte fout opgetreden tijdens het ophalen van de details van uw bestellingsitem. Probeer later opnieuw!");
                }
            }
        }

        public class OrderItemDetailsVm
        {
            public Guid Id { get; set; }
            public string ProductNaam { get; set; }
            public string? Maat { get; set; }
            public string? Kleur { get; set; }
            public int Prijs { get; set; }
            public int Hoeveelheid { get; set; }
            public OrderStatus OrderStatus { get; set; }
            public string? Opmerkingen { get; set; }
        }
    }
}
