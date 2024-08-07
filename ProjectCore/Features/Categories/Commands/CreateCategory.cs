using FluentValidation;
using FluentValidation.Results;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.Categories.Commands
{
    public class CreateCategory
    {
        public record CreateCategoryCommand : IRequest<Result>
        {
            public string Naam { get; set; } = string.Empty;
            public string? Beschrijving { get; set; }
            public Guid? ParentCategorieId { get; set; }
        }

        public class Validator : AbstractValidator<CreateCategoryCommand>
        {
            private readonly ApplicationDbContext _context;
            public Validator(ApplicationDbContext context)
            {
                _context = context;
                RuleFor(c => c.Naam).NotNull().NotEmpty().WithMessage("Naam mag niet leeg zijn");
                RuleFor(e => e)
                        .MustAsync(NameUnique)
                        .WithMessage("Een categorie met dezelfde naam bestaat al.");
            }

            private async Task<bool> NameUnique(CreateCategoryCommand e, CancellationToken token)
            {
                return !(await IsNameUnique(e.Naam));
            }
            public Task<bool> IsNameUnique(string Naam)
            {
                var matches = _context.Categories.Any(e => e.Naam.Equals(Naam));
                return Task.FromResult(matches);
            }
        }

        public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result>
        {
            private readonly ApplicationDbContext _context;
            private readonly IMapper _mapper;
            public CreateCategoryCommandHandler(ApplicationDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }
            public async Task<Result> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var validator = new Validator(_context);
                    ValidationResult validationResult = await validator.ValidateAsync(request);

                    if (!validationResult.IsValid)
                    {
                        return new ValidationErrorResult("Validatie mislukt.", validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList());
                    }

                    var category = _mapper.Map<Categorie>(request);
                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync(cancellationToken);

                    return new SuccessResult<Guid>(category.Id);
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
