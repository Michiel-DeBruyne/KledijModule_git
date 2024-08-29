using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.Producten.Queries
{
    public class GetProductMetDetails
    {

        public record Query : IRequest<Result>
        {
            public Guid Id { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, Result>
        {

            private readonly ApplicationDbContext _context;
            private readonly IMapper _mapper;

            public QueryHandler(ApplicationDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }
            public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var ProductDetails = await _context.Producten
                                            .Include(p => p.Maten)
                                            .Include(p => p.Kleuren)
                                            .Include(p => p.Fotos)
                                            .AsNoTracking()
                                            .ProjectToType<ProductMetDetailsVm>()
                                            .FirstOrDefaultAsync(p => p.Id == request.Id);

                    if (ProductDetails == null)
                    {
                        return new NotFoundErrorResult($"Product met id {request.Id} kon niet gevonden worden tijdens het ophalen van de details. Mogelijks het werd het verwijderd?");
                    }
                    return new SuccessResult<ProductMetDetailsVm>(ProductDetails);
                }
                catch (Exception ex)
                {
                    return new ErrorResult("Er trad een overwachte fout op, probeer later opnieuw!");
                }
            }
        }

        public class ProductMetDetailsVm
        {
            public Guid Id { get; set; }
            public string Naam { get; set; } = string.Empty;
            public string? Beschrijving { get; set; }
            public bool Beschikbaar { get; set; } = false;
            public int Punten { get; set; }
            public Geslacht Geslacht { get; set; }
            public int? ArtikelNummer { get; set; }

            public Guid CategorieId { get; set; }
            //public ProductCategorie Categorie { get; set; } = default!;
            //public record ProductCategorie
            //{
            //    public Guid Id { get; set; }
            //    public string Naam { get; set; }
            //}

            #region VervangingsTermijn

            public int MaxAantalBestelbaar { get; set; }

            public int PerAantalJaar { get; set; }

            public string DisplayVervangingsTermijn
            {
                get
                {
                    return $"{MaxAantalBestelbaar} per {PerAantalJaar} {(PerAantalJaar > 1 ? "jaren" : "jaar")}";
                }
            }

            #endregion VervangingsTermijn


            public List<Foto> Fotos { get; set; } = new List<Foto>();
            public List<ProductMaat> Maten { get; set; } = new List<ProductMaat>();
            public List<ProductKleur> Kleuren { get; set; } = new List<ProductKleur>();
        }
    }
}
