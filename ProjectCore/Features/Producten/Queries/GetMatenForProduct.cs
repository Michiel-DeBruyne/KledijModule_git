using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.Producten.Queries
{
    public class GetMatenForProduct
    {
        public record Query : IRequest<Result>
        {
            public Guid ProductId { get; set; }
        }

        public class GetMatenForProductQueryHandler : IRequestHandler<Query, Result>
        {
            private readonly ApplicationDbContext _context;

            public GetMatenForProductQueryHandler(ApplicationDbContext context)
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
                    var matenForProduct = await _context.Producten
                                        .Include(p => p.Maten)
                                        .Where(p => p.Id == request.ProductId)
                                        .SelectMany(p => p.Maten)
                                        .ProjectToType<GetMatenForProductListVm>()
                                        .ToListAsync();

                    return new SuccessResult<List<GetMatenForProductListVm>>(matenForProduct);
                }
                catch (Exception ex)
                {
                    return new ErrorResult("Er trad een overwachte fout op, gelieve later nogmaals te proberen.");
                }
            }
        }


        public class GetMatenForProductListVm
        {
            public Guid Id { get; set; }
            public string Maat { get; set; }
        }
    }
}
