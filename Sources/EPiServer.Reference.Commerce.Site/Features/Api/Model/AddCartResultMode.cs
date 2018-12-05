namespace EPiServer.Reference.Commerce.Site.Features.Api.Model
{
    public class AddCartResultMode
    {
        public AddCartResultMode()
        {
            Successful = true;
        }

        public bool Successful { get; set; }
        public string WarningMessage { get; set; }
    }
}