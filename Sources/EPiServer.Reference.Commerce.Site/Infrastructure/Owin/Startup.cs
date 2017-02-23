﻿using EPiServer.Core;
using EPiServer.Reference.Commerce.Shared.Models.Identity;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.MicrosoftAccount;
using Microsoft.Owin.Security.Twitter;
using Owin;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using EPiServer.Framework.Modules;
using EPiServer.Shell;

[assembly: OwinStartupAttribute(typeof(EPiServer.Reference.Commerce.Site.Infrastructure.Owin.Startup))]
namespace EPiServer.Reference.Commerce.Site.Infrastructure.Owin
{
    public class Startup
    {
        const string LogoutUrl = "/util/logout.aspx";

        public void Configuration(IAppBuilder app)
        {
            // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864

            // Configure the db context, user manager and signin manager to use a single instance per request.
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider.
            // Configure the sign in cookie.
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager)),
                    OnApplyRedirect = ApplyRedirect,
                    OnResponseSignedIn = context => ServiceLocator.Current.GetInstance<ISynchronizingUserService>().SynchronizeAsync(context.Identity, Enumerable.Empty<string>())
                }
            });

            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            app.Map(LogoutUrl, map =>
            {
                map.Run(ctx =>
                {
                    ctx.Authentication.SignOut();
                    return Task.Run(() => ctx.Response.Redirect(UrlResolver.Current.GetUrl(ContentReference.StartPage)));
                });
            });

            // To enable using an external provider like Facebook or Google, uncomment the options you want to make available.
            // Also remember to apply the correct client id and secret code to each method that you call below.
            // Uncomment the external login providers you want to enable in your site. Don't forget to change their respective client id and secret.

            //EnableMicrosoftAccountLogin(app);
            //EnableTwitterAccountLogin(app);
            //EnableFacebookAccountLogin(app);
            //EnableGoogleAccountLogin(app);
        }

        /// <summary>
        /// Method for managing all the re-directs that occurs on the website.
        /// </summary>
        /// <param name="context"></param>
        private static void ApplyRedirect(CookieApplyRedirectContext context)
        {
            string backendPath = Paths.ProtectedRootPath.TrimEnd('/');

            // We use the method for transferring the user to the backend login pages if she tries to go
            // to the Edit views without being navigated.
            if (context.Request.Uri.AbsolutePath.StartsWith(backendPath) && !context.Request.User.Identity.IsAuthenticated)
            {
                context.RedirectUri = VirtualPathUtility.ToAbsolute("~/BackendLogin") +
                        new QueryString(
                            context.Options.ReturnUrlParameter,
                            context.Request.Uri.AbsoluteUri);
            }

            context.Response.Redirect(context.RedirectUri);
        }
    }
}