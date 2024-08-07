using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectCore.Data;
using ProjectCore.Shared.Exceptions;
using static KledijModule.Areas.Admin.Pages.Shared.Components.KleurenForProductTable.KleurenForProductTableViewComponent.KleurenForProductViewModel;
using static ProjectCore.Features.Producten.Queries.GetKleurenForProduct;

namespace KledijModule.Areas.Admin.Pages.Shared.Components.KleurenForProductTable
{
    public class KleurenForProductTableViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IMediator _mediator;

        public KleurenForProductViewModel Data { get; set; }


        public KleurenForProductTableViewComponent(ApplicationDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid ProductId)
        {
            Data = new KleurenForProductViewModel(); // Initialize Data property
            var items = await _mediator.Send(new Query() { ProductId = ProductId });
            if (items is SuccessResult<List<GetKleurenForProductListVm>> succesResult)
            {
                Data.Kleuren = succesResult.Data.Adapt<List<KleurenViewModel>>();
                Data.ProductId = ProductId;
            }
            if (items is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Errors;
            }
            return View(Data);
        }

        public class KleurenForProductViewModel
        {
            public Guid ProductId { get; set; }
            public List<KleurenViewModel> Kleuren { get; set; } = new List<KleurenViewModel>();
            public record KleurenViewModel
            {
                public Guid Id { get; set; }
                public string Kleur { get; set; }
            }
        }

    }
}
