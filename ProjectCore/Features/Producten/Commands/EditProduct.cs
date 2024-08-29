using FluentValidation;
using FluentValidation.Results;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Shared.Exceptions;
namespace ProjectCore.Features.Producten.Commands
{
    public class EditProduct
    {

        public record EditProductCommand : IRequest<Result>
        {
            public Guid Id { get; set; }
            public string Naam { get; set; } = string.Empty;
            public string? Beschrijving { get; set; }
            public bool Beschikbaar { get; set; } = false;
            public int Punten { get; set; }
            public Geslacht Geslacht { get; set; }
            public int? ArtikelNummer { get; set; }
            public Guid CategorieId { get; set; }
            public int MaxAantalBestelbaar { get; set; }

            public int PerAantalJaar { get; set; }
        }

        #region Validator
        public class Validator : AbstractValidator<EditProductCommand>
        {
            public Validator()
            {
                RuleFor(command => command.Id).NotEmpty().WithMessage("Product-ID mag niet leeg zijn.");

                RuleFor(command => command.Naam).NotEmpty().WithMessage("Naam mag niet leeg zijn.");

                RuleFor(command => command.Beschrijving).MaximumLength(500).WithMessage("Beschrijving mag niet langer zijn dan 500 tekens.");

                RuleFor(command => command.Punten).GreaterThan(0).WithMessage("Punten moet groter zijn dan 0.");

                RuleFor(command => command.ArtikelNummer).GreaterThan(0).When(command => command.ArtikelNummer.HasValue).WithMessage("Artikelnummer moet groter zijn dan 0 als het is opgegeven.");

                RuleFor(command => command.CategorieId).NotEmpty().WithMessage("Categorie-ID mag niet leeg zijn.");

                RuleFor(command => command.MaxAantalBestelbaar).GreaterThan(0).WithMessage("Maximaal aantal bestelbaar moet groter zijn dan 0.");

                RuleFor(command => command.PerAantalJaar).GreaterThan(0).WithMessage("PerAantalJaar moet groter zijn dan 0.");
            }
        }
        #endregion Validator

        public class EditProductCommandHandler : IRequestHandler<EditProductCommand, Result>
        {
            private readonly ApplicationDbContext _context;
            private readonly IMapper _mapper;

            public EditProductCommandHandler(ApplicationDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }
            public async Task<Result> Handle(EditProductCommand request, CancellationToken cancellationToken)
            {
                //TODO: unieke namen voor alle producten en categorieën dus dergelijke check in de validatie steken!
                try
                {
                    var validator = new Validator();
                    ValidationResult validationResult = validator.Validate(request);

                    if (!validationResult.IsValid)
                    {
                        return new ValidationErrorResult("Fout tijdens het valideren van je product info.", validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList());
                    }

                    var product = await _context.Producten.FindAsync(request.Id);

                    if (product == null)
                    {
                        return new NotFoundErrorResult("Product dat u wenst te updaten kon niet gevonden worden, mogelijks werd het verwijderd?");
                    }
                    var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategorieId);
                    if (!categoryExists)
                    {
                        return new NotFoundErrorResult($"De opgegeven categorie met id {request.CategorieId} kon niet worden gevonden.");
                    }
                    // Product werd gevonden en input is gevalideerd. Map het commando naar het effectieve gevonden product.
                    product = request.Adapt(product);

                    // Sla de wijzigingen op.
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
