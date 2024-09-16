using FluentValidation;
using FluentValidation.Results;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.Producten.Commands
{
    public class CreateProduct
    {
        public record Command : IRequest<Result>
        {
            public string Naam { get; set; } = string.Empty;
            public string? Beschrijving { get; set; }
            public bool Beschikbaar { get; set; } = false;
            public bool BeschikbaarVoorCalog { get; set; } = false;
            public int Punten { get; set; }
            public Geslacht Geslacht { get; set; }
            public int? ArtikelNummer { get; set; }
            public Guid CategorieId { get; set; }
            public int MaxAantalBestelbaar { get; set; }

            public int PerAantalJaar { get; set; }
        }

        #region Validator
        public class Validator : AbstractValidator<Command>
        {
            private readonly ApplicationDbContext _context;
            public Validator(ApplicationDbContext context)
            {
                _context = context;
                RuleFor(command => command.Beschrijving).MaximumLength(500).WithMessage("Beschrijving mag niet langer zijn dan 500 tekens.");

                RuleFor(command => command.Punten).GreaterThan(0).WithMessage("Punten moet groter zijn dan 0.");

                RuleFor(command => command.ArtikelNummer).GreaterThan(0).When(command => command.ArtikelNummer.HasValue).WithMessage("Artikelnummer moet groter zijn dan 0 als het is opgegeven.");

                RuleFor(command => command.CategorieId).NotEmpty().WithMessage("Categorie-ID mag niet leeg zijn.");

                RuleFor(command => command.MaxAantalBestelbaar).GreaterThan(0).WithMessage("Maximaal aantal bestelbaar moet groter zijn dan 0.");

                RuleFor(command => command.PerAantalJaar).GreaterThan(0).WithMessage("PerAantalJaar moet groter zijn dan 0.");
                RuleFor(e => e)
                        .MustAsync(NameUnique)
                        .WithMessage("Een Product met dezelfde naam bestaat al.");

            }

            private async Task<bool> NameUnique(Command e, CancellationToken token)
            {
                return !(await IsNameUnique(e.Naam));
            }
            public Task<bool> IsNameUnique(string Naam)
            {
                var matches = _context.Producten.Any(e => e.Naam.Equals(Naam));
                return Task.FromResult(matches);
            }
        }
        #endregion Validator

        public class CommandHandler : IRequestHandler<Command, Result>
        {
            private readonly IMapper _mapper;
            private readonly ApplicationDbContext _context;

            public CommandHandler(IMapper mapper, ApplicationDbContext context)
            {
                _mapper = mapper;
                _context = context;
            }
            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    var validator = new Validator(_context);
                    ValidationResult validationResult = await validator.ValidateAsync(request);

                    if (!validationResult.IsValid)
                    {
                        return new ValidationErrorResult("Validatie mislukt tijdens het aanmaken van je product...", validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList());
                    }

                    var categorie = await _context.Categories.FindAsync(request.CategorieId, cancellationToken);
                    if (categorie == null)
                    {
                        return new NotFoundErrorResult($"Categorie met categorieID {request.CategorieId} kon niet worden gevonden. Mogelijks bestaat het niet langer");
                    }
                    var product = _mapper.Map<Product>(request);
                    // add standaard kleur en maat
                    var standaardKleur = await _context.ProductKleuren
                        .Where(k => k.Kleur.Equals("Standaard"))
                        .FirstOrDefaultAsync();
                    if (standaardKleur == null) { return new NotFoundErrorResult("Standaard als kleur kon niet gevonden worden. Product wordt niet toegevoegd."); }
                    product.Kleuren.Add(standaardKleur);
                    var standaardMaat = await _context.ProductMaten
                                            .Where(m => m.Maat.Equals("Standaard"))
                                            .FirstOrDefaultAsync();
                    if (standaardMaat == null) { return new NotFoundErrorResult("Standaard als Maat kon niet gevonden worden. Product wordt niet toegevoegd."); }

                    product.Maten.Add(standaardMaat);

                    await _context.Producten.AddAsync(product);
                    await _context.SaveChangesAsync();
                    return new SuccessResult<Guid>(product.Id);
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
