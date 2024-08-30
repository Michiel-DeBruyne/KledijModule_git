using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.Producten.Queries
{
    public class GetFotosForProductList
    {
        public record Query : IRequest<Result>
        {
            public Guid ProductId { get; set; }
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
                    var product = await _context.Producten.FindAsync(request.ProductId);
                    if (product == null)
                    {
                        return new NotFoundErrorResult($"Product met Id {request.ProductId} kon niet gevonden worden, mogelijks werd het verwijderd?");
                    }
                    var FotosForProduct = await _context.ProductFotos
                    .Where(p => p.ProductId == request.ProductId)
                    .ProjectToType<ProductFotoListVm>()
                    .ToListAsync();

                    return new SuccessResult<List<ProductFotoListVm>>(FotosForProduct);
                }
                catch (Exception ex)
                {
                    return new ErrorResult("Er trad een overwachte fout op, gelieve later nogmaals te proberen.");
                }
            }
        }

        public class ProductFotoListVm
        {
            public Guid Id { get; set; }
            public Guid ProductId { get; set; }
            public string ImageUrl { get; set; }
            public int Volgorde { get; set; }
        }
    }
}
