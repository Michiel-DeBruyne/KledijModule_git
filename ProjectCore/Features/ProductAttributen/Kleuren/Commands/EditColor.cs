using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.ProductAttributen.Kleuren.Commands
{
    public class EditColor
    {
        public record EditColorCommand : IRequest<Result>
        {
            public Guid Id { get; set; }
            public string Kleur { get; set; }
        }

        public class CommandValidator : AbstractValidator<EditColorCommand>
        {
            public CommandValidator()
            {
                RuleFor(c => c.Id).NotNull().NotEmpty();
                RuleFor(c => c.Kleur).NotNull().NotEmpty();
            }
        }

        public class EditColorCommandHandler : IRequestHandler<EditColorCommand, Result>
        {
            private readonly ApplicationDbContext _context;

            public EditColorCommandHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Result> Handle(EditColorCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var validator = new CommandValidator();
                    var validationResult = await validator.ValidateAsync(request, cancellationToken);

                    if (!validationResult.IsValid)
                    {
                        return new ValidationErrorResult("Validatie mislukt voor het updaten van een kleur", validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList());
                    }
                    var kleur = await _context.ProductKleuren.FindAsync(request.Id);
                    if (kleur == null)
                    {
                        return new NotFoundErrorResult($"Kleur met id {request.Id} & kleur {request.Kleur} werd niet gevonden, mogelijks werd het door een andere gebruiker verwijderd?");
                    }
                    kleur.Kleur = request.Kleur;
                    await _context.SaveChangesAsync();
                    return new SuccessResult();

                }
                catch (DbUpdateException ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een databasefout opgetreden bij het maken van een product.");

                    // Returneer een specifieke foutresultaat voor databasegerelateerde fouten
                    return new ErrorResult("Er is een databasefout opgetreden bij het updaten van het kleur.");
                }
                catch (Exception ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een onverwachte fout opgetreden bij het maken van een product.");

                    // Returneer een specifiek foutresultaat voor onverwachte fouten
                    return new ErrorResult("Er is een onverwachte fout opgetreden bij het updaten van het kleur. Probeer het later opnieuw.");
                }
            }
        }

    }
}
