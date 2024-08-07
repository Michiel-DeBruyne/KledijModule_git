using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.Categories.Queries
{
    public class GetSubCategoriesForCategorie
    {
        public record Query : IRequest<Result>
        {
            public Guid CategorieId { get; set; }
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
                    var result = await _context.Categories
                        .Where(c => c.Id == request.CategorieId)
                        .SelectMany(c => c.SubCategorieën)
                        .ToListAsync();
                    return new SuccessResult<List<Categorie>>(result);
                }
                catch (Exception ex)
                {
                    return new ErrorResult("Er is een onverwacht probleem opgetreden tijdens het ophalen van de subcategorieen");
                }

            }
        }
    }
}
