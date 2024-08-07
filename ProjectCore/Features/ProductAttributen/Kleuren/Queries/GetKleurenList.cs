using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.ProductAttributen.Kleuren.Queries
{
    public class GetKleurenList
    {
        public record GetKleurenListQuery : IRequest<Result>
        {
            public Guid? ProductId { get; set; }
        }

        public class GetKleurenListQueryHandler : IRequestHandler<GetKleurenListQuery, Result>
        {
            private readonly ApplicationDbContext _context;

            public GetKleurenListQueryHandler(ApplicationDbContext context)
            {
                _context = context;
            }
            public async Task<Result> Handle(GetKleurenListQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var query = _context.ProductKleuren.AsQueryable();
                    if (request.ProductId != null && request.ProductId != Guid.Empty)
                    {
                        query = query.Where(pk => !pk.AssociatedProducts.Any(p => p.Id == request.ProductId));
                    }
                    var results = await query.ProjectToType<GetKleurenListVm>().ToListAsync();
                    return new SuccessResult<List<GetKleurenListVm>>(results);
                }
                catch (Exception ex)
                {
                    return new ErrorResult("Er trad een onverwacht probleem op bij het ophalen van de kleuren. Probeer later opnieuw!");
                }
            }
        }

        public class GetKleurenListVm
        {
            public Guid Id { get; set; }
            public string Kleur { get; set; }
        }
    }
}
