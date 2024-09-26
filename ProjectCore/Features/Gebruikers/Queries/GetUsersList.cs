using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.Gebruikers.Queries
{
    public class GetUsersList
    {

        public record Query : IRequest<Result>
        {
            public string Id { get; set; }
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
                //TypeAdapterConfig<ApplicationUser, GetusersListVm>.NewConfig()
                //            .Map(dest => dest.UserId, src => src.Id);
                try
                {
                    var result = await _context.Gebruikers.ProjectToType<GetusersListVm>().OrderBy(gebr => gebr.VoorNaam).ToListAsync(cancellationToken);
                    return new SuccessResult<List<GetusersListVm>>(result);
                }
                catch (Exception ex)
                {
                    return new ErrorResult("Er trad een onverwacht probleem op bij het ophalen van de gebruikers. Gelieve IT te informeren en het later opnieuw te proberen!");
                }
            }
        }

        public class GetusersListVm
        {
            public string Id { get; set; }
            public string VoorNaam { get; set; } = string.Empty;
            public string AchterNaam { get; set; } = string.Empty;

            public string FullName => VoorNaam + " " + AchterNaam;

            public string Email { get; set; }
            public int Balans { get; set; }
        }
    }
}
