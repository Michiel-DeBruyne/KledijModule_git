using FluentValidation;
using FluentValidation.Results;
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
                try
                {
                    var validator = new CommandValidator();
                    ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);

                    if (!validationResult.IsValid)
                    {
                        return new ValidationErrorResult("Validatie mislukt bij het updaten van de gebruiker zijn balans!", validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList());
                    }
                    bool gebruikerBestaat = await _context.Gebruikers
                                            .AnyAsync(gebr => gebr.Id == request.Id, cancellationToken);

                    if (!gebruikerBestaat)
                    {
                        return new NotFoundErrorResult("Geen gebruiker gevonden met het opgegeven ID.");
                    }
                    var updatebalanceResult = await _context.Gebruikers.Where(gebr => gebr.Id == request.Id).ExecuteUpdateAsync(gebr => gebr.SetProperty(g => g.Balans,request.Balans));
                    
                    return new SuccessResult();
                }
                catch (DbUpdateException ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een databasefout opgetreden bij het maken van een product.");

                    // Returneer een specifieke foutresultaat voor databasegerelateerde fouten
                    return new ErrorResult("Er is een databasefout opgetreden bij het updaten van de gebruiker zijn balans! Contacteer ICT.");
                }
                catch (Exception ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een onverwachte fout opgetreden bij het maken van een product.");

                    // Returneer een specifiek foutresultaat voor onverwachte fouten
                    return new ErrorResult("Er is een onverwachte fout opgetreden bij het updaten van de gebruiker zijn balans! Probeer het later opnieuw en ontacteer ICT.");
                }
            }
        }
    }
}
