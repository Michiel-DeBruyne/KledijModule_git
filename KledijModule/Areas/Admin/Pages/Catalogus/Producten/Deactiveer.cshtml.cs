using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectCore.Features.Producten.Commands;
using ProjectCore.Shared.Exceptions;

namespace KledijModule.Areas.Admin.Pages.Catalogus.Producten
{
    /// <summary>
    /// Paginamodel voor het deactiveren van producten.
    /// </summary>
    public class DeactiveerModel : PageModel
    {
        #region Properties

        private readonly IMediator _mediator;

        #endregion Properties

        /// <summary>
        /// Instantieert een nieuwe instantie van mediator <see cref="DeactiveerModel"/> class.
        /// </summary>
        /// <param name="mediator">Mediator voor het verwerken van commando's.</param>
        public DeactiveerModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public void OnGet()
        {
            // Geen actie nodig voor GET-verzoeken
        }

        /// <summary>
        /// Deactiveren van producten 
        /// </summary>
        /// <param name="Command">Het deactiveercommando.</param>
        /// <returns>Een IActionResult die een redirect naar de Indexpagina uitvoert bij succes, anders keert terug naar de huidige pagina.</returns>
        public async Task<IActionResult> OnPost(DeactivateProducts.DeactivateProductsCommand Command)
        {
            var result = await _mediator.Send(Command);
            if (result is ErrorResult errorResult)
            {
                // Behandel foutresultaat
                TempData["ErrorMessage"] = errorResult.Message;
            }

            // Als het resultaat geen succes of fout is, keer terug naar de huidige pagina
            return RedirectToPage(nameof(Index));
        }
    }
}
