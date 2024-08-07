using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.Producten.Commands
{
    public class MapColorToProduct
    {
        public record Command : IRequest<Result>
        {
            public List<Guid> Kleuren { get; set; } = new List<Guid>();
            public Guid Product { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            private readonly ApplicationDbContext _context;

            public CommandValidator(ApplicationDbContext context)
            {
                _context = context;

                RuleFor(c => c.Product).NotEmpty();
                RuleFor(c => c.Kleuren).NotNull().NotEmpty().WithMessage("Er werden geen kleuren geselecteerd om aan het product te koppelen");

                // Aanvullende validatie om te controleren of de gekozen kleuren bestaan in de database
                RuleForEach(c => c.Kleuren)
                    .MustAsync(async (kleurId, cancellationToken) =>
                    {
                        return await _context.ProductKleuren.AnyAsync(k => k.Id == kleurId, cancellationToken);
                    }).WithMessage("Een of meer geselecteerde kleuren bestaan niet.");
            }
        }

        public class MapColorToProductCommandHandler : IRequestHandler<Command, Result>
        {
            private readonly ApplicationDbContext _context;

            public MapColorToProductCommandHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                var validator = new CommandValidator(_context);
                ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
                try
                {
                    if (!validationResult.IsValid)
                    {
                        return new ValidationErrorResult("Validatie mislukt", validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList());
                    }
                    var product = _context.Producten.Find(request.Product);

                    if (product == null)
                    {
                        return new NotFoundErrorResult($"Het product met id {request.Product} waarvoor u kleuren wenst toe te voegen, kon niet worden gevonden. Mogelijks werd het verwijderd?");
                    }

                    var gekozenKleuren = await _context.ProductKleuren
                                        .Where(k => request.Kleuren.Contains(k.Id))
                                        .ToListAsync();


                    product.Kleuren.AddRange(gekozenKleuren);

                    // Save changes to persist the association to the database
                    _context.SaveChanges();
                    return new SuccessResult<Unit>(Unit.Value);
                }
                catch (DbUpdateException ex)
                {
                    // Handle specific EF Core database update exceptions
                    // such as constraint violations, concurrency conflicts, etc.
                    return new ErrorResult("Er is een fout opgetreden tijdens het opslaan van de gekoppelde kleuren aan het product.");
                }
                catch (Exception ex)
                {
                    // Handle other unexpected exceptions
                    return new ErrorResult("Er is een onverwachte fout opgetreden.");
                }
            }
        }

    }
}
