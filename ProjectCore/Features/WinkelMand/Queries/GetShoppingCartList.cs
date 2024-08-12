using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.WinkelMand.Queries
{
    public class GetShoppingCartList
    {

        #region ReturnModel

        public class GetShoppingCartListVm
        {
            public Guid Id { get; set; }
            public string GebruikerId { get; set; }
            public List<ShoppingCartItem> ShoppingCartItems { get; set; } = new List<ShoppingCartItem>();
            public record ShoppingCartItem
            {
                public Guid Id { get; set; }
                public Product Product { get; set; }
                public int Hoeveelheid { get; set; }
                public string? Kleur { get; set; }
                public string? Maat { get; set; }
                public string? Opmerkingen { get; set; }
            }

        }
        #endregion ReturnModel
        public record Query : IRequest<Result>
        {
            public string UserId { get; set; }
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
                    var shoppingCart = await _context.ShoppingCarts
                        .Include(sc => sc.ShoppingCartItems)
                        .ProjectToType<GetShoppingCartListVm>()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(sc => sc.GebruikerId == request.UserId);
                    return new SuccessResult<GetShoppingCartListVm>(shoppingCart == null ? new GetShoppingCartListVm() { GebruikerId = request.UserId } : shoppingCart);
                }
                catch (Exception ex)
                {
                    // Handle unexpected exceptions
                    return new ErrorResult("Er is een onverwachte fout opgetreden bij het ophalen van je winkelmand. Probeer later opnieuw!");
                }
            }

        }
    }
}
