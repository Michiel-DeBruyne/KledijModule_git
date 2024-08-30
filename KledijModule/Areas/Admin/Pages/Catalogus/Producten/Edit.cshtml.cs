using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Features.Categories.Queries;
using ProjectCore.Features.ProductAttributen.ProductImages.Commands;
using ProjectCore.Features.Producten.Commands;
using ProjectCore.Features.Producten.Queries;
using ProjectCore.Shared.Exceptions;
using System.ComponentModel;

namespace KledijModule.Areas.Admin.Pages.Catalogus.Producten
{
    public class EditModel : PageModel
    {
        #region Properties
        private readonly IMediator _mediator;
        [BindProperty]
        public EditProductViewModel ProductMetDetails { get; set; }
        [BindProperty]
        [DisplayName("Product-Fotos")]
        public List<IFormFile> ProductImages { get; set; }

        private readonly string[] permittedFileExtensions = { ".png", ".jpg" };
        private readonly long _fileSizeLimit;
        private readonly string _targetFilePath;
        private readonly IWebHostEnvironment _webHostEnvironment;
        #endregion Properties

        #region ctor

        public EditModel(IConfiguration config, IMediator mediator, IWebHostEnvironment webHostEnvironment)
        {
            _mediator = mediator;
            _fileSizeLimit = config.GetValue<long>("FileSizeLimit");
            _targetFilePath = config.GetValue<string>("StoredFilesPath");
            _webHostEnvironment = webHostEnvironment;
        }
        #endregion ctor

        #region ViewModel
        public class EditProductViewModel
        {
            public Guid Id { get; set; }
            public string Naam { get; set; } = string.Empty;
            public string? Beschrijving { get; set; }
            public bool Beschikbaar { get; set; } = false;
            public int Punten { get; set; }
            public Geslacht Geslacht { get; set; }
            public int? ArtikelNummer { get; set; }
            [DisplayName("Categorie")]
            public Guid CategorieId { get; set; }

            [DisplayName("Maximum aantal bestelbaar")]
            public int MaxAantalBestelbaar { get; set; }

            [DisplayName("Per aantal jaar")]
            public int PerAantalJaar { get; set; }

            //TODO: nieuwe command maken die onderstaande niet includes fz, komt nooit mee vanuit view bij model binding.

            public List<ProductFoto> Fotos { get; set; } = new List<ProductFoto>();

            public List<ProductMaat> Maten { get; set; } = new List<ProductMaat>();
            public List<ProductKleur> Kleuren { get; set; } = new List<ProductKleur>();
            public record ProductFoto
            {
                public Guid Id { get; set; }
                public string ImageUrl { get; set; }
                public int Volgorde { get; set; }
            }

            public record ProductMaat
            {
                public Guid Id { get; set; }
                public string Maat { get; set; }
            }

            public record ProductKleur
            {
                public Guid Id { get; set; }
                public string Kleur { get; set; } = string.Empty;
            }

        }
        #endregion ViewModel


        public async Task OnGetAsync(GetProductMetDetails.Query query)
        {
            var getProductResult = await _mediator.Send(query);
            if (getProductResult is SuccessResult<GetProductMetDetails.ProductMetDetailsVm> productResult)
            {
                ProductMetDetails = productResult.Data.Adapt<EditProductViewModel>();
            }
            else if (getProductResult is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }

            await LoadCategoriesAsync();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            var result = await _mediator.Send(ProductMetDetails.Adapt<EditProduct.EditProductCommand>());
            if (result is SuccessResult<Guid> successResult)
            {
                // Indien alles OK : Redirect to index page
                return RedirectToPage(nameof(Index));
            }

            if (result is ValidationErrorResult validationErrorResult)
            {
                foreach (ValidationError error in validationErrorResult.Errors)
                {
                    string modelStateKey = $"{nameof(Product)}.{error.PropertyName}"; // TODO: dit is misschien wat overkill, bespreken met Caitlin.
                    ModelState.AddModelError(modelStateKey, error.Details);
                }
            }
            if (result is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }

            await LoadCategoriesAsync();
            return Page();
        }
        public async Task<IActionResult> OnPostUploadImagesAsync()
        {
            foreach (var imageFile in ProductImages)
            {
                //Kijk of extensie van bestand toegelaten is.
                var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(ext) || !permittedFileExtensions.Contains(ext))
                {
                    throw new Exception("Foto extensie is niet toegelaten. De toegelaten extensies zijn png, jpg.");
                }

                if (imageFile.Length > _fileSizeLimit)
                {
                    throw new Exception("Afbeelding is te groot. Gelieve een kleinere afbeelding te selecteren.");
                }

                if (!ModelState.IsValid)
                {
                    return Page();
                }
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                string trustedFileNameForFileStorage = Path.GetRandomFileName() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(wwwRootPath + _targetFilePath, trustedFileNameForFileStorage);

                try
                {
                    using (var fileStream = System.IO.File.Create(filePath))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                    var result = await _mediator.Send(new AddImageToProduct.AddCommand { ImagePath = _targetFilePath + trustedFileNameForFileStorage, ProductId = ProductMetDetails.Id });
                    if (result is SuccessResult successResult)
                    {
                        continue;
                    }
                    if (result is ValidationErrorResult validationErrorResult)
                    {
                        foreach (ValidationError error in validationErrorResult.Errors)
                        {
                            string modelStateKey = $"{nameof(Product)}.{error.PropertyName}"; // TODO: dit is misschien wat overkill, bespreken met Caitlin.
                            ModelState.AddModelError(modelStateKey, error.Details);
                        }
                    }
                    if (result is ErrorResult errorResult)
                    {
                        TempData["Errors"] = errorResult.Message;
                    }
                }
                catch (Exception ex)
                {
                    //TODO: error handlen
                }
            }
            return ViewComponent("FotosForProductTable", new { ProductId = ProductMetDetails.Id });
        }
        private async Task LoadCategoriesAsync()
        {
            var result = await _mediator.Send(new GetCategoriesList.Query());
            if (result is SuccessResult<List<GetCategoriesList.CategoriesListVm>> successResult)
            {
                ViewData["Categories"] = new SelectList(successResult.Data, "Id", "Naam");
            }
            if (result is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
        }

    }
}
