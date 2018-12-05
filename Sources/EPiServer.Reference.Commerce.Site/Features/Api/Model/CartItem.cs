namespace EPiServer.Reference.Commerce.Site.Features.Api.Model
{
    public class CartItem
    {
        public string Code { get; set; }
        public string DisplayName { get; set; }
        public decimal Quantity { get; set; }
        public string Price { get; set; }
    }
}