using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCore.Features.Images.Commands
{
    public class DeleteImage
    {
        public record Command:IRequest<Result>
        {
            public Guid Id { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator() { 
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
                var validator = new CommandValidator();
                ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
                try
                {
                    if (!validationResult.IsValid)
                    {
                        return new ValidationErrorResult("Probleem met validatie van je command", validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList());
                    }
                    var result = await _context.ProductFotos.Where(f => f.Id ==  request.Id)
                        .ExecuteDeleteAsync();

                    return new SuccessResult<int>(result);
                }
                catch (DbUpdateException ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een databasefout opgetreden bij het maken van een product.");

                    // Returneer een specifieke foutresultaat voor databasegerelateerde fouten
                    return new ErrorResult("Er is een probleem opgetreden tijdens het verwijderen van de foto. Probeer later opnieuw.");
                }
                catch (Exception ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een fout opgetreden bij het verwijderen van producten.");

                    // Returneer een specifiek foutresultaat voor onverwachte fouten
                    return new ErrorResult("Er is een onverwachte fout opgetreden bij het verwijderen van de foto. Probeer het later opnieuw.");
                }
            }
        }
    }
}
