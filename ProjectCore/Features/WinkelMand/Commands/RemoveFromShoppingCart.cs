using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.WinkelMand.Commands
{
    public class RemoveFromShoppingCart
    {
        public record Command : IRequest<Result>
        {
            public Guid ShoppingCartId { get; set; }
            public Guid ShoppingCartItemId { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.ShoppingCartId).NotNull().NotEmpty();
                RuleFor(c => c.ShoppingCartItemId).NotNull().NotEmpty().WithMessage("Er werd geen winkelmand item geselecteerd om te verwijderen.");
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
                        return new ValidationErrorResult("Validatie voor het product uit de winkelmand te verwijderen mislukt", validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList());
                    }
                    await _context.ShoppingCartItems
                        .Where(shi => shi.Id == request.ShoppingCartItemId)
                        .Where(shi => shi.ShoppingCartId == request.ShoppingCartId)
                        .ExecuteDeleteAsync(cancellationToken);

                    return new SuccessResult();
                }
                catch (DbUpdateException ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een databasefout opgetreden bij het maken van een product.");

                    // Returneer een specifieke foutresultaat voor databasegerelateerde fouten
                    return new ErrorResult("Er is een databasefout opgetreden bij het verwijderen van het product uit je winkelmand. Probeer later opnieuw");
                }
                catch (Exception ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een onverwachte fout opgetreden bij het maken van een product.");

                    // Returneer een specifiek foutresultaat voor onverwachte fouten
                    return new ErrorResult("Er is een onverwachte fout opgetreden bij het verwijderen van het product uit je winkelmand. Probeer het later opnieuw.");
                }


            }
        }
    }
}
