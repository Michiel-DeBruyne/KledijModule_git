using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.ProductAttributen.Maten.Queries
{
    public class GetMatenList
    {
        public record GetMatenListQuery : IRequest<Result>
        {
            //TODO : dit is een filter parameter, mogelijks duidelijker maken wat de bedoeling is van deze param?
            //TODO: ik wil het eventueel mogelijk maken dat deze feature kan gebruikt worden om alle maten te returnen die nog niet geselecteerd zijn voor het product. 
            // en ook welke wel voor gebruik in een andere pagina, en de algemene lijst als geen params zijn meegegeven, vermoedelijk andere queries maken voor hier genoemde use cases
            public Guid? ProductId { get; set; }
        }

        public class GetMatenListQueryHandler : IRequestHandler<GetMatenListQuery, Result>
        {
            private readonly ApplicationDbContext _context;

            public GetMatenListQueryHandler(ApplicationDbContext context)
            {
                _context = context;
            }
            public async Task<Result> Handle(GetMatenListQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var query = _context.ProductMaten.AsQueryable();
                    if (request.ProductId != null && request.ProductId != Guid.Empty)
                    {
                        query = query.Where(pm => !pm.AssociatedProducts.Any(p => p.Id == request.ProductId));
                    }
                    return new SuccessResult<List<GetMatenListVm>>(await query.ProjectToType<GetMatenListVm>().OrderByDescending(m => m.Maat).ToListAsync());
                }
                catch (Exception ex)
                {
                    return new ErrorResult("Er trad een onverwacht probleem op bij het ophalen van de maten. Probeer later opnieuw!");
                }
            }
        }

        public class GetMatenListVm
        {
            public Guid Id { get; set; }
            public string Maat { get; set; }
        }
    }
}
