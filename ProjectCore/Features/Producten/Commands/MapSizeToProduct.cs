using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.Producten.Commands
{
    public class MapSizeToProduct
    {
        public record Command : IRequest<Result>
        {
            /// <summary>
            /// Het ID van het product waaraan de maten moeten worden toegevoegd.
            /// </summary>
            public Guid Product { get; set; }

            /// <summary>
            /// Een lijst van ID's van maten die aan het product moeten worden toegevoegd.
            /// </summary>
            public List<Guid> Maten { get; set; } = new List<Guid>();
        }

        /// <summary>
        /// Validator voor het commando om maten aan een product toe te voegen.
        /// </summary>
        public class CommandValidator : AbstractValidator<Command>
        {
            private readonly ApplicationDbContext _context;

            /// <summary>
            /// Initialiseert een nieuwe instantie van <see cref="CommandValidator"/>.
            /// </summary>
            public CommandValidator(ApplicationDbContext context)
            {

                _context = context;

                RuleFor(c => c.Product).NotEmpty().WithMessage("ProductId mag niet leeg zijn.");
                RuleFor(c => c.Maten).NotNull().NotEmpty().WithMessage("Er werden geen maten geselecteerd om aan het product te koppelen");

                // Aanvullende validatie om te controleren of de gekozen kleuren bestaan in de database
                RuleForEach(c => c.Maten)
                    .MustAsync(async (MaatId, cancellationToken) =>
                    {
                        return await _context.ProductMaten.AnyAsync(k => k.Id == MaatId, cancellationToken);
                    }).WithMessage("Een of meer geselecteerde kleuren bestaan niet.");
            }
        }

        /// <summary>
        /// Handler voor het toevoegen van maten aan een product.
        /// </summary>
        public class MapSizeToProductCommandHandler : IRequestHandler<Command, Result>
        {
            private readonly ApplicationDbContext _context;

            /// <summary>
            /// Initialiseert een nieuwe instantie van <see cref="MapSizeToProductCommandHandler"/>.
            /// </summary>
            /// <param name="context">De databasecontext.</param>
            public MapSizeToProductCommandHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            /// <summary>
            /// Handelt het toevoegen van maten aan een product af.
            /// </summary>
            /// <param name="request">Het commando om maten aan een product toe te voegen.</param>
            /// <param name="cancellationToken">Het annuleringsverzoek.</param>
            /// <returns>Unit.</returns>
            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                var validator = new CommandValidator(_context);
                var validationResult = await validator.ValidateAsync(request, cancellationToken);

                try
                {
                    if (!validationResult.IsValid)
                    {
                        return new ValidationErrorResult("Validatie mislukt", validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList());
                    }
                    // Zoek het product op basis van het opgegeven ID.
                    var product = await _context.Producten.FindAsync(request.Product);

                    // Controleer of het product bestaat.
                    if (product == null)
                    {
                        return new NotFoundErrorResult($"Het product met ID {request.Product} kon niet worden gevonden. Mogelijks werd het verwijderd?");
                    }

                    // Zoek de maten die moeten worden toegevoegd aan het product.
                    var toeTeVoegenMaten = await _context.ProductMaten
                        .Where(m => request.Maten.Contains(m.Id))
                        .ToListAsync();

                    // Voeg de maten toe aan het product.
                    product.Maten.AddRange(toeTeVoegenMaten);

                    // Sla wijzigingen op in de database.
                    await _context.SaveChangesAsync();

                    return new SuccessResult<Unit>(Unit.Value);
                }
                catch (DbUpdateException ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een databasefout opgetreden bij het toevoegen van maten aan een product.");

                    // Returneer een specifiek foutresultaat voor databasegerelateerde fouten
                    return new ErrorResult("Er is een databasefout opgetreden bij het toevoegen van maten aan een product.");
                }
                catch (Exception ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een onverwachte fout opgetreden bij het toevoegen van maten aan een product.");

                    // Returneer een specifiek foutresultaat voor onverwachte fouten
                    return new ErrorResult("Er is een onverwachte fout opgetreden bij het toevoegen van maten aan een product. Probeer het later opnieuw.");
                }
            }
        }
    }
}
