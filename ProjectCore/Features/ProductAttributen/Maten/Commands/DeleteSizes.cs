using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.ProductAttributen.Maten.Commands
{
    public class DeleteSizes
    {
        public record DeleteCommand : IRequest<Result>
        {
            public List<Guid> MatenToDelete { get; set; } = new List<Guid>();
        }

        public class CommandValidator : AbstractValidator<DeleteCommand>
        {
            public CommandValidator()
            {
                RuleFor(c => c.MatenToDelete).NotEmpty().WithMessage("Er werden geen maten geselecteerd om te verwijderen.");
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
                    var numberOfMatenDeleted = await _context.ProductMaten.Where(pk => request.MatenToDelete.Contains(pk.Id)).ExecuteDeleteAsync();

                    return new SuccessResult<int>(numberOfMatenDeleted);
                }
                catch (DbUpdateException ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een databasefout opgetreden bij het maken van een product.");

                    // Returneer een specifieke foutresultaat voor databasegerelateerde fouten
                    return new ErrorResult("Er is een databasefout opgetreden bij het maken van de maat.");
                }
                catch (Exception ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een onverwachte fout opgetreden bij het maken van een product.");

                    // Returneer een specifiek foutresultaat voor onverwachte fouten
                    return new ErrorResult("Er is een onverwachte fout opgetreden bij het maken van de maat. Probeer het later opnieuw.");
                }
            }
        }
    }
}
