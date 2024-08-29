using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Domain.Entities.Bestellingen;
using ProjectCore.Domain.Entities.Gebruiker;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.Orders.Queries
{
    public class GetOrdersSummaryList
    {
        public record Query : IRequest<Result>
        {
            public string? UserNaam { get; set; }
            public string? Status { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, Result>
        {
            private readonly ApplicationDbContext _context;

            public QueryHandler(ApplicationDbContext context) => _context = context;

            public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var config = new TypeAdapterConfig();
                    config.NewConfig<Order, OrdersSummaryListVm>()
                        .Map(dest => dest.OrderStatus, src => src.OrderItems.Any(oi =>
                                                                oi.OrderStatus == OrderStatus.Open ||
                                                                oi.OrderStatus == OrderStatus.Besteld ||
                                                                oi.OrderStatus == OrderStatus.OpTeHalen ||
                                                                oi.OrderStatus == OrderStatus.Retour) ? "in behandeling" : "Afgehandeld");
                    var ordersQuery =  _context.Orders.AsQueryable();
                    if(request.Status == "Openstaande bestellingen") //Hardcoded I know. Later misschien evalueren?
                    {
                        ordersQuery = ordersQuery.Where(order => order.OrderItems.Any(oi =>
                            oi.OrderStatus == OrderStatus.Open ||
                            oi.OrderStatus == OrderStatus.Besteld ||
                            oi.OrderStatus == OrderStatus.OpTeHalen ||
                            oi.OrderStatus == OrderStatus.Retour));
                    }
                    else
                    {
                        ordersQuery = ordersQuery.Where(order => order.OrderItems.Any(oi =>
                            oi.OrderStatus == OrderStatus.Opgehaald ||
                            oi.OrderStatus == OrderStatus.Geannuleerd));
                    }
                    // Projecteer de query naar het viewmodel en uitvoeren
                    var orders = await ordersQuery
                        .ProjectToType<OrdersSummaryListVm>(config)
                        .ToListAsync(cancellationToken);

                    return new SuccessResult<List<OrdersSummaryListVm>>(orders);
                }
                catch (Exception ex)
                {
                    // Handle unexpected exceptions
                    return new ErrorResult("Er is een onverwachte fout opgetreden bij het ophalen van de bestellingen. Probeer later opnieuw!");
                }
            }
        }

        public class OrdersSummaryListVm
        {
            public Guid Id { get; set; }

            public string UserId { get; set; }
            public string UserNaam { get; set; }
            public DateTime CreatedDate { get; set; }

            public int TotaalPunten { get; set; }
            public string OrderStatus { get; set; }


        }
    }
}
