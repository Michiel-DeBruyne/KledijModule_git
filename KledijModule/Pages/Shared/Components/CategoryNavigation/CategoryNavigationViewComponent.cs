using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectCore.Features.Categories.Queries;
using ProjectCore.Shared.Exceptions;

namespace KledijModule.Pages.Shared.Components.CategoryNavigation
{
    public class CategoryNavigationViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;

        public CategoryNavigationViewComponent(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid rootCategoryId = default)
        {
            //TODO : test implementatie, concretiseren
            var model = await PrepareCategoryNavModel(rootCategoryId);
            return View(model);
        }

        public class CategorieNavModel
        {
            public CategorieNavModel()
            {
                SubCategorieën = new List<CategorieNavModel>();
            }
            public Guid Id { get; set; }
            public string Naam { get; set; }

            public List<CategorieNavModel> SubCategorieën { get; set; }
        }

        public virtual async Task<List<CategorieNavModel>> PrepareCategoryNavModel(Guid rootCategoryId, bool loadSubCategories = true)
        {
            var result = new List<CategorieNavModel>();
            var allCategoriesResult = await _mediator.Send(new GetCategoriesList.Query());
            if (allCategoriesResult is SuccessResult<List<GetCategoriesList.CategoriesListVm>> successResult)
            {
                var categories = successResult.Data.Where(c => c.ParentCategorieId == rootCategoryId).OrderBy(c => c.Naam).ToList();
                foreach (var categorie in categories)
                {
                    var categorieModel = new CategorieNavModel
                    {
                        Id = categorie.Id,
                        Naam = categorie.Naam
                    };

                    if (loadSubCategories)
                    {
                        var subCategories = await PrepareCategoryNavModel(categorie.Id);
                        categorieModel.SubCategorieën.AddRange(subCategories);
                    }
                    result.Add(categorieModel);
                }
            }
            return result;
        }
    }
}
