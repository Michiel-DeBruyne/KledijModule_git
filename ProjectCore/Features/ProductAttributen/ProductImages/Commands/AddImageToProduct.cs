using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Shared.Exceptions;

namespace ProjectCore.Features.ProductAttributen.ProductImages.Commands
{
    public class AddImageToProduct
    {
        public record AddCommand : IRequest<Result>
        {
            public string ImagePath { get; set; }
            public Guid ProductId { get; set; }
        }

        public class CommandValidator : AbstractValidator<AddCommand>
        {
            public CommandValidator()
            {
                RuleFor(c => c.ImagePath).NotNull().NotEmpty();
                RuleFor(c => c.ProductId).NotEmpty();
            }
        }

        public class AddImageToProductCommandHandler : IRequestHandler<AddCommand, Result>
        {
            private readonly ApplicationDbContext _context;

            public AddImageToProductCommandHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Result> Handle(AddCommand request, CancellationToken cancellationToken)
            {
                var validator = new CommandValidator();
                ValidationResult validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return new ValidationErrorResult("validatie voor het toevoegen van de foto aan het product mislukt", validationResult.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToList());
                }
                try
                {
                    var productImage = new Foto { ImageUrl = request.ImagePath, ProductId = request.ProductId };
                    await _context.ProductFotos.AddAsync(productImage);
                    await _context.SaveChangesAsync();
                    return new SuccessResult();
                }
                catch (DbUpdateException ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een databasefout opgetreden bij het maken van een product.");

                    // Returneer een specifieke foutresultaat voor databasegerelateerde fouten
                    return new ErrorResult("Er is een databasefout opgetreden bij het uploaden van de foto naar uw product.");
                }
                catch (Exception ex)
                {
                    // Log de uitzondering
                    //_logger.LogError(ex, "Er is een onverwachte fout opgetreden bij het maken van een product.");

                    // Returneer een specifiek foutresultaat voor onverwachte fouten
                    return new ErrorResult("Er is een onverwachte fout opgetreden bij het uploaden van de foto naar uw product. Probeer het later opnieuw of licht uw IT dienst in.");
                }
            }
        }
    }
}
