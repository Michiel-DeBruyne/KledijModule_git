using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Domain.Entities.Bestellingen;
using ProjectCore.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProjectCore.Features.Orders.Queries.GetOrdersSummaryList;

namespace ProjectCore.Features.OrderItems.Queries
{
    public class GetOrderItemsList
    {
        public record Query : IRequest<Result>{
            public bool ToonAfgesloten { get; set; } = false;
            public string? ProductSearchString { get; set; }
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 50; // Aantal items per pagina
        }

        public class QueryHandler : IRequestHandler<Query, Result>
        {
            private readonly ApplicationDbContext _context;

            //Initialiseer de database context
            public QueryHandler(ApplicationDbContext context) => _context = context;
            public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    // Begin met de query naar OrderItems en projecteer deze naar OrderitemsListVm
                    var orderItemsListQuery = _context.OrderItems
                        .Select(orderItem => new OrderitemsListVm
                        {
                            Id = orderItem.Id,
                            ProductNaam = orderItem.ProductNaam,
                            Maat = orderItem.Maat,
                            Kleur = orderItem.Kleur,
                            Punten = orderItem.Punten,
                            Hoeveelheid = orderItem.Hoeveelheid,
                            OrderStatus = orderItem.OrderStatus,
                            Opmerkingen = orderItem.Opmerkingen,
                            CreatedDate = orderItem.CreatedDate,
                            UserNaam = orderItem.Order.UserNaam // Ophalen van gerelateerde 'Order' UserNaam
                        })
                        .AsQueryable();

                    // Paging toepassen
                    orderItemsListQuery = orderItemsListQuery
                        .Skip((request.Page - 1) * request.PageSize)
                        .Take(request.PageSize);

                    if (request.ProductSearchString != null && request.ProductSearchString != "")
                    {
                        orderItemsListQuery = orderItemsListQuery.Where(oi => oi.ProductNaam.Contains(request.ProductSearchString));
                    }

                    // Als ToonAfgesloten false is, filter dan afgesloten orders eruit (bv. 'Opgehaald', 'Geannuleerd', etc.)
                    if (!request.ToonAfgesloten)
                    {
                        orderItemsListQuery = orderItemsListQuery.Where(orderItem =>
                            orderItem.OrderStatus != OrderStatus.Opgehaald &&
                            orderItem.OrderStatus != OrderStatus.Retour &&
                            orderItem.OrderStatus != OrderStatus.Geannuleerd);
                    }

                    // Voer de query uit en haal de resultaten op
                    var orderItemsList = await orderItemsListQuery.OrderBy(oi => oi.ProductNaam).ToListAsync(cancellationToken);


                    // Controleer of er geen orderitems gevonden zijn
                    if (orderItemsList == null || !orderItemsList.Any())
                    {
                        return new NotFoundErrorResult("Er zijn geen orderitems gevonden.");
                    }

                    // Succesvolle response
                    return new SuccessResult<List<OrderitemsListVm>>(orderItemsList);
                }
                catch (Exception ex)
                {
                    // Log de fout indien nodig (bijv. met ILogger)
                    // Geef een algemene foutmelding terug aan de gebruiker
                    return new ErrorResult($"Er is een fout opgetreden bij het ophalen van de orderitems: {ex.Message}");
                }
            }
        }

        //Return model
        public class OrderitemsListVm
        {
            public string UserNaam { get; set; }
            public Guid Id { get; set; }
            public string ProductNaam { get; set; }
            public string? Maat { get; set; }
            public string? Kleur { get; set; }
            public int Punten { get; set; }
            public int Hoeveelheid { get; set; }
            public OrderStatus OrderStatus { get; set; }
            public string? Opmerkingen { get; set; }
            public DateTime CreatedDate { get; set; }
        }
    }
}
