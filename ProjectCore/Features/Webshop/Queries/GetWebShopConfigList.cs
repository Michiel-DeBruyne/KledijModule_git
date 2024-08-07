using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.Webshop.Queries
{
    public class GetWebShopConfigList
    {
        public record Query : IRequest<Result>
        {

        }

        #region ViewModel
        public class WebShopConfigVm
        {
            public Guid Id { get; set; }
            public DateTime OpeningDate { get; set; }
            public DateTime ClosingDate { get; set; }
            public bool IsWebshopOpen()
            {
                DateTime currentDate = DateTime.Now;
                return currentDate >= OpeningDate && currentDate <= ClosingDate;
            }
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
                    var webshopConfigResult = await _context.WebShopConfigurations.ProjectToType<WebShopConfigVm>().ToListAsync();
                    return new SuccessResult<List<WebShopConfigVm>>(webshopConfigResult);
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
