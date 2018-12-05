using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace EPiServer.Reference.Commerce.Bot
{
    public static class OAuthHelpers
    {
        public static async Task AddToCartAsync(ITurnContext turnContext, TokenResponse tokenResponse, string code)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (tokenResponse == null)
            {
                throw new ArgumentNullException(nameof(tokenResponse));
            }

            var result = await new CommerceClient().AddToCarTAsync(tokenResponse.Token, code);

            var message = string.Empty;

            if (result.Successful)
            {
                message = $"The {code} is being added to your cart";
            }

            if (!string.IsNullOrWhiteSpace(result.WarningMessage))
            {
                message = $" with the warning: {result.WarningMessage}";
            }

            await turnContext.SendActivityAsync(message);
        }

        public static async Task ProductsAsync(ITurnContext turnContext, TokenResponse tokenResponse, string query)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (tokenResponse == null)
            {
                throw new ArgumentNullException(nameof(tokenResponse));
            }

            var products = await new CommerceClient().ProductsAsync(tokenResponse.Token, query);

            var reply = turnContext.Activity.CreateReply();

            reply.Text = $"List of products:{Environment.NewLine}{string.Join(",", products)}";

            await turnContext.SendActivityAsync(reply);
        }

        public static async Task CartAsync(ITurnContext turnContext, TokenResponse tokenResponse)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (tokenResponse == null)
            {
                throw new ArgumentNullException(nameof(tokenResponse));
            }

            var cardItems = await new CommerceClient().CartAsync(tokenResponse.Token);
            var reply = turnContext.Activity.CreateReply();

            if (cardItems == null || !cardItems.Any())
            {
                reply.Text = "You cart is empty";
            }
            else
            {
                reply.Text = $"Your cart: {string.Join(",", cardItems.Select(item => $" code: {item.Code}, qty: {item.Quantity}"))}";
            }

            await turnContext.SendActivityAsync(reply);
        }

        public static async Task CheckoutAsync(ITurnContext turnContext, TokenResponse tokenResponse)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (tokenResponse == null)
            {
                throw new ArgumentNullException(nameof(tokenResponse));
            }

            var result = await new CommerceClient().CheckoutAsync(tokenResponse.Token);

            var message = "Sucessfully checkout!";

            await turnContext.SendActivityAsync(message);
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