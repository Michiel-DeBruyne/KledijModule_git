using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProjectCore.Features.Gebruikers.Queries.GetUsersList;

namespace ProjectCore.Features.Gebruikers.Queries
{
    public class GetUserBalans
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
                try
                {
                    var result = await _context.Gebruikers.ProjectToType<GetUserBalanceVm>().FirstOrDefaultAsync(cancellationToken);
                    return new SuccessResult<GetUserBalanceVm>(result);
                }
                catch (Exception ex)
                {
                    return new ErrorResult($"Er trad een onverwacht probleem op bij het ophalen van de balans voor gebruiker met id {request.Id} . Gelieve IT te informeren en het later opnieuw te proberen!");
                }
            }
        }

        public class GetUserBalanceVm
        {
            public string Id { get; set; }
            public string VoorNaam { get; set; } = string.Empty;
            public string AchterNaam { get; set; } = string.Empty;

            public string FullName => VoorNaam + " " + AchterNaam;

            public int Balans { get; set; }

        }
    }
}
