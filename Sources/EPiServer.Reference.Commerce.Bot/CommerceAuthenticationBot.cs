using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace EPiServer.Reference.Commerce.Bot
{
    public class CommerceAuthenticationBot : IBot
    {
        private const string ConnectionSettingName = "A0";

        private const string WelcomeText =
            @"Welcome to Episerver Commerce Bot. You can type 'products' to see the list of product, 
            'add <sku number>' to add product to the shopping cart, 
            'card' to see your current card, 'signout' to sign out or 'help' to view the commands again. 
            Any other text will display your token.";

        private readonly CommerceAuthenticationBotAccessors stateAccessors;
        private readonly DialogSet dialogs;

        public CommerceAuthenticationBot(CommerceAuthenticationBotAccessors accessors)
        {
            if (string.IsNullOrWhiteSpace(ConnectionSettingName))
            {
                throw new InvalidOperationException("ConnectionSettingName must be configured prior to running the bot.");
            }

            stateAccessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            dialogs = new DialogSet(stateAccessors.ConversationDialogState);
            dialogs.Add(OAuthHelpers.Prompt(ConnectionSettingName));
            dialogs.Add(new ChoicePrompt("choicePrompt"));
            dialogs.Add(new WaterfallDialog("commerceDialog", new WaterfallStep[] { PromptStepAsync, ProcessStepAsync }));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.Message:
                    await ProcessInputAsync(turnContext, cancellationToken);
                    break;
                case ActivityTypes.Event:
                case ActivityTypes.Invoke:

                    // Sanity check the activity type and channel Id.
                    if (turnContext.Activity.Type == ActivityTypes.Invoke && turnContext.Activity.ChannelId != "msteams")
                    {
                        throw new InvalidOperationException("The Invoke type is only valid onthe MSTeams channel.");
                    }

                    var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);
                    await dc.ContinueDialogAsync(cancellationToken);
                    if (!turnContext.Responded)
                    {
                        await dc.BeginDialogAsync("commerceDialog", cancellationToken: cancellationToken);
                    }

                    break;
                case ActivityTypes.ConversationUpdate:
                    if (turnContext.Activity.MembersAdded != null)
                    {
                        await SendWelcomeMessageAsync(turnContext, cancellationToken);
                    }

                    break;
            }
        }

        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var reply = turnContext.Activity.CreateReply();
                    reply.Text = WelcomeText;
                    reply.Attachments = new List<Attachment> { CreateHeroCard(member.Id).ToAttachment() };
                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
            }
        }

        private static HeroCard CreateHeroCard(string newUserName)
        {
            var heroCard = new HeroCard($"Welcome {newUserName}", "OAuthBot")
            {
                Images = new List<CardImage>
                {
                    new CardImage(
                        "https://botframeworksamples.blob.core.windows.net/samples/aadlogo.png",
                        "AAD Logo",
                        new CardAction(
                            ActionTypes.OpenUrl,
                            value: "https://ms.portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/Overview")),
                },
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.ImBack, "Products", text: "Products", displayText: "Products", value: "Products"),
                    new CardAction(ActionTypes.ImBack, "Card", text: "Card", displayText: "Card", value: "Card"),
                    new CardAction(ActionTypes.ImBack, "View Token", text: "View Token", displayText: "View Token", value: "View Token"),
                    new CardAction(ActionTypes.ImBack, "Help", text: "Help", displayText: "Help", value: "Help"),
                    new CardAction(ActionTypes.ImBack, "Signout", text: "Signout", displayText: "Signout", value: "Signout"),
                },
            };
            return heroCard;
        }

        private async Task<DialogContext> ProcessInputAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);
            switch (turnContext.Activity.Text.ToLowerInvariant())
            {
                case "signout":
                case "logout":
                case "signoff":
                case "logoff":
                    // The bot adapter encapsulates the authentication processes and sends
                    // activities to from the Bot Connector Service.
                    var botAdapter = (BotFrameworkAdapter)turnContext.Adapter;
                    await botAdapter.SignOutUserAsync(turnContext, ConnectionSettingName, cancellationToken: cancellationToken);

                    // Let the user know they are signed out.
                    await turnContext.SendActivityAsync("You are now signed out.", cancellationToken: cancellationToken);
                    break;
                case "help":
                    await turnContext.SendActivityAsync(WelcomeText, cancellationToken: cancellationToken);
                    break;
                default:
                    // The user has input a command that has not been handled yet,
                    // begin the waterfall dialog to handle the input.
                    await dc.ContinueDialogAsync(cancellationToken);
                    if (!turnContext.Responded)
                    {
                        await dc.BeginDialogAsync("commerceDialog", cancellationToken: cancellationToken);
                    }

                    break;
            }

            return dc;
        }

        private async Task<DialogTurnResult> ProcessStepAsync(WaterfallStepContext step, CancellationToken cancellationToken)
        {
            if (step.Result != null)
            {
                var tokenResponse = step.Result as TokenResponse;

                // If we have the token use the user is authenticated so we may use it to make API calls.
                if (tokenResponse?.Token != null)
                {
                    var parts = stateAccessors.CommandState.GetAsync(step.Context, () => string.Empty, cancellationToken: cancellationToken).Result.Split(' ');
                    string command = parts[0].ToLowerInvariant();

                    if (command == "products")
                    {
                        await OAuthHelpers.ProductsAsync(step.Context, tokenResponse);
                    }
                    else if (command.StartsWith("add"))
                    {
                        await OAuthHelpers.AddToCardAsync(step.Context, tokenResponse, parts[1]);
                    }
                    else if (command.StartsWith("card"))
                    {
                        await OAuthHelpers.CardAsync(step.Context, tokenResponse);
                    }
                    else
                    {
                        await step.Context.SendActivityAsync($"Your token is: {tokenResponse.Token}", cancellationToken: cancellationToken);
                    }

                    await stateAccessors.CommandState.DeleteAsync(step.Context, cancellationToken);
                }
            }
            else
            {
                await step.Context.SendActivityAsync("We couldn't log you in. Please try again later.", cancellationToken: cancellationToken);
            }

            return await step.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext step, CancellationToken cancellationToken)
        {
            var activity = step.Context.Activity;

            // Set the context if the message is not the magic code.
            if (activity.Type == ActivityTypes.Message &&
                (!Regex.IsMatch(activity.Text, @"(\d{6})") || activity.Text.Contains("SKU")))
            {
                await stateAccessors.CommandState.SetAsync(step.Context, activity.Text, cancellationToken);
                await stateAccessors.UserState.SaveChangesAsync(step.Context, cancellationToken: cancellationToken);
            }

            return await step.BeginDialogAsync("loginPrompt", cancellationToken: cancellationToken);
        }
    }
}