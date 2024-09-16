using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectCore.Data;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Shared.Exceptions;
using ProjectCore.Shared.RequestContext;
using System.ComponentModel;
using System.Security.Claims;
using static ProjectCore.Features.Gebruikers.Commands.UpdateUserBalance;

namespace ProjectCore.Features.Producten.Queries
{
    public class GetProductList
    {
        public record GetProductsListQuery : IRequest<Result>
        {
            public string? SearchQuery { get; set; }
            public string? Categorie { get; set; }
            public bool? OnlyPublished { get; set; }
            public bool? IncludeSubCategorieProducts { get; set; }

            public bool? EnkelCalog { get; set; } // ALs gebruiker enkel calog items mag zien

            public  bool? EnkelFavorieten {get;set;} // Om de favorieten van de gebruiker op te halen nz
        }

        public class GetProductsListQueryHandler : IRequestHandler<GetProductsListQuery, Result>
        {
            private readonly IMapper _mapper;
            private readonly ApplicationDbContext _context;
            private readonly IRequestContext _requestContext;

            public GetProductsListQueryHandler(IMapper mapper, ApplicationDbContext context, IRequestContext requestContext)
            {
                _mapper = mapper;
                _context = context;
                _requestContext = requestContext;
            }

            public async Task<Result> Handle(GetProductsListQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    string gebruikerId = _requestContext.UserId;
                    

                    var config = new TypeAdapterConfig();
                    config.NewConfig<Product, ProductsListVm>()
                    .Map(dest => dest.IsFavoriet, src =>
                            _context.Favorieten.Any(f => f.GebruikerId == gebruikerId && f.ProductId == src.Id));


                    IQueryable<Product> query = _context.Producten
                                         .Include(p => p.Fotos.Take(1))
                                         .Include(p => p.Categorie)
                                         .OrderBy(p => p.Naam);

                    if (!string.IsNullOrEmpty(request.SearchQuery))
                    {
                        query = query.Where(p => p.Naam.Contains(request.SearchQuery));
                    }
                    if (request.OnlyPublished != null && request.OnlyPublished == true)
                    {
                        query = query.Where(p => p.Beschikbaar);
                    }
                    if (!string.IsNullOrWhiteSpace(request.Categorie))
                    {
                        Categorie categorie = await _context.Categories.FirstOrDefaultAsync(c => c.Naam == request.Categorie);
                        //TODO: Is dit gewenst?
                        if (request.IncludeSubCategorieProducts != null && request.IncludeSubCategorieProducts == true)
                        {
                            var subcategories = await GetSubcategoriesRecursive(categorie.Id);
                            subcategories.Add(categorie.Id);

                            // Voeg de gevonden categorie en alle subcategorieën toe aan de query
                            query = query.Where(p => subcategories.Contains(p.CategorieId));
                        }
                        else
                        {
                            query = query.Where(p => p.Categorie == categorie!);
                        }

                    }
                    //Als een calog de catalogus opvraagt
                    if(request.EnkelCalog != null && request.EnkelCalog == true)
                    {
                        query = query.Where(p => p.BeschikbaarVoorCalog);
                    }
                    // Als enkel favorieten is ingesteld, filter alleen favorieten
                    if (request.EnkelFavorieten == true)
                    {
                        var favorieteProductIds = await _context.Favorieten
                            .Where(f => f.GebruikerId == gebruikerId)
                            .Select(f => f.ProductId)
                            .ToListAsync(cancellationToken);

                        query = query.Where(p => favorieteProductIds.Contains(p.Id));
                    }

                    var products = await query.ProjectToType<ProductsListVm>(config).ToListAsync(cancellationToken);

                    return new SuccessResult<List<ProductsListVm>>(products);
                }
                catch (Exception ex)
                {
                    // Handle unexpected exceptions
                    return new ErrorResult("Er is een onverwachte fout opgetreden bij het ophalen van de producten. Probeer later opnieuw!");
                }
            }

            private async Task<List<Guid>> GetSubcategoriesRecursive(Guid categoryId)
            {
                var subcategories = new List<Guid>();

                // Zoek alle subcategorieën van de opgegeven categorie recursief
                await GetSubcategoriesRecursiveHelper(categoryId, subcategories);

                return subcategories;
            }

            private async Task GetSubcategoriesRecursiveHelper(Guid categoryId, List<Guid> subcategories)
            {
                var children = await _context.Categories
                    .Where(c => c.ParentCategorieId == categoryId)
                    .Select(c => c.Id)
                    .ToListAsync();

                foreach (var childId in children)
                {
                    subcategories.Add(childId);
                    await GetSubcategoriesRecursiveHelper(childId, subcategories);
                }
            }
        }


        public class ProductsListVm
        {
            public Guid Id { get; init; }
            public string Naam { get; init; }
            public Category Categorie { get; init; }

            public bool IsFavoriet { get; set; }
            public Geslacht Geslacht { get; init; }
            public int Punten { get; init; }
            public bool Beschikbaar { get; init; }
            public bool BeschikbaarVoorCalog { get; init; }

            [DisplayName("Foto")]
            public List<ProductImage> Fotos { get; init; }
            public int MaxAantalBestelbaar { get; set; }
            public int PerAantalJaar { get; set; }
            public string DisplayVervangingsTermijn
            {
                get
                {
                    return $"{MaxAantalBestelbaar} per {PerAantalJaar} {(PerAantalJaar > 1 ? "jaren" : "jaar")}";
                }
            }

            public record Category
            {
                public string Naam { get; set; }
            }

            public record ProductImage
            {
                public string ImageUrl { get; init; }
            }
        }
    }
}
