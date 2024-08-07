using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Features.Categories.Queries;
using ProjectCore.Shared.Exceptions;
using System.ComponentModel;

namespace KledijModule.Areas.Admin.Pages.Catalogus.Categories
{
    public class IndexModel : PageModel
    {
        #region Properties
        public List<CategoryIndexViewModel> Categorieen { get; set; } = new List<CategoryIndexViewModel>();
        private readonly IMediator _mediator;
        #endregion Properties

        #region ctor
        public IndexModel(IMediator mediator)
        {
            _mediator = mediator;
        }
        #endregion ctor

        #region ViewModel
        // dit misschien refactoren naar result fz die een lijst returned? kan leesbaarder zijn.
        public class CategoryIndexViewModel
        {
            public Guid Id { get; set; }
            public string Naam { get; set; }
            [DisplayName("Hoofd categorie")]
            public Categorie? ParentCategorie { get; set; }
            public record Categorie
            {
                public string Naam { get; set; } = string.Empty;
            }
        }
        #endregion ViewModel

        public async Task OnGet()
        {
            var result = await _mediator.Send(new GetCategoriesList.Query());
            if (result is SuccessResult<List<GetCategoriesList.CategoriesListVm>> successResult)
            {
                Categorieen = successResult.Data.Adapt<List<CategoryIndexViewModel>>();
            }
            if (result is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
        }

    }
}
