namespace EPiServer.Reference.Commerce.Site.Features.Api.Model
{
    public class AddCardResultMode
    {
        public AddCardResultMode()
        {
            Successful = true;
        }

        public bool Successful { get; set; }
        public string WarningMessage { get; set; }
    }
}