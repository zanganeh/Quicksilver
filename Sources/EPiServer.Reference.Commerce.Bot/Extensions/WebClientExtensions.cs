using System.Net;
using System.Threading.Tasks;

namespace EPiServer.Reference.Commerce.Bot.Extensions
{
    public static class WebClientExtensions
    {
        public static async Task<T> UploadTaskAsync<T>(this WebClient client, string url, string content)
        {
            var json = await client.UploadStringTaskAsync(url, string.Empty);

            if (!string.IsNullOrWhiteSpace(json))
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
                return result;
            }

            return default(T);
        }

        public static async Task<T> DownloadTaskAsync<T>(this WebClient client, string url)
        {
            var json = await client.DownloadStringTaskAsync(url);

            if (!string.IsNullOrWhiteSpace(json))
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
                return result;
            }

            return default(T);
        }
    }
}