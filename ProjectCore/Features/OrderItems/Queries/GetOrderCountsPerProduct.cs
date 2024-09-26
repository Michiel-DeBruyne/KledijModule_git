using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Domain.Entities.Bestellingen;
using ProjectCore.Shared.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectCore.Features.OrderItems.Queries
{
    public class GetOrderCountsPerProduct
    {
        /// <summary>
        /// Model dat het overzicht per product weergeeft, inclusief het totaal aantal bestellingen en details per maat en kleur.
        /// </summary>
        public class ProductOrderOverview
        {
            /// <summary>
            /// De naam van het product.
            /// </summary>
            public string ProductNaam { get; set; } = string.Empty;

            /// <summary>
            /// Het totaal aantal keer dat dit product is besteld.
            /// </summary>
            public int TotaalBesteld { get; set; }

            /// <summary>
            /// Een lijst met details over het aantal bestellingen per maat.
            /// </summary>
            public List<MaatDetail> MaatDetails { get; set; } = new List<MaatDetail>();
        }

        /// <summary>
        /// Model dat de details over de maat van een product bevat, inclusief het aantal kleuren.
        /// </summary>
        public class MaatDetail
        {
            /// <summary>
            /// De maat van het product.
            /// </summary>
            public string? Maat { get; set; }

            /// <summary>
            /// Het aantal bestellingen van deze specifieke maat.
            /// </summary>
            public int Aantal { get; set; }

            /// <summary>
            /// Een lijst met details over de kleuren voor deze maat.
            /// </summary>
            public List<KleurDetail> KleurDetails { get; set; } = new List<KleurDetail>();
        }

        /// <summary>
        /// Model dat de details over de kleur van een product bevat.
        /// </summary>
        public class KleurDetail
        {
            /// <summary>
            /// De kleur van het product.
            /// </summary>
            public string? Kleur { get; set; }

            /// <summary>
            /// Het aantal bestellingen van deze specifieke kleur.
            /// </summary>
            public int Aantal { get; set; }
        }


        // The query class
        public record Query : IRequest<Result>
        {
            public DateTime? VanDatum { get; set; }
            public DateTime? TotDatum { get; set; }

        }


        // The query handler
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
                    // Start de query
                    var productOverzichtQuery = _context.OrderItems
                        .Where(o => o.OrderStatus == OrderStatus.Open);

                    // Voeg de filter voor VanDatum toe als deze is ingevuld
                    if (request.VanDatum.HasValue)
                    {
                        productOverzichtQuery = productOverzichtQuery
                            .Where(o => o.CreatedDate >= request.VanDatum);
                    }

                    // Voeg de filter voor TotDatum toe als deze is ingevuld
                    if (request.TotDatum.HasValue)
                    {
                        productOverzichtQuery = productOverzichtQuery
                            .Where(o => o.CreatedDate <= request.TotDatum);
                    }

                    // Groepeer en selecteer de resultaten
                    var productOverzicht = await productOverzichtQuery
                        .GroupBy(o => o.ProductNaam)
                        .Select(g => new ProductOrderOverview
                        {
                            ProductNaam = g.Key,
                            TotaalBesteld = g.Sum(o => o.Hoeveelheid),
                            MaatDetails = g.GroupBy(o => o.Maat)
                                .Select(m => new MaatDetail
                                {
                                    Maat = m.Key,
                                    Aantal = m.Sum(o => o.Hoeveelheid),
                                    KleurDetails = m.GroupBy(o => o.Kleur)
                                        .Select(k => new KleurDetail
                                        {
                                            Kleur = k.Key,
                                            Aantal = k.Sum(o => o.Hoeveelheid)
                                        })
                                        .ToList()
                                })
                                .ToList()
                        })
                        .ToListAsync(cancellationToken);


                    // Return the result
                    return new SuccessResult<List<ProductOrderOverview>>(productOverzicht);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("An error occurred while retrieving order data.", ex);
                }
            }
        }
    }
}
