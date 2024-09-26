using Htmx;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Features.OrderItems.Queries;
using ProjectCore.Shared.Exceptions;

namespace KledijModule.Areas.Admin.Pages.Orders
{
    public class AantalBestellingenPerArtikelModel : PageModel
    {
        private readonly IMediator _mediator;
        public AantalBestellingenPerArtikelModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        [BindProperty(SupportsGet =true)]
        public DateTime? VanDatum { get;set; }

        [BindProperty(SupportsGet =true)]
        public DateTime? TotDatum { get;set; }

        public List<ProductOrderOverview> ProductOverzicht { get; set; } = new List<ProductOrderOverview>();

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await _mediator.Send(new GetOrderCountsPerProduct.Query { VanDatum = VanDatum, TotDatum = TotDatum });
            if(result is SuccessResult<List<GetOrderCountsPerProduct.ProductOrderOverview>> successResult)
            {
                ProductOverzicht = successResult.Data.Adapt<List<ProductOrderOverview>>();
            }
            if (result is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }

            return Page();

        }

        #region ViewModel
        /// <summary>
        /// Model dat het overzicht per product weergeeft, inclusief het totaal aantal bestellingen en details per maat en kleur.
        /// </summary>
        public class ProductOrderOverview
        {
            /// <summary>
            /// De naam van het product.
            /// </summary>
            public string ProductNaam { get; set; } = string.Empty;

            /// <summary>
            /// Het totaal aantal keer dat dit product is besteld.
            /// </summary>
            public int TotaalBesteld { get; set; }

            /// <summary>
            /// Een lijst met details over het aantal bestellingen per maat.
            /// </summary>
            public List<MaatDetail> MaatDetails { get; set; } = new List<MaatDetail>();
        }

        /// <summary>
        /// Model dat de details over de maat van een product bevat, inclusief het aantal kleuren.
        /// </summary>
        public class MaatDetail
        {
            /// <summary>
            /// De maat van het product.
            /// </summary>
            public string? Maat { get; set; }

            /// <summary>
            /// Het aantal bestellingen van deze specifieke maat.
            /// </summary>
            public int Aantal { get; set; }

            /// <summary>
            /// Een lijst met details over de kleuren voor deze maat.
            /// </summary>
            public List<KleurDetail> KleurDetails { get; set; } = new List<KleurDetail>();
        }

        /// <summary>
        /// Model dat de details over de kleur van een product bevat.
        /// </summary>
        public class KleurDetail
        {
            /// <summary>
            /// De kleur van het product.
            /// </summary>
            public string? Kleur { get; set; }

            /// <summary>
            /// Het aantal bestellingen van deze specifieke kleur.
            /// </summary>
            public int Aantal { get; set; }
        }

        #endregion ViewModel
    }
}
