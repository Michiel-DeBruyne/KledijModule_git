using FluentValidation;
using FluentValidation.Results;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.Producten.Commands
{
    public class DeactivateProducts
    {
        public record DeactivateProductsCommand : IRequest<Result>
        {
            public List<Guid> Products { get; set; }
        }

        public class CommandValidator : AbstractValidator<DeactivateProductsCommand>
        {
            public CommandValidator()
            {
                RuleFor(c => c.Products).NotEmpty().WithMessage("Er werden geen producten geselecteerd om te deactiveren.");
            }
        }

        public class DeactivateProductsCommandHandler : IRequestHandler<DeactivateProductsCommand, Result>
        {
            private readonly IMapper _mapper;
            private readonly ApplicationDbContext _context;

            public DeactivateProductsCommandHandler(IMapper mapper, ApplicationDbContext context)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result> Handle(DeactivateProductsCommand request, CancellationToken cancellationToken)
            {
                // Validatie van de input, want als de lijst leeg is moet er niets verwijderd worden
                var validator = new CommandValidator();
                ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
                try
                {
                    if (!validationResult.IsValid)
                    {
                        return new ValidationErrorResult("Validatie fout! ", validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList());
                    }
                    var result = await _context.Producten.Where(p => request.Products.Contains(p.Id))
                                .ExecuteUpdateAsync(p => p.SetProperty(product => product.Beschikbaar, p => !p.Beschikbaar));

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
