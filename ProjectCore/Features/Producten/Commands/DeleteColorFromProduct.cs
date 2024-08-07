using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.Producten.Commands
{
    public class DeleteColorFromProduct
    {
        public record DeleteColorFromProductCommand : IRequest<Result>
        {
            public Guid ProductId { get; set; }
            public Guid KleurId { get; set; }

        }

        public class CommandValidator : AbstractValidator<DeleteColorFromProductCommand>
        {
            public CommandValidator()
            {
                RuleFor(c => c.ProductId).NotEmpty();
                RuleFor(c => c.KleurId).NotEmpty();
            }
        }

        public class DeleteColorFromProductCommandHandler : IRequestHandler<DeleteColorFromProductCommand, Result>
        {
            private readonly ApplicationDbContext _context;

            public DeleteColorFromProductCommandHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Result> Handle(DeleteColorFromProductCommand request, CancellationToken cancellationToken)
            {
                var validator = new CommandValidator();
                ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
                try
                {
                    if (!validationResult.IsValid)
                    {
                        return new ValidationErrorResult($"Validatie fout ", validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList());
                    }
                    // Zoek het product op basis van de ID
                    var product = await _context.Producten
                        .Include(p => p.Kleuren)
                        .FirstOrDefaultAsync(p => p.Id == request.ProductId);

                    if (product == null)
                    {
                        return new NotFoundErrorResult($"Product met ID {request.ProductId} kon niet worden gevonden.");
                    }

                    // Zoek de kleur die moet worden verwijderd uit de lijst met kleuren van het product
                    var kleurTeVerwijderen = product.Kleuren.FirstOrDefault(k => k.Id == request.KleurId);

                    if (kleurTeVerwijderen == null)
                    {
                        return new NotFoundErrorResult($"Kleur met ID {request.KleurId} kon niet worden gevonden in het product.");
                    }

                    // Verwijder de kleur uit de lijst met kleuren van het product
                    product.Kleuren.Remove(kleurTeVerwijderen);

                    // Sla wijzigingen op in de database
                    await _context.SaveChangesAsync();

                    return new SuccessResult();
                }
                catch (DbUpdateException ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een databasefout opgetreden bij het maken van een product.");

                    // Returneer een specifieke foutresultaat voor databasegerelateerde fouten
                    return new ErrorResult("Er is een databasefout opgetreden bij het maken van het product.");
                }
                catch (Exception ex)
                {
                    // Log de uitzondering
                    // _logger.LogError(ex, "Er is een fout opgetreden bij het verwijderen van een kleur uit een product.");

                    // Returneer een specifiek foutresultaat voor onverwachte fouten
                    return new ErrorResult("Er is een onverwachte fout opgetreden bij het verwijderen van een kleur uit een product. Probeer het later opnieuw.");
                }
            }

        }
    }
}
