using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Domain.Entities.WinkelMand;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.WinkelMand.Queries
{
    public class GetShoppingCartItemWithDetails
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
                    var result = await _context.ShoppingCartItems.Where(shi => shi.Id == request.Id).FirstOrDefaultAsync(cancellationToken);
                    return (result != null) ?
                        new SuccessResult<GetShoppingCartItemVm>(result.Adapt<GetShoppingCartItemVm>()) :
                        new ErrorResult($"Shoppingcart item met id {request.Id} kon niet worden gevonden.");
                }
                catch (Exception ex)
                {
                    // Handle unexpected exceptions
                    return new ErrorResult("Er is een onverwachte fout opgetreden bij het ophalen van het winkelmand item. Probeer later opnieuw!");
                }
            }

        }
        public class GetShoppingCartItemVm
        {
            public Guid Id { get; set; }

            public Guid ShoppingCartId { get; set; }

            public ShoppingCart ShoppingCart { get; set; }
            public Guid ProductId { get; set; }
            public Product Product { get; set; }
            public int Hoeveelheid { get; set; }
            public string? Kleur { get; set; }
            public string? Maat { get; set; }

            public string? Opmerkingen { get; set; }
        }

    }
}
