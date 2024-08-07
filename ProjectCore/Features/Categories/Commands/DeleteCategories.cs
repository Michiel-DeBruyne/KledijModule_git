using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.Categories.Commands
{
    // implement multi delete if possible. but first basic stuff
    public class DeleteCategories
    {
        public record DeleteCommand : IRequest<Result>
        {
            public List<Guid> Categories { get; set; }
        }

        public class CommandValidator : AbstractValidator<DeleteCommand>
        {
            public CommandValidator()
            {
                RuleFor(c => c.Categories).NotEmpty().WithMessage("Er werden geen categorieën geselecteerd om te verwijderen");
            }
        }

        public class CommandHandler : IRequestHandler<DeleteCommand, Result>
        {
            private readonly ApplicationDbContext _context;

            public CommandHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Result> Handle(DeleteCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    #region ValidateCommand

                    var validator = new CommandValidator();
                    ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
                    if (!validationResult.IsValid)
                    {
                        return new ValidationErrorResult("Validatie mislukt", validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList());
                    }

                    #endregion ValidateCommand

                    #region checkIfHasSubcategories
                    var categoriesWithSubcategories = await _context.Categories
                        .Where(c => request.Categories.Contains(c.Id) && c.SubCategorieën.Any())
                        .Select(c => c.Naam)
                        .ToListAsync();

                    if (categoriesWithSubcategories.Any())
                    {
                        return new ErrorResult($"De categorieën '{string.Join("' , '", categoriesWithSubcategories)}' kunnen niet worden verwijderd omdat ze subcategorieën hebben. Gelieve deze eerst los te koppelen.");
                    }
                    #endregion checkIfHasSubcategories

                    #region checkifHasProducts

                    var categoriesWithProducts = await _context.Categories
                        .Where( c => request.Categories.Contains(c.Id) && c.Products.Any())
                        .Select(c => c.Naam)
                        .ToListAsync();

                    if (categoriesWithProducts.Any())
                    {
                        return new ErrorResult($"De categorieën '{string.Join("' , '", categoriesWithProducts)}' kunnen niet worden verwijderd omdat er nog producten aan gekoppeld zijn. Gelieve deze eerst los te koppelen.");
                    }

                    #endregion checkifHasProducts

                    var categoriesToDelete = await _context.Categories
                        .Where(c => request.Categories.Contains(c.Id))
                        .ToListAsync();

                    _context.Categories.RemoveRange(categoriesToDelete);

                    var result = await _context.SaveChangesAsync();

                    return new SuccessResult<int>(result);
                }
                catch (DbUpdateException ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een databasefout opgetreden bij het maken van een product.");

                    // Returneer een specifieke foutresultaat voor databasegerelateerde fouten
                    return new ErrorResult("Er is een databasefout opgetreden bij het verwijderen van de categorie.");
                }
                catch (Exception ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een fout opgetreden bij het verwijderen van producten.");

                    // Returneer een specifiek foutresultaat voor onverwachte fouten
                    return new ErrorResult("Er is een onverwachte fout opgetreden bij het verwijderen van categorieën. Probeer het later opnieuw.");
                }

            }
        }
    }
}
