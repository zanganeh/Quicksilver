using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DalSoft.RestClient;

namespace EPiServer.Reference.Commerce.Bot
{
    public class CommerceClient
    {
        private readonly string _token;
        private readonly string _url = "http://localhost:50244/";

        public CommerceClient(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            _token = token;
        }

        public async Task AddToCardAsync(string code)
        {
            var commerceClient = GetAuthenticatedClient();

            var result = await commerceClient.api.card.add.Query(new { code }).Post();

        }

        private dynamic GetAuthenticatedClient()
        {

            dynamic client = new RestClient(_url);
            return client.Headers(new { Authorization = $"Bearer {_token}" });
        }

        public Task<IEnumerable<string>> ProductsAsync()
        {
            return Task.FromResult(new[] { "asd", "bc" }.AsEnumerable());
        }
    }
}