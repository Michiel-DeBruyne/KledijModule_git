using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.Producten.Commands
{
    public class DeleteSizeFromproduct
    {
        public record DeleteSizeFromproductCommand : IRequest<Result>
        {
            public Guid ProductId { get; set; }
            public Guid MaatId { get; set; }

        }

        public class CommandValidator : AbstractValidator<DeleteSizeFromproductCommand>
        {
            public CommandValidator()
            {
                RuleFor(c => c.ProductId).NotEmpty().WithMessage("Product-ID mag niet leeg zijn.");
                RuleFor(c => c.MaatId).NotEmpty().WithMessage("Maat-ID mag niet leeg zijn.");
            }
        }

        public class DeleteSizeFromproductCommandHandler : IRequestHandler<DeleteSizeFromproductCommand, Result>
        {

            private readonly ApplicationDbContext _context;

            public DeleteSizeFromproductCommandHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Result> Handle(DeleteSizeFromproductCommand request, CancellationToken cancellationToken)
            {
                // Zoek het product op basis van de ID
                var validator = new CommandValidator();
                ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
                try
                {
                    if (!validationResult.IsValid)
                    {
                        return new ValidationErrorResult($"Validatie fout bij het verwijderen van een maat voor een product", validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList());
                    }
                    // Zoek het product op basis van de ID
                    var product = await _context.Producten
                        .Include(p => p.Maten)
                        .FirstOrDefaultAsync(p => p.Id == request.ProductId);

                    if (product == null)
                    {
                        return new NotFoundErrorResult($"Product met ID {request.ProductId} kon niet worden gevonden.");
                    }

                    // Zoek de maat die moet worden verwijderd uit de lijst met maten van het product
                    var maatTeVerwijderen = product.Maten.FirstOrDefault(m => m.Id == request.MaatId);

                    if (maatTeVerwijderen == null)
                    {
                        return new NotFoundErrorResult($"Maat met ID {request.MaatId} kon niet worden gevonden in het product.");
                    }

                    // Verwijder de maat uit de lijst met maten van het product
                    product.Maten.Remove(maatTeVerwijderen);

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
                    // _logger.LogError(ex, "Er is een fout opgetreden bij het verwijderen van een maat uit een product.");

                    // Returneer een specifiek foutresultaat voor onverwachte fouten
                    return new ErrorResult("Er is een onverwachte fout opgetreden bij het verwijderen van een maat uit een product. Probeer het later opnieuw.");
                }
            }
        }
    }
}