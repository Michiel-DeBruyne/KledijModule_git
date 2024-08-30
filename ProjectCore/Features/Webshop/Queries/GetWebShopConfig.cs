using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.Webshop.Queries
{
    public class GetWebShopConfig
    {
        public record Query : IRequest<Result>
        {
            public Guid Id { get; set; }
        }

        #region ViewModel
        public class WebShopConfigVm
        {
            public Guid Id { get; set; }
            public DateTime OpeningDate { get; set; }
            public DateTime ClosingDate { get; set; }
        }
        #endregion ViewModel

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
                    var webshopConfigResult = await _context.WebShopConfigurations.Where(c => c.Id == request.Id).ProjectToType<WebShopConfigVm>().FirstOrDefaultAsync();
                    if (webshopConfigResult == null)
                    {
                        return new NotFoundErrorResult("Bestelperiode kon niet gevonden worden. Mogelijks werd het verwijderd?");
                    }
                    return new SuccessResult<WebShopConfigVm>(webshopConfigResult);
                }
                catch (Exception ex)
                {
                    // Handle unexpected exceptions
                    return new ErrorResult("Er is een onverwachte fout opgetreden bij het ophalen van de configuraties voor deze webwinkel. Probeer later opnieuw!");
                }
            }
        }
    }
}
