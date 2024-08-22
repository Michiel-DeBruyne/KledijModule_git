using FluentValidation;
using FluentValidation.Results;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Domain.Entities.Bestellingen;
using ProjectCore.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCore.Features.Gebruikers.Commands
{
    public class UpdateUserBalance
    {
        public record Command:IRequest<Result>
        {
            public string Id { get;set; }
            public int Balans { get;set; }
        }

        public class CommandValidator:AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.Id).NotEmpty().NotNull();
                RuleFor(c => c.Balans).NotEmpty().NotNull().GreaterThan(0);
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
                //return new NotFoundErrorResult("Geen gebruiker gevonden met het opgegeven ID"); // test om fout te simuleren.
                try
                {
                    // Valideer de input via de validator
                    var validator = new CommandValidator();
                    ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);

                    if (!validationResult.IsValid)
                    {
                        return new ValidationErrorResult("Validatie mislukt bij het updaten van de gebruiker zijn balans!",
                            validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList());
                    }

                    // Start een database-transactie
                    using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
                    {
                        try
                        {
                            // Haal de gebruiker op die je wilt updaten
                            var gebruiker = await _context.Gebruikers
                                .FirstOrDefaultAsync(gebr => gebr.Id == request.Id, cancellationToken);

                            if (gebruiker == null)
                            {
                                return new NotFoundErrorResult("Geen gebruiker gevonden met het opgegeven ID.");
                            }

                            // Update de balans van de gebruiker
                            gebruiker.Balans = request.Balans;

                            // Sla de wijzigingen op in de database
                            await _context.SaveChangesAsync(cancellationToken);

                            // Commit de transactie
                            await transaction.CommitAsync(cancellationToken);

                            // Return succes als alles goed ging
                            return new SuccessResult<GebruikerVm>(gebruiker.Adapt<GebruikerVm>());  // Optioneel: stuur de geüpdatete gebruiker terug
                        }
                        catch (DbUpdateException dbEx)
                        {
                            // Rol de transactie terug bij een databasefout
                            await transaction.RollbackAsync(cancellationToken);

                            // Log de databasefout
                          //  _logger.LogError(dbEx, "Er is een databasefout opgetreden bij het updaten van de gebruiker zijn balans met ID {Id}.", request.Id);

                            // Return een foutbericht
                            return new ErrorResult("Er is een databasefout opgetreden bij het updaten van de gebruiker zijn balans! Contacteer ICT.");
                        }
                        catch (Exception ex)
                        {
                            // Rol de transactie terug bij een onverwachte fout
                            await transaction.RollbackAsync(cancellationToken);

                            // Log de onverwachte fout
                          //  _logger.LogError(ex, "Er is een onverwachte fout opgetreden bij het updaten van de gebruiker zijn balans met ID {Id}.", request.Id);

                            // Return een foutbericht
                            return new ErrorResult("Er is een onverwachte fout opgetreden bij het updaten van de gebruiker zijn balans! Probeer het later opnieuw en contacteer ICT.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Dit vangt uitzonderingen op die buiten de transacties kunnen plaatsvinden
                   // _logger.LogError(ex, "Er is een onverwachte fout opgetreden buiten de transactie bij het updaten van de gebruiker zijn balans met ID {Id}.", request.Id);

                    // Return een foutbericht
                    return new ErrorResult("Er is een onverwachte fout opgetreden bij het verwerken van de balans update! Probeer het later opnieuw.");
                }

            }


        }
        public class GebruikerVm
        {
            public string Id { get; set; }
            public string VoorNaam { get; set; } = string.Empty;
            public string AchterNaam { get; set; } = string.Empty;

            public string FullName => VoorNaam + " " + AchterNaam;
            public string Email { get; set; }
            public int Balans { get; set; } = 0;
        }
    }
}
