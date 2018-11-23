using DalSoft.RestClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPiServer.Reference.Commerce.Bot
{
    public class CommerceClient
    {
        private readonly string token;
        private readonly string url = "http://localhost:50244/";

        public CommerceClient(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            this.token = token;
        }

        public async Task AddToCardAsync(string code)
        {
            var commerceClient = GetAuthenticatedClient();

            var result = await commerceClient.api.card.add.Query(new { code }).Post();
        }

        public async Task<IEnumerable<string>> ProductsAsync(string query)
        {
            var commerceClient = GetAuthenticatedClient();

            var result = await commerceClient.api.products.Query(new { q = query }).Get();

            return result;
        }

        private dynamic GetAuthenticatedClient()
        {
            dynamic client = new RestClient(url);
            return client.Headers(new { Authorization = $"Bearer {token}" });
        }
    }
}