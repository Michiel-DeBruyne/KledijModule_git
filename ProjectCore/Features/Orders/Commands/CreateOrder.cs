using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Domain.Entities.Bestellingen;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Shared.Exceptions;


namespace ProjectCore.Features.Orders.Commands
{
    public class CreateOrder
    {
        public record Command : IRequest<Result>
        {
            public string UserId { get; set; }
            public string UserNaam { get; set; }
            public List<OrderItem> OrderItems { get; set; }
            public int TotaalPrijs { get; set; }
            public record OrderItem
            {
                public string ProductNaam { get; set; }
                public string? Maat { get; set; }
                public string? Kleur { get; set; }
                public Guid ProductId { get; set; }
                public Product Product { get; set; }
                public int Prijs { get; set; }

                public int Hoeveelheid { get; set; }
                public OrderStatus OrderStatus { get; set; } = OrderStatus.Open;

                public string? Opmerkingen { get; set; }
            }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            private readonly ApplicationDbContext _context;
            public CommandValidator(ApplicationDbContext context)
            {
                _context = context;
                RuleFor(c => c.UserId).NotEmpty().NotNull();
                RuleFor(c => c.OrderItems).NotEmpty().NotNull();
                //  RuleForEach(c => c.OrderItems).Must((c, orderItem) => AllowedToAdd(orderItem, c.UserId)).WithMessage((c, orderItem) => $"{orderItem.ProductNaam} kan niet worden besteld, vervangingstermijn nog niet overschreden.");
                RuleForEach(c => c.OrderItems).Custom((item, context) =>
                {
                    var command = context.InstanceToValidate as Command;
                    if (command != null)
                    {
                        if (item.Hoeveelheid > item.Product.MaxAantalBestelbaar)
                        {
                            context.AddFailure($"Het aantal dat u ingegeven heeft voor '{item.ProductNaam}', is groter dan het maximum aantal dat mag besteld worden voor dit artikel. Er {(item.Product.MaxAantalBestelbaar > 1 ? "mogen" : "mag")} maximaal {item.Product.MaxAantalBestelbaar} item(s) besteld worden.");
                        }
                        else if (!AllowedToAdd(item, command.UserId, out int remainingAmountOrderable))
                        {
                            context.AddFailure($"{item.ProductNaam} kan niet worden besteld, vervangingstermijn nog niet overschreden. U kunt nog {remainingAmountOrderable} stuk(s) bestellen.");
                        }
                    }
                });
            }

            private bool AllowedToAdd(Command.OrderItem item, string UserId, out int remainingAmountOrderable)
            {
                remainingAmountOrderable = 0;
                var totalOrderedQuantity = _context.Orders
                            .Where(o => o.UserId == UserId)
                            .Where(o => o.OrderItems.Any(oi => oi.ProductId == item.ProductId))
                            .Where(o => o.CreatedDate >= DateTime.Now.AddYears(-item.Product.PerAantalJaar))
                            .SelectMany(o => o.OrderItems)
                            .Where(oi => oi.ProductId == item.Product.Id && oi.OrderStatus != OrderStatus.Geannuleerd && oi.OrderStatus != OrderStatus.Retour)
                            .Sum(oi => oi.Hoeveelheid);
                remainingAmountOrderable = item.Product.MaxAantalBestelbaar - totalOrderedQuantity;
                // Check if the total ordered quantity plus the quantity being ordered exceeds the maximum allowed quantity
                if (totalOrderedQuantity + item.Hoeveelheid > item.Product.MaxAantalBestelbaar)
                {
                    return false;
                }

                return true;
            }
        }

        public class CommandHandler : IRequestHandler<Command, Result>
        {
            private readonly ApplicationDbContext _context;

            public CommandHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    var validator = new CommandValidator(_context);
                    ValidationResult validationResult = await validator.ValidateAsync(request);

                    if (!validationResult.IsValid)
                    {
                        return new ValidationErrorResult("Fout bij het valideren van je bestelling...", validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList());
                    }
                    //Geen mapster gebruikt omdat addasync anders ook een nieuw product zal proberen maken, nadeel van navigation properties :/
                    var order = new Order
                    {
                        UserId = request.UserId,
                        UserNaam = request.UserNaam,
                        TotaalPrijs = request.TotaalPrijs,
                        OrderItems = request.OrderItems.Select(item => new OrderItem
                        {
                            ProductNaam = item.ProductNaam,
                            Maat = item.Maat,
                            Kleur = item.Kleur,
                            ProductId = item.ProductId,
                            Prijs = item.Prijs,
                            Hoeveelheid = item.Hoeveelheid,
                            OrderStatus = item.OrderStatus,
                            Opmerkingen = item.Opmerkingen
                        }).ToList()
                    };
                    await _context.Orders.AddAsync(order);
                    await _context.SaveChangesAsync(cancellationToken);
                    await _context.ShoppingCarts.Where(sh => sh.UserId == request.UserId).ExecuteDeleteAsync(cancellationToken);
                    return new SuccessResult<Guid>(order.Id);
                }
                catch (DbUpdateException ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een databasefout opgetreden bij het maken van een product.");

                    // Returneer een specifieke foutresultaat voor databasegerelateerde fouten
                    return new ErrorResult("Er is een databasefout opgetreden bij het maken van uw bestelling.");
                }
                catch (Exception ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een onverwachte fout opgetreden bij het maken van een product.");

                    // Returneer een specifiek foutresultaat voor onverwachte fouten
                    return new ErrorResult("Er is een onverwachte fout opgetreden bij het maken van de bestelling. Probeer het later opnieuw.");
                }
            }
        }
    }
}
