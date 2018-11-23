using EPiServer.Reference.Commerce.Site.Features.Search.Services;
using EPiServer.ServiceLocation;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace EPiServer.Reference.Commerce.Site.Features.Api.Controller
{
    [RoutePrefix("api/products")]
    public class ProductsController : ApiController
    {
        private readonly ISearchService searchService;

        public ProductsController() : this(ServiceLocator.Current.GetInstance<ISearchService>())
        { }

        public ProductsController(ISearchService searchService)
        {
            this.searchService = searchService;
        }

        public IEnumerable<string> Get(string q = null)
        {
            return searchService.QuickSearch(q).Select(a => a.Code);
        }
    }
}