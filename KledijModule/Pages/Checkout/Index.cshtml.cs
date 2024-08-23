using KledijModule.Common.Authorization;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using ProjectCore.Domain.Entities.Catalogus;
using ProjectCore.Features.Orders.Commands;
using ProjectCore.Features.Webshop.Queries;
using ProjectCore.Features.WinkelMand.Queries;
using ProjectCore.Shared.Exceptions;
using System.ComponentModel;
using System.Security.Claims;
using System.Text;

namespace KledijModule.Pages.Checkout
{
    [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
    public class IndexModel : PageModel
    {
        #region Properties
        private readonly IMediator _mediator;
        private readonly GraphServiceClient _graphServiceClient;
        public CheckoutIndexViewModel ShoppingCart { get; set; } = new CheckoutIndexViewModel();
        public bool IsWebShopOpen { get; set; } = false;

        [BindProperty]
        public OrderCommandModel Order { get; set; } = new OrderCommandModel();

        public List<User> UsersList { get; set; } = new List<User>();

        #endregion Properties

        #region Ctor
        public IndexModel(IMediator mediator, GraphServiceClient graphServiceClient)
        {
            _mediator = mediator;
            _graphServiceClient = graphServiceClient;
        }
        #endregion Ctor

        #region ViewModel
        public class CheckoutIndexViewModel
        {
            public Guid ShoppingCartId { get; set; }

            public string UserId { get; set; }
            public List<ShoppingCartItem> ShoppingCartItems { get; set; } = new List<ShoppingCartItem>();
            public record ShoppingCartItem
            {
                public Guid Id { get; set; }
                public Product Product { get; set; }
                public int Hoeveelheid { get; set; }
                public string? Kleur { get; set; }
                public string? Maat { get; set; }
                public string? Opmerkingen { get; set; }
            }
        }
        #endregion ViewModel

        #region CommandModel // dient om de shoppingcartitems naartoe te mappen en door te geven naar de feature die het opslaat in de databank
        public class OrderCommandModel
        {
            [DisplayName("Gebruiker")]
            public string UserId { get; set; }

            //dient om het mogelijk te maken dat een admin een bestelling plaatst voor een ander en de code weet dat de admin zijn/haar winkelmand moet geleegd worden.
            public string RequesterId { get; set; } 

            public string UserNaam { get; set; }
            [DisplayName("Totaal")]
            public int TotaalPrijs { get; set; }
            public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

            public record OrderItem
            {
                public string ProductNaam { get; set; }
                public string? Maat { get; set; }
                public string? Kleur { get; set; }
                public Guid? ProductId { get; set; }
                public Product Product { get; set; }
                public int Prijs { get; set; }

                public int Hoeveelheid { get; set; }

                public string? Opmerkingen { get; set; }
            }
        }
        #endregion CommandModel

        public async Task OnGetAsync()
        {
            var result = await GetShoppingCart();
            SetUIShoppingCart(result);
            await IsWebshopOpen();
            await OnGetUsersListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var shoppingCart = await GetShoppingCart();

            if (shoppingCart is SuccessResult<GetShoppingCartList.GetShoppingCartListVm> shoppingCartSuccessResult)
            {
                TypeAdapterConfig<GetShoppingCartList.GetShoppingCartListVm.ShoppingCartItem, OrderCommandModel.OrderItem>.NewConfig()
                        .Map(dest => dest.ProductId, src => src.Product.Id)
                        .Map(dest => dest.ProductNaam, src => src.Product.Naam)
                        .Map(dest => dest.Prijs, src => src.Product.Prijs);
                Order.OrderItems = shoppingCartSuccessResult.Data.ShoppingCartItems.Adapt<List<OrderCommandModel.OrderItem>>();

                var CreateOrderResult = await _mediator.Send(Order.Adapt<CreateOrder.Command>());
                if (CreateOrderResult.Success) { return RedirectToPage("./CheckoutComplete"); }
                if (CreateOrderResult is ValidationErrorResult validationErrorResult)
                {
                    foreach (ValidationError error in validationErrorResult.Errors)
                    {
                        string modelStateKey = $"{nameof(Order)}.{error.PropertyName}";
                        ModelState.AddModelError(modelStateKey, error.Details);
                    }
                }
                else if (CreateOrderResult is ErrorResult errorResult) { TempData["Errors"] = errorResult.Message; }
            }
            else if (shoppingCart is ErrorResult error)
            {
                TempData["Errors"] = error.Message;
            }
            SetUIShoppingCart(shoppingCart);
          //await GetUsersListAsync();
            await IsWebshopOpen();
            //Als je hier komt is er iets misgelopen
            return Page();

        }

        private async Task<Result> GetShoppingCart()
        {
            var result = await _mediator.Send(new GetShoppingCartList.Query() { UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value });
            return result;
        }
        private void SetUIShoppingCart(Result result)
        {
            TypeAdapterConfig<GetShoppingCartList.GetShoppingCartListVm, CheckoutIndexViewModel>.NewConfig()
                .Map(dest => dest.ShoppingCartId, src => src.Id);
            if (result is SuccessResult<GetShoppingCartList.GetShoppingCartListVm> successResult)
            {
                ShoppingCart = successResult.Data.Adapt<CheckoutIndexViewModel>();
                Order.UserId = ShoppingCart.UserId;
                ViewData["Totaal"] = ShoppingCart.ShoppingCartItems.Sum(p => p.Hoeveelheid * p.Product.Prijs);
            }
            else if (result is ErrorResult getshoppingCartErrorResult)
            {
                TempData["Errors"] = getshoppingCartErrorResult.Message;
            }
        }

        public async Task OnGetUsersListAsync()
        {
            //if (!User.IsInRole(Roles.Admin)) { 
                Order.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Order.UserNaam = User.Identity.Name;
            //}
        }
        public async Task<IActionResult> OnGetUsersListAutoCompleteAsync(string term)
        {
            var result = await _graphServiceClient.Users.GetAsync((requestConfiguration) =>
            {
                if (!string.IsNullOrEmpty(term))
                {
                    requestConfiguration.QueryParameters.Filter = $"startswith(displayName,'{term}')";
                }
                    //requestConfiguration.QueryParameters.Filter = "userType eq 'Member'";// --> user.read.all permission required
                    requestConfiguration.QueryParameters.Select = new[] { "id", "displayName", "mail" };
            });

            var users = new List<User>();
            if (result != null && result.Value != null)
            {
                foreach (var user in result.Value)
                {
                    if (!string.IsNullOrEmpty(user.DisplayName))
                    {
                        users.Add(user);
                    }
                }
            }

            return new JsonResult(users);
        }
        private async Task IsWebshopOpen()
        {
            var isWebShopOpenresult = await _mediator.Send(new GetIsWebShopOpen.Query());
            if (isWebShopOpenresult is SuccessResult<bool> successResult)
            {
                IsWebShopOpen = successResult.Data;
            }
            if (isWebShopOpenresult is ErrorResult errorResult)
            {
                TempData["Errors"] = errorResult.Message;
            }
        }
    }
}