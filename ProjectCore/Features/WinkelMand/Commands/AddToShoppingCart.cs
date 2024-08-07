using FluentValidation;
using FluentValidation.Results;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Domain.Entities.WinkelMand;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.WinkelMand.Commands
{
    public class AddToShoppingCart
    {
        public record Command : IRequest<Result>
        {
            public string UserId { get; set; }
            public ShoppingCartItem Item { get; set; }
            public record ShoppingCartItem
            {
                public Guid ProductId { get; set; }
                public int Hoeveelheid { get; set; }
                public string? Kleur { get; set; }
                public string? Maat { get; set; }
                public string? Opmerkingen { get; set; }
            }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.UserId).NotNull().WithMessage("Er werd geen gebruiker meegegeven voor het product aan de winkelmand toe te voegen");
                RuleFor(c => c.Item.ProductId).NotNull().WithMessage("Er werd geen product doorgegeven, probeer opnieuw");
                RuleFor(c => c.Item.Hoeveelheid).GreaterThanOrEqualTo(1).WithMessage("Hoeveelheid kan niet kleiner zijn dan 1");
            }
        }

        public class CommandHandler : IRequestHandler<Command, Result>
        {
            private readonly ApplicationDbContext _context;
            public CommandHandler(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
            {
                _context = context;
            }

            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    var validator = new CommandValidator();
                    ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);

                    if (!validationResult.IsValid)
                    {
                        return new ValidationErrorResult("Validatie mislukt", validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList());
                    }

                    // Zoek de winkelwagen van de gebruiker
                    var shoppingCart = await _context.ShoppingCarts.Include(sh => sh.ShoppingCartItems)
                        .FirstOrDefaultAsync(sc => sc.UserId == request.UserId);

                    // Als er geen winkelwagen bestaat, maak er dan een aan
                    if (shoppingCart == null)
                    {
                        shoppingCart = new ShoppingCart { UserId = request.UserId };
                        await _context.ShoppingCarts.AddAsync(shoppingCart); // Voeg nieuwe winkelwagen toe want er bestaat nog geen.
                    }
                    ShoppingCartItem shoppingCartItem = new ShoppingCartItem();
                    shoppingCartItem = request.Item.Adapt<ShoppingCartItem>();

                    var product = await _context.Producten.FindAsync(request.Item.ProductId, cancellationToken);
                    if (product == null)
                    {
                        return new NotFoundErrorResult($"Product met Id {request.Item.ProductId} kon niet worden gevonden om toe te voegen aan je winkelmand. Mogelijks het werd verwijderd?");
                    }

                    // Controleer of een identiek item al in de winkelwagen zit
                    var existingItem = shoppingCart.ShoppingCartItems.FirstOrDefault(item =>
                        item.ProductId == shoppingCartItem.ProductId &&
                        item.Maat == shoppingCartItem.Maat &&
                        item.Kleur == shoppingCartItem.Kleur);

                    if (existingItem != null)
                    {
                        // Als een identiek item al bestaat, tel de hoeveelheden op
                        existingItem.Hoeveelheid += shoppingCartItem.Hoeveelheid;
                    }
                    else
                    {
                        // Voeg het nieuwe item toe aan het winkelwagentje
                        shoppingCart.ShoppingCartItems.Add(shoppingCartItem);
                    }

                    await _context.SaveChangesAsync();

                    return new SuccessResult<Guid>(shoppingCartItem.Id);
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
