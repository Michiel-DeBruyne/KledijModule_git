using FluentValidation;
using FluentValidation.Results;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCore.Features.Favorieten
{
    public class UpdateFavorite
    {
        public record Command : IRequest<Result>
        {
            public Guid ProductId { get; set; }
            public string UserId { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.UserId).NotEmpty().NotNull();
                RuleFor(c => c.ProductId).NotEmpty().NotNull();
            }
        }

        public class CommandHandler : IRequestHandler<Command, Result>
        {
            private readonly IMapper _mapper;
            private readonly ApplicationDbContext _context;

            public CommandHandler(IMapper mapper, ApplicationDbContext context)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    var validator = new CommandValidator();
                    ValidationResult validationResult = await validator.ValidateAsync(request);

                    if (!validationResult.IsValid)
                    {
                        return new ValidationErrorResult("Er lijkt wat data te ontbreken bij je verzoek. Je verzoek is niet doorgegaan", validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList());
                    }
                    var FavorietBestaat = await _context.Favorieten.FirstOrDefaultAsync(f => f.ProductId == request.ProductId && f.GebruikerId == request.UserId);

                    if (FavorietBestaat != null)
                    {
                        _context.Favorieten.Remove(FavorietBestaat);
                    }
                    else
                    {
                        var favoriet = new Favoriet
                        {
                            ProductId = request.ProductId,
                            GebruikerId = request.UserId
                        };
                        _context.Favorieten.Add(favoriet);
                    }

                    await _context.SaveChangesAsync();

                    return new SuccessResult();

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
