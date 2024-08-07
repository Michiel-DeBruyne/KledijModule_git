using FluentValidation;
using FluentValidation.Results;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.Categories.Commands
{
    public class EditCategory
    {

        public record EditCategoryCommand : IRequest<Result>
        {
            public Guid Id { get; set; }
            public string Naam { get; set; } = string.Empty;
            public string? Beschrijving { get; set; }
            public Guid? ParentCategorieId { get; set; }
        }

        public class Validator : AbstractValidator<EditCategoryCommand>
        {
            public Validator()
            {
                RuleFor(c => c.Id).NotEmpty();
                RuleFor(c => c.Naam).NotNull().NotEmpty().WithMessage("Naam mag niet leeg zijn");
            }
        }

        public class EditCategoryCommandHandler : IRequestHandler<EditCategoryCommand, Result>
        {
            private readonly ApplicationDbContext _context;
            public EditCategoryCommandHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Result> Handle(EditCategoryCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var validator = new Validator();
                    ValidationResult validationResult = validator.Validate(request);
                    if (!validationResult.IsValid)
                    {
                        return new ValidationErrorResult("Validatie mislukt!", validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList());
                    }
                    // misschien findbyId nz in een gedeelde repository steken.
                    var categorieToUpdate = await _context.Categories.FindAsync(request.Id);
                    if (categorieToUpdate == null)
                    {
                        return new NotFoundErrorResult("Categorie die u wenst te updaten kon niet worden gevonden, mogelijks werd het verwijderd?");
                    }
                    request.Adapt(categorieToUpdate);
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
                    //_logger.LogError(ex, "Er is een onverwachte fout opgetreden bij het maken van een product.");

                    // Returneer een specifiek foutresultaat voor onverwachte fouten
                    return new ErrorResult("Er is een onverwachte fout opgetreden bij het maken van het product. Probeer het later opnieuw.");
                }
            }
        }
    }
}
