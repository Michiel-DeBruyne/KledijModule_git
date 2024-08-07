using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectCore.Data;
using ProjectCore.Features.Producten.Queries;
using ProjectCore.Shared.Exceptions;
using static KledijModule.Areas.Admin.Pages.Shared.Components.MatenForProductTable.MatenForProductTableViewComponent.MatenForProductViewModel;

namespace KledijModule.Areas.Admin.Pages.Shared.Components.MatenForProductTable
{
    public class MatenForProductTableViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IMediator _mediator;

        public MatenForProductViewModel Data { get; set; }


        public MatenForProductTableViewComponent(ApplicationDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid ProductId)
        {
            Data = new MatenForProductViewModel(); // Initialize Data property
            var items = await _mediator.Send(new GetMatenForProduct.Query() { ProductId = ProductId });
            if (items is SuccessResult<List<GetMatenForProduct.GetMatenForProductListVm>> succesResult)
            {
                Data.Maten = succesResult.Data.Adapt<List<MatenViewModel>>();
                Data.ProductId = ProductId;
            }
            if (items is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Errors;
            }
            return View(Data);
        }

        public class MatenForProductViewModel
        {
            public Guid ProductId { get; set; }
            public List<MatenViewModel> Maten { get; set; } = new List<MatenViewModel>();
            public record MatenViewModel
            {
                public Guid Id { get; set; }
                public string Maat { get; set; }
            }
        }

        //https://dev.to/ezzylearning/creating-view-components-in-asp-net-core-3-1-1161

    }
}
