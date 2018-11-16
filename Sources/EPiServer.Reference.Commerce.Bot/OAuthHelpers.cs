using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace EPiServer.Reference.Commerce.Bot
{
    public static class OAuthHelpers
    {
        public static async Task AddToCardAsync(ITurnContext turnContext, TokenResponse tokenResponse, string code)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (tokenResponse == null)
            {
                throw new ArgumentNullException(nameof(tokenResponse));
            }

            var client = new CommerceClient(tokenResponse.Token);
            await client.AddToCardAsync(code);

            await turnContext.SendActivityAsync(
                $"The {code} is being added to your card");
        }

        // Displays information about the user in the bot.
        public static async Task ProductsAsync(ITurnContext turnContext, TokenResponse tokenResponse)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (tokenResponse == null)
            {
                throw new ArgumentNullException(nameof(tokenResponse));
            }

            // Pull in the data from the Microsoft Graph.
            var client = new CommerceClient(tokenResponse.Token);
            var products = await client.ProductsAsync();

            var reply = turnContext.Activity.CreateReply();

            reply.Text = $"List of products:{Environment.NewLine}{string.Join(",", products)}";

            await turnContext.SendActivityAsync(reply);
        }

        public static async Task CardAsync(ITurnContext turnContext, TokenResponse tokenResponse)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (tokenResponse == null)
            {
                throw new ArgumentNullException(nameof(tokenResponse));
            }

            var client = new CommerceClient(tokenResponse.Token);
            //var messages = await client.GetRecentMailAsync();
            var reply = turnContext.Activity.CreateReply();
            

            await turnContext.SendActivityAsync(reply);
        }

        public static OAuthPrompt Prompt(string connectionName)
        {
            return new OAuthPrompt(
                "loginPrompt",
                new OAuthPromptSettings
                {
                    ConnectionName = connectionName,
                    Text = "Please login",
                    Title = "Login",
                    Timeout = 300000, // User has 5 minutes to login
                });
        }
    }
}
