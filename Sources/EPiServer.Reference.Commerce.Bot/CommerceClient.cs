using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EPiServer.Reference.Commerce.Bot.Extensions;
using EPiServer.Reference.Commerce.Bot.Model;
using EPiServer.Reference.Commerce.Bot.Models;

namespace EPiServer.Reference.Commerce.Bot
{
    public class CommerceClient
    {
        const string baseUrl = "http://localhost:50244";

        public Task<AddCartResultMode> AddToCarTAsync(string token, string code)
        {
            var commerceClient = GetAuthenticatedClient(token);

            return commerceClient.UploadTaskAsync<AddCartResultMode>($"{baseUrl}/api/cart/add?code={code}", string.Empty);
        }

        public async Task<IEnumerable<string>> ProductsAsync(string token, string query)
        {
            var commerceClient = GetAuthenticatedClient(token);

            var result = await commerceClient.DownloadDataTaskAsync(new Uri($"{baseUrl}/api/products/?q={query}"));

            return Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<CartItem>> CartAsync(string token)
        {
            var client = GetAuthenticatedClient(token);

            return (await client.DownloadTaskAsync<IEnumerable<CartItem>>($"{baseUrl}/api/cart")) ?? Enumerable.Empty<CartItem>();
        }

        public Task<AddCartResultMode> CheckoutAsync(string token)
        {
            var commerceClient = GetAuthenticatedClient(token);

            return commerceClient.UploadTaskAsync<AddCartResultMode>($"{baseUrl}/api/cart/checkout", string.Empty);
        }

        private WebClient GetAuthenticatedClient(string token)
        {
            var client = new WebClient();

            client.Headers[HttpRequestHeader.Authorization] = $"Bearer {token}";

            return client;
        }
    }
}