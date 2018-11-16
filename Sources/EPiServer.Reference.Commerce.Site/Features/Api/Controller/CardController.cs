using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Api.Model;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using EPiServer.ServiceLocation;
using System.Web.Http;

namespace EPiServer.Reference.Commerce.Site.Features.Api.Controller
{
    [RoutePrefix("api/card")]
    public class CardController : ApiController
    {
        private readonly ICartService _cartService;
        private readonly IOrderRepository _orderRepository;
        private ICart _cart;

        public CardController()
            : this(ServiceLocator.Current.GetInstance<ICartService>(), ServiceLocator.Current.GetInstance<IOrderRepository>())
        {
        }

        public CardController(ICartService cartService, IOrderRepository orderRepository)
        {
            _cartService = cartService;
            _orderRepository = orderRepository;
        }

        [AllowDBWrite]
        [AcceptVerbs("POST")]
        [Route("add")]
        public AddCardResultMode AddToCart(string code)
        {
            ModelState.Clear();

            if (Cart == null)
            {
                _cart = _cartService.LoadOrCreateCart(_cartService.DefaultCartName);
            }

            var result = new AddCardResultMode();

            if (_cartService.AddToCart(Cart, code, out string warningMessage))
            {
                _orderRepository.Save(Cart);

                return result;
            }
            else
            {
                result.WarningMessage = warningMessage;
            }

            return result;
        }

        private ICart Cart
        {
            get { return _cart ?? (_cart = _cartService.LoadCart(_cartService.DefaultCartName)); }
        }
    }
}