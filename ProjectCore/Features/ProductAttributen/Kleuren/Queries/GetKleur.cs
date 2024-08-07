using Mapster;
using MediatR;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.ProductAttributen.Kleuren.Queries
{
    public class GetKleur
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
                    var kleur = await _context.ProductKleuren.FindAsync(request.Id);

                    if (kleur == null)
                    {
                        return new NotFoundErrorResult($"Product met Id {request.Id} kon niet worden gevonden. Mogelijks werd het verwijderd?");
                    }

                    return new SuccessResult<GetKleurVm>(kleur.Adapt<GetKleurVm>());
                }
                catch (Exception ex)
                {
                    return new ErrorResult("Er trad een onverwacht probleem op bij het ophalen van het kleur. Probeer later opnieuw!");
                }
            }
        }


        public class GetKleurVm
        {

            public Guid Id { get; set; }
            public string Kleur { get; set; }
        }
    }
}
