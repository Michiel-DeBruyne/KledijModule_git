using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.Webshop.Commands
{
    public class EditBestelPeriode
    {
        public record Command : IRequest<Result>
        {
            public Guid Id { get; set; }
            public DateTime OpeningDate { get; set; }
            public DateTime ClosingDate { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            //TODO : wat met overlappende bestelperiodes en reeds bestaande bestelperiodes
            public CommandValidator()
            {
                RuleFor(c => c.Id).NotEmpty();
                RuleFor(c => c.OpeningDate).NotNull()
                    .NotEmpty().WithMessage("Openingsdatum mag niet leeg zijn.")
                    .Must(BeAValidDate).WithMessage("Openingsdatum moet een geldige datum zijn en mag niet in het verleden liggen.");

                RuleFor(c => c.ClosingDate).NotNull()
                    .NotEmpty().WithMessage("Sluitingsdatum mag niet leeg zijn.")
                    .Must(BeAValidDate).WithMessage("Sluitingsdatum moet een geldige datum zijn en mag niet in het verleden liggen.")
                    .GreaterThan(c => c.OpeningDate).WithMessage("Sluitingsdatum moet na de openingsdatum liggen.");
            }

            private static bool BeAValidDate(DateTime date)
            {
                return date > DateTime.Now.Date;
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
                    var bestelperiode = await _context.WebShopConfigurations.FindAsync(request.Id, cancellationToken);
                    if (bestelperiode == null)
                    {
                        return new NotFoundErrorResult("Bestelperiode die u wenst te bewerken kon niet langer gevonden worden. Mogelijks werd het verwijderd?");
                    }
                    request.Adapt(bestelperiode);
                    await _context.SaveChangesAsync();
                    return new SuccessResult();
                }
                catch (DbUpdateException ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een databasefout opgetreden bij het maken van een product.");

                    // Returneer een specifieke foutresultaat voor databasegerelateerde fouten
                    return new ErrorResult("Er is een databasefout opgetreden bij het bewerken van de bestelperiode, contacteer ICT");
                }
                catch (Exception ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een onverwachte fout opgetreden bij het maken van een product.");

                    // Returneer een specifiek foutresultaat voor onverwachte fouten
                    return new ErrorResult("Er is een onverwachte fout opgetreden bij het toevoegen van de bestelperiode. Probeer het later opnieuw.");
                }
            }
        }
    }
}
