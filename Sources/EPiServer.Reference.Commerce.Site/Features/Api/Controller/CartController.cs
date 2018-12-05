using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Api.Model;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using EPiServer.ServiceLocation;
using System.Collections.Generic;
using System.Web.Http;

namespace EPiServer.Reference.Commerce.Site.Features.Api.Controller
{
    [RoutePrefix("api/cart")]
    public class CartController : ApiController
    {
        private readonly ICartService cartService;
        private readonly IOrderRepository orderRepository;
        private readonly IPaymentProcessor paymentProcessor;
        private readonly IOrderGroupCalculator orderGroupCalculator;
        private ICart _cart;

        public CartController()
            : this(ServiceLocator.Current.GetInstance<ICartService>(),
                  ServiceLocator.Current.GetInstance<IOrderRepository>(),
                  ServiceLocator.Current.GetInstance<IPaymentProcessor>(),
                  ServiceLocator.Current.GetInstance<IOrderGroupCalculator>())
        { }

        public CartController(
            ICartService cartService, 
            IOrderRepository orderRepository, 
            IPaymentProcessor paymentProcessor,
            IOrderGroupCalculator orderGroupCalculator)
        {
            this.cartService = cartService;
            this.orderRepository = orderRepository;
            this.paymentProcessor = paymentProcessor;
            this.orderGroupCalculator = orderGroupCalculator;
        }

        [AcceptVerbs("GET")]
        public IEnumerable<CartItem> GetCart()
        {
            ModelState.Clear();

            if (Cart == null)
            {
                _cart = cartService.LoadOrCreateCart(cartService.DefaultCartName);
            }

            foreach (var shipment in _cart.GetFirstForm().Shipments)
            {
                foreach (var lineItem in shipment.LineItems)
                {
                    yield return new CartItem
                    {
                        Code = lineItem.Code,
                        Quantity = lineItem.Quantity
                    };
                }
            }
        }

        [AllowDBWrite]
        [AcceptVerbs("POST")]
        [Route("add")]
        public AddCartResultMode AddToCart(string code)
        {
            ModelState.Clear();

            if (Cart == null)
            {
                _cart = cartService.LoadOrCreateCart(cartService.DefaultCartName);
            }

            var result = new AddCartResultMode();

            if (cartService.AddToCart(Cart, code, out string warningMessage))
            {
                orderRepository.Save(Cart);

                return result;
            }
            else
            {
                result.WarningMessage = warningMessage;
            }

            return result;
        }

        [AllowDBWrite]
        [AcceptVerbs("POST")]
        [Route("checkout")]
        public AddCartResultMode Checkout()
        {
            ModelState.Clear();

            if (Cart == null)
            {
                _cart = cartService.LoadOrCreateCart(cartService.DefaultCartName);
            }

            Cart.ProcessPayments(paymentProcessor, orderGroupCalculator);

            var orderReference = orderRepository.SaveAsPurchaseOrder(Cart);
            var purchaseOrder = orderRepository.Load<IPurchaseOrder>(orderReference.OrderGroupId);
            orderRepository.Delete(Cart.OrderLink);

            return new AddCartResultMode();
        }

        private ICart Cart
        {
            get { return _cart ?? (_cart = cartService.LoadCart(cartService.DefaultCartName)); }
        }
    }
}