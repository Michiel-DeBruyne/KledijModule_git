using Mapster;
using MediatR;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.ProductAttributen.Maten.Queries
{
    public class GetMaat
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
                    var maat = await _context.ProductMaten.FindAsync(request.Id);
                    if (maat == null)
                    {
                        return new NotFoundErrorResult($"Maat met Id {request.Id} werd niet gevonden. Mogelijks werd het verwijderd?");
                    }

                    return new SuccessResult<GetMaatVm>(maat.Adapt<GetMaatVm>());
                }
                catch (Exception ex)
                {
                    return new ErrorResult("Er trad een onverwacht probleem op bij het ophalen van de maat, probeer later opnieuw!"); //TODO : misschien ex.Message achter plakken al zal de gebruiker hier minder aan hebben, misschien error gewoon loggen?
                }
            }
        }

        public class GetMaatVm
        {
            public Guid Id { get; set; }
            public string Maat { get; set; }
        }
    }
}
