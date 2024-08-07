using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.Categories.Queries
{
    public class GetRootCategories
    {
        public record Query : IRequest<Result> { }

        public class QueryHandler : IRequestHandler<Query, Result>
        {
            #region properties
            private readonly ApplicationDbContext _context;
            private readonly IMapper _mapper;
            #endregion properties

            #region ctor
            public QueryHandler(ApplicationDbContext context, IMapper mapper)
            {
                _mapper = mapper;
                _context = context;
            }
            #endregion ctor

            public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var categoriesDto = await _context.Categories
                                .Where(c => c.ParentCategorieId == null)
                                .OrderBy(c => c.Naam)
                                .ProjectToType<CategoriesListVm>()
                                .ToListAsync(cancellationToken);

                    return new SuccessResult<List<CategoriesListVm>>(categoriesDto);
                }
                catch (Exception ex)
                {
                    return new ErrorResult("Er is een onverwacht probleem opgetreden bij het ophalen van de hoofdcategorieën"); //TODO : Hier misschien de errors ook tonen ofwel enkel loggen
                }
            }
        }

        #region ViewModel
        public class CategoriesListVm
        {
            public Guid Id { get; set; }
            public string Naam { get; set; } = string.Empty;
        }

        #endregion ViewModel
    }
}
