using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.Gebruikers.Queries
{
    public class GetUserBalans
    {
        public record Query : IRequest<Result>
        {
            public string Id { get; set; }
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
                    var result = await _context.Gebruikers.Where(user => user.Id == request.Id).ProjectToType<GetUserBalanceVm>().FirstOrDefaultAsync(cancellationToken);
                    if (result == null)
                    {
                        return new NotFoundErrorResult($"Gebruiker met ID {request.Id} kon niet gevonden worden. Werd deze misschien verwijderd?");
                    }
                    return new SuccessResult<GetUserBalanceVm>(result);
                }
                catch (Exception ex)
                {
                    return new ErrorResult($"Er trad een onverwacht probleem op bij het ophalen van de balans voor gebruiker met id {request.Id} . Gelieve IT te informeren en het later opnieuw te proberen!");
                }
            }
        }

        public class GetUserBalanceVm
        {
            public string Id { get; set; }
            public string VoorNaam { get; set; } = string.Empty;
            public string AchterNaam { get; set; } = string.Empty;

            public string FullName => VoorNaam + " " + AchterNaam;

            public int Balans { get; set; }

        }
    }
}
