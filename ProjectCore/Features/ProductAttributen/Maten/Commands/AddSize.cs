using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.ProductAttributen.Maten.Commands
{
    public class AddSize
    {
        public record AddSizeCommand : IRequest<Result>
        {
            public string Maat { get; set; }
        }

        //Nog niet gebruikt
        public class AddProductValidator : AbstractValidator<AddSizeCommand>
        {
            private readonly ApplicationDbContext _context;

            public AddProductValidator(ApplicationDbContext context)
            {
                this._context = context;
                RuleFor(model => model.Maat).NotEmpty().WithMessage("Maat kan geen lege waarde zijn");
                RuleFor(e => e)
                        .MustAsync(NameUnique)
                        .WithMessage("Een maat met dezelfde naam bestaat al.");

            }

            private async Task<bool> NameUnique(AddSizeCommand e, CancellationToken token)
            {
                return !(await IsNameUnique(e.Maat));
            }
            public Task<bool> IsNameUnique(string Naam)
            {
                var matches = _context.ProductMaten.Any(e => e.Maat.Equals(Naam));
                return Task.FromResult(matches);
            }
        }
        public class CommandHandler : IRequestHandler<AddSizeCommand, Result>
        {
            private readonly ApplicationDbContext _context;
            public CommandHandler(ApplicationDbContext ctx)
            {
                _context = ctx;
            }
            public async Task<Result> Handle(AddSizeCommand request, CancellationToken cancellationToken)
            {

                try
                {
                    var validator = new AddProductValidator(_context);
                    var validationResult = await validator.ValidateAsync(request, cancellationToken);
                    if (!validationResult.IsValid)
                    {
                        return new ValidationErrorResult("Validatie van je maat mislukt.", validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList());
                    }
                    var @maat = request.Adapt<ProductMaat>();
                    await _context.ProductMaten.AddAsync(@maat, cancellationToken);
                    await _context.SaveChangesAsync();
                    return new SuccessResult<Guid>(@maat.Id);
                }
                catch (DbUpdateException ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een databasefout opgetreden bij het maken van een product.");

                    // Returneer een specifieke foutresultaat voor databasegerelateerde fouten
                    return new ErrorResult("Er is een databasefout opgetreden bij het maken van het kleur.");
                }
                catch (Exception ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een onverwachte fout opgetreden bij het maken van een product.");

                    // Returneer een specifiek foutresultaat voor onverwachte fouten
                    return new ErrorResult("Er is een onverwachte fout opgetreden bij het maken van het kleur. Probeer het later opnieuw.");
                }

            }
        }

    }
}
