using FluentValidation;
using FluentValidation.Results;
using MediatR;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.ProductAttributen.Maten.Commands
{
    public class EditSize
    {
        public record Command : IRequest<Result>
        {
            public Guid Id { get; set; }
            public string Maat { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.Id).NotEmpty();
                RuleFor(c => c.Maat).NotEmpty();
            }
        }

        public class CommandHandler : IRequestHandler<Command, Result>
        {
            private readonly ApplicationDbContext _context;

            public CommandHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    var validator = new CommandValidator();
                    ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);

                    if (!validationResult.IsValid)
                    {
                        return new ValidationErrorResult("Validatie voor het updaten van de maat mislukt", validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList());
                    }
                    var maat = await _context.ProductMaten.FindAsync(request.Id, cancellationToken);
                    if (maat == null)
                    {
                        return new NotFoundErrorResult($"de maat met id {request.Id} kon niet worden gevonden, mogelijks werd het verwijderd?");
                    }

                    maat.Maat = request.Maat;
                    await _context.SaveChangesAsync();

                    return new SuccessResult();
                }
                catch (Exception ex)
                {
                    return new ErrorResult("Er trad een onverwacht probleem op bij het ophalen van de maat. Probeer later opnieuw!");
                }
            }
        }


    }
}
