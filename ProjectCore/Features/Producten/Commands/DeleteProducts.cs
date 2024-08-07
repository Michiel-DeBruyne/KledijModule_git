using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.Producten.Commands
{
    public class DeleteProducts
    {
        public record DeleteProductsCommand : IRequest<Result>
        {
            public List<Guid> Products { get; set; } = new List<Guid>();
        }

        public class CommandValidator : AbstractValidator<DeleteProductsCommand>
        {
            public CommandValidator()
            {
                RuleFor(c => c.Products).NotEmpty().WithMessage("Er werden geen producten geselecteerd om te verwijderen.");
            }
        }

        public class DeleteProductsCommandHandler : IRequestHandler<DeleteProductsCommand, Result>
        {
            private readonly ApplicationDbContext _context;

            public DeleteProductsCommandHandler(ApplicationDbContext context)
            {
                _context = context;
            }
            public async Task<Result> Handle(DeleteProductsCommand request, CancellationToken cancellationToken)
            {
                var validator = new CommandValidator();
                ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
                try
                {
                    if (!validationResult.IsValid)
                    {
                        return new ValidationErrorResult("Probleem met validatie van je command", validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList());
                    }
                    var result = await _context.Producten.Where(p => request.Products.Contains(p.Id))
                        .ExecuteDeleteAsync();

                    return new SuccessResult<int>(result);
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
                    //_logger.LogError(ex, "Er is een fout opgetreden bij het verwijderen van producten.");

                    // Returneer een specifiek foutresultaat voor onverwachte fouten
                    return new ErrorResult("Er is een onverwachte fout opgetreden bij het verwijderen van producten. Probeer het later opnieuw.");
                }
            }
        }
    }
}
