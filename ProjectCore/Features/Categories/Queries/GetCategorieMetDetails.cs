using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.Categories.Queries
{
    public class GetCategorieMetDetails
    {
        public record Query : IRequest<Result>
        {
            public Guid Id { get; set; }
        }


        public class QueryHandler : IRequestHandler<Query, Result>
        {
            #region properties
            private readonly ApplicationDbContext _context;
            private IMapper _mapper;
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
                    var category = await _context.Categories.ProjectToType<GetCategorieMetDetailsVm>().FirstOrDefaultAsync(c => c.Id == request.Id);
                    if (category == null)
                    {
                        return new NotFoundErrorResult($"Categorie met id {request.Id} kon niet gevonden worden tijdens het ophalen van de details. Mogelijks werd het verwijderd?");
                    }
                    return new SuccessResult<GetCategorieMetDetailsVm>(category);
                }
                catch (Exception ex)
                {
                    return new ErrorResult("Er is een onverwacht probleem opgetreden tijdens het ophalen van de categorie met zijn details. Probeer later opnieuw."); //TODO : Hier misschien de errors ook tonen ofwel enkel loggen
                }
            }
        }

        #region ReturnModel
        // dit return je
        public class GetCategorieMetDetailsVm
        {
            public Guid Id { get; set; }
            public string Naam { get; set; }
            public string? Beschrijving { get; set; }

            public Guid? ParentCategorieId { get; set; }

            public List<Categorie> SubCategorieën = new List<Categorie>();

            public record Categorie
            {
                public Guid Id { get; set; }
                public string Naam { get; set; }
            }
        }
        #endregion ReturnModel
    }
}
