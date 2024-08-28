using FluentValidation;
using FluentValidation.Results;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Domain.Entities.Bestellingen;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.OrderItems.Commands
{
    public class UpdateOrderItems
    {

        public record Command : IRequest<Result>
        {
            public List<Guid> OrderItems { get; set; } = new List<Guid>();
            public int OrderStatus { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.OrderItems).NotEmpty().NotNull();
                RuleFor(c => c.OrderStatus).NotEmpty().NotNull();
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
                    // Valideer de aanvraag van de gebruiker. Als er iets ontbreekt, stop het gewoon en wordt er feedback gegeven aan de gebruiker.
                    var validator = new CommandValidator();
                    ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);

                    if (!validationResult.IsValid)
                    {
                        return new ValidationErrorResult("Er ging iets mis bij het valideren van je aanvraag om de status van bestelitems aan te passen", validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList());
                    }

                    // converteer de verkregen status naar een domein gekende status voor verder gebruik.
                    var orderStatus = request.OrderStatus.Adapt<OrderStatus>();


                    // Probeer de status van de orderitems bij te werken
                    var updatedItemsCount = await _context.OrderItems
                        .Where(oi => request.OrderItems.Contains(oi.Id))
                        .ExecuteUpdateAsync(o => o.SetProperty(oi => oi.OrderStatus, orderStatus), cancellationToken);

                    if (updatedItemsCount == 0)
                    {
                        return new NotFoundErrorResult("Geen bestelitems werden gevonden om te updaten.");
                    }

                    // Vind alle orderitems waarvan de status daadwerkelijk succesvol is bijgewerkt
                    var succesvolBijgewerkteItems = await _context.Orders
                        .Include(o => o.OrderItems)
                        .Where(o => o.OrderItems.Any(oi => request.OrderItems.Contains(oi.Id) && oi.OrderStatus == orderStatus))
                        .ToListAsync(cancellationToken);

                    // Verwerking van terugbetaling voor geannuleerde of geretourneerde items
                    if (orderStatus == OrderStatus.Geannuleerd || orderStatus == OrderStatus.Retour)
                    {
                        // Groepeer de succesvolle OrderItems op basis van GebruikerId en bereken de totale terugbetaling per gebruiker
                        var refundsPerUser = succesvolBijgewerkteItems
                            .GroupBy(o => o.UserId)
                            .Select(group => new
                            {
                                GebruikerId = group.Key,
                                TotalRefund = group.SelectMany(o => o.OrderItems)
                                                   .Where(oi => request.OrderItems.Contains(oi.Id))
                                                   .Sum(oi => oi.Hoeveelheid * oi.Prijs)
                            })
                            .ToList();

                        // Haal de gebruikers op waarvan de balans moet worden bijgewerkt
                        var gebruikerIds = refundsPerUser.Select(r => r.GebruikerId).ToList();
                        var gebruikers = await _context.Gebruikers
                            .Where(g => gebruikerIds.Contains(g.Id))
                            .ToListAsync(cancellationToken);

                        // Werk het saldo van elke gebruiker bij
                        foreach (var refund in refundsPerUser)
                        {
                            var gebruiker = gebruikers.FirstOrDefault(g => g.Id == refund.GebruikerId);
                            if (gebruiker != null)
                            {
                                gebruiker.Balans += refund.TotalRefund;
                            }
                        }

                        // Sla de bijgewerkte saldo's op
                        await _context.SaveChangesAsync(cancellationToken);
                    }

                    // Retourneer een succesresultaat met het aantal succesvol bijgewerkte items
                    return new SuccessResult<string>($"{succesvolBijgewerkteItems.Count} bestelitems zijn succesvol geüpdatet.");

                    ///// Als de gebruiker het item annuleert, moet de gebruiker zijn 'geld' terugkrijgen.
                    //if (orderStatus == OrderStatus.Geannuleerd || orderStatus ==  OrderStatus.Retour)
                    //{
                    //    var gebruiker = await _context.Gebruikers.Where(gebr => gebr.Id == request.UserId).FirstOrDefaultAsync();
                    //    if(gebruiker == null)
                    //    {
                    //        return new NotFoundErrorResult("Gebruiker waarvan het item moet geannuleerd worden kan niet teruggevonden worden");
                    //    }
                    //    gebruiker.Balans += (orderItem.Hoeveelheid * orderItem.Prijs); // Tell de prijs van het item terug bij de balans van de gebruiker

                    //}
                    //await _context.SaveChangesAsync();

                    //return new SuccessResult();
                }
                catch (DbUpdateException ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een databasefout opgetreden bij het maken van een product.");

                    // Returneer een specifieke foutresultaat voor databasegerelateerde fouten
                    return new ErrorResult("Er is een databasefout opgetreden bij updaten van de status voor het bestelitem");
                }
                catch (Exception ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een onverwachte fout opgetreden bij het maken van een product.");

                    // Returneer een specifiek foutresultaat voor onverwachte fouten
                    return new ErrorResult("Er is een onverwachte fout opgetreden bij het updaten van het bestelitem. Probeer het later opnieuw.");
                }
            }
        }
    }
}
