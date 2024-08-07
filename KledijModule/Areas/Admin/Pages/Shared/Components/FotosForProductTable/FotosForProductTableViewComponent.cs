using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectCore.Data;
using ProjectCore.Features.Producten.Queries;
using ProjectCore.Shared.Exceptions;
using static KledijModule.Areas.Admin.Pages.Shared.Components.KleurenForProductTable.KleurenForProductTableViewComponent.KleurenForProductViewModel;
using static ProjectCore.Features.Producten.Queries.GetFotosForProductList;

namespace KledijModule.Areas.Admin.Pages.Shared.Components.FotosForProductTable
{
    public class FotosForProductTableViewComponent : ViewComponent
    {

        private readonly ApplicationDbContext _context;
        private readonly IMediator _mediator;

        public FotosForProductViewModel Data { get; set; }

        public FotosForProductTableViewComponent(ApplicationDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid ProductId)
        {
            Data = new FotosForProductViewModel(); // Initialize Data property
            var items = await _mediator.Send(new GetFotosForProductList.Query() { ProductId = ProductId });
            if (items is SuccessResult<List<ProductFotoListVm>> succesResult)
            {
                Data.Fotos = succesResult.Data.Adapt<List<FotosForProductViewModel.ProductFoto>>();
                Data.ProductId = ProductId;
            }
            if (items is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Errors;
            }
            return View(Data);
        }


        public class FotosForProductViewModel
        {
            public Guid ProductId { get; set; }
            public List<ProductFoto> Fotos { get; set; } = new List<ProductFoto>();

            public record ProductFoto
            {
                public Guid Id { get; set; }
                public string ImageUrl { get; set; }
                public int Volgorde { get; set; }
            }
        }

    }
}
