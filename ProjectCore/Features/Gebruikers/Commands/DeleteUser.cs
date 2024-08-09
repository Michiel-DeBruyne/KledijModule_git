using FluentValidation;
using FluentValidation.Results;
using MediatR;
using MediatR.Wrappers;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCore.Features.Gebruikers.Commands
{
    public class DeleteUser
    {
        public record Command:IRequest<Result>
        {
            public string Id { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Id).NotEmpty();
            }
        }

        public class CommandHandler : IRequestHandler<Command, Result>
        {
            private readonly ApplicationDbContext _context;
            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    var validator = new CommandValidator();
                    ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);

                    if (!validationResult.IsValid)
                    {
                        return new ValidationErrorResult("Validatie mislukt voor het verwijderen van de gebruiker", validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList());
                    }
                    var user = await _context.Gebruikers.FindAsync(request.Id);
                    if(user == null)
                    {
                        return new NotFoundErrorResult($"Gebruiker met Id {request.Id} kon niet worden gevonden. Mogelijks werd deze al verwijderd?");
                    }
                    var updatebalanceResult = await _context.Gebruikers.Where(gebr => gebr.Id == request.Id).ExecuteDeleteAsync();

                    return new SuccessResult();
                }
                catch (DbUpdateException ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een databasefout opgetreden bij het maken van een product.");

                    // Returneer een specifieke foutresultaat voor databasegerelateerde fouten
                    return new ErrorResult("Er is een databasefout opgetreden bij het verwijderen van de gebruiker! Contacteer ICT.");
                }
                catch (Exception ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een onverwachte fout opgetreden bij het maken van een product.");

                    // Returneer een specifiek foutresultaat voor onverwachte fouten
                    return new ErrorResult("Er is een onverwachte fout opgetreden bij het verwijderen van de gebruiker! Probeer het later opnieuw en ontacteer ICT.");
                }
            }
        }
    }
}
