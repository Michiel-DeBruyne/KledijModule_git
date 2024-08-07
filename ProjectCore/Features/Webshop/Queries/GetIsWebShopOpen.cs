using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.Webshop.Queries
{
    public class GetIsWebShopOpen
    {
        public record Query : IRequest<Result> { }

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
                    var currentDate = DateTime.Now;
                    var isWebShopOpen = await _context.WebShopConfigurations
                                            .Where(s => s.OpeningDate <= currentDate && currentDate <= s.ClosingDate)
                                            .AnyAsync();
                    return new SuccessResult<bool>(isWebShopOpen);
                }
                catch (Exception ex)
                {
                    // Handle unexpected exceptions
                    return new ErrorResult("Er is een onverwachte fout opgetreden bij het ophalen van de openingsuren voor deze webwinkel. Probeer later opnieuw!");
                }
            }
        }
    }
}
