using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.Webshop.Commands
{
    public class DeleteBestelPeriode
    {
        public record Command : IRequest<Result>
        {
            public Guid Id { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.Id).NotEmpty();
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
                    var openingsUur = await _context.WebShopConfigurations.FindAsync(request.Id, cancellationToken);
                    if (openingsUur == null)
                    {
                        return new NotFoundErrorResult("De openingsperiode dat u geselecteerd heeft om te verwijderen, kon niet worden gevonden. Mogelijks is deze al verwijderd?");
                    }
                    _context.WebShopConfigurations.Remove(openingsUur);
                    await _context.SaveChangesAsync();
                    return new SuccessResult();
                }
                catch (DbUpdateException ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een databasefout opgetreden bij het maken van een product.");

                    // Returneer een specifieke foutresultaat voor databasegerelateerde fouten
                    return new ErrorResult("Er is een databasefout opgetreden bij het verwijderen van de bestelperiode");
                }
                catch (Exception ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een onverwachte fout opgetreden bij het maken van een product.");

                    // Returneer een specifiek foutresultaat voor onverwachte fouten
                    return new ErrorResult("Er is een onverwachte fout opgetreden bij het verwijderen van de bestelperiode. Probeer het later opnieuw.");
                }

            }
        }
    }
}
