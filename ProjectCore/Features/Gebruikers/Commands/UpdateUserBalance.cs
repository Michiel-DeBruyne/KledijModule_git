using FluentValidation;
using MediatR;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCore.Features.Gebruikers.Commands
{
    public class UpdateUserBalance
    {
        public record Command:IRequest<Result>
        {
            public string Id { get;set; }
            public int Balans { get;set; }
        }

        public class CommandValidator:AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.Id).NotEmpty().NotNull();
                RuleFor(c => c.Balans).NotEmpty().NotNull().GreaterThan(0);
            }
        }

        public class CommandHandler : IRequestHandler<Command, Result>
        {

            private readonly ApplicationDbContext _context;

            public CommandHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            public Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}
