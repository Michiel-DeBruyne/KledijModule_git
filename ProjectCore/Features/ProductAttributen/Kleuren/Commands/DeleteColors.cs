using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.ProductAttributen.Kleuren.Commands
{
    public class DeleteColors
    {
        public record DeleteCommand : IRequest<Result>
        {
            public List<Guid> KleurenToDelete { get; set; } = new List<Guid>();
        }

        public class CommandValidator : AbstractValidator<DeleteCommand>
        {
            public CommandValidator()
            {
                RuleFor(c => c.KleurenToDelete).NotNull().NotEmpty().WithMessage("Er werden geen kleuren geselecteerd om te verwijderen");
                
            }
        }

        public class DeleteCommandHandler : IRequestHandler<DeleteCommand, Result>
        {
            private readonly ApplicationDbContext _context;

            public DeleteCommandHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Result> Handle(DeleteCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var validator = new CommandValidator();
                    ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
                    if (!validationResult.IsValid)
                    {
                        return new ValidationErrorResult("Validatie mislukt voor het verwijderen van kleuren", validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList());
                    }
                    //Niet toelaten dat standaardkleur verwijderd wordt
                    var standaardKleur = await _context.ProductKleuren
                                        .Where(k => k.Kleur.Equals("Standaard"))
                                        .FirstOrDefaultAsync();
                    if (request.KleurenToDelete.Contains(standaardKleur.Id))
                    {
                        return new ErrorResult("Standaardkleur mag niet worden verwijderd.");
                    }
                    var numberOfKleurenDeleted = await _context.ProductKleuren.Where(pk => request.KleurenToDelete.Contains(pk.Id)).ExecuteDeleteAsync();
                    return new SuccessResult<int>(numberOfKleurenDeleted);
                }
                catch (DbUpdateException ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een databasefout opgetreden bij het maken van een product.");

                    // Returneer een specifieke foutresultaat voor databasegerelateerde fouten
                    return new ErrorResult("Er is een databasefout opgetreden bij het maken van het kleur.");
                }
                catch (Exception ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een onverwachte fout opgetreden bij het maken van een product.");

                    // Returneer een specifiek foutresultaat voor onverwachte fouten
                    return new ErrorResult("Er is een onverwachte fout opgetreden bij het maken van het kleur. Probeer het later opnieuw.");
                }
            }
        }
    }
}
