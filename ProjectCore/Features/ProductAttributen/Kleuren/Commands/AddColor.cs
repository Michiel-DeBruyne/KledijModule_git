using FluentValidation;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.ProductAttributen.Kleuren.Commands
{
    public class AddColor
    {

        public record AddColorCommand : IRequest<Result>
        {
            public string Kleur { get; set; }
        }

        //Nog niet gebruikt
        public class AddProductValidator : AbstractValidator<AddColorCommand>
        {
            private readonly ApplicationDbContext _context;
            public AddProductValidator(ApplicationDbContext context)
            {
                _context = context;
                RuleFor(model => model.Kleur).NotEmpty();
                RuleFor(e => e)
                        .MustAsync(NameUnique)
                        .WithMessage("Een Kleur met dezelfde naam bestaat al.");

            }

            private async Task<bool> NameUnique(AddColorCommand e, CancellationToken token)
            {
                return !(await IsNameUnique(e.Kleur));
            }
            public Task<bool> IsNameUnique(string Naam)
            {
                var matches = _context.ProductKleuren.Any(e => e.Kleur.Equals(Naam));
                return Task.FromResult(matches);
            }
        }
        public class CommandHandler : IRequestHandler<AddColorCommand, Result>
        {
            private readonly IMapper _mapper;
            private readonly ApplicationDbContext _context;
            public CommandHandler(IMapper mapper, ApplicationDbContext ctx)
            {
                _mapper = mapper;
                _context = ctx;
            }
            public async Task<Result> Handle(AddColorCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var validator = new AddProductValidator(_context);
                    var validationResult = await validator.ValidateAsync(request, cancellationToken);

                    if (!validationResult.IsValid)
                    {
                        return new ValidationErrorResult("Validatie mislukt bij het aanmaken van het kleur", validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList());
                    }

                    var color = _mapper.Map<ProductKleur>(request);
                    await _context.ProductKleuren.AddAsync(color, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                    return new SuccessResult<Guid>(color.Id);
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
