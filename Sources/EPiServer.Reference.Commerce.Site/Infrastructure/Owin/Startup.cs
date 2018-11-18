using EPiServer.Core;
using EPiServer.Reference.Commerce.Shared.Models.Identity;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Shell;
using EPiServer.Web.Routing;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

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

            app.UseOAuthBearerTokens(new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/Token"),
                AuthorizeEndpointPath = new PathString("/Login"),
                Provider = new CustomOAuthAuthorizationServerProvider(),
                AllowInsecureHttp = true
            });

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider.
            // Configure the sign in cookie.
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
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

            //string auth0Domain = ConfigurationManager.AppSettings["auth0:Domain"];
            //string auth0ClientId = ConfigurationManager.AppSettings["auth0:ClientId"];
            //string auth0ClientSecret = ConfigurationManager.AppSettings["auth0:ClientSecret"];
            //string auth0RedirectUri = ConfigurationManager.AppSettings["auth0:RedirectUri"];
            //string auth0PostLogoutRedirectUri = ConfigurationManager.AppSettings["auth0:PostLogoutRedirectUri"];

            //app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            //{
            //    AuthenticationType = "Auth0",

            //    Authority = $"https://{auth0Domain}",

            //    ClientId = auth0ClientId,
            //    ClientSecret = auth0ClientSecret,

            //    RedirectUri = auth0RedirectUri,
            //    PostLogoutRedirectUri = auth0PostLogoutRedirectUri,

            //    ResponseType = OpenIdConnectResponseTypes.CodeIdToken,
            //    Scope = "openid profile",

            //    TokenValidationParameters = new TokenValidationParameters
            //    {
            //        NameClaimType = "name"
            //    },

            //    Notifications = new OpenIdConnectAuthenticationNotifications
            //    {
            //        RedirectToIdentityProvider = notification =>
            //        {
            //            if (notification.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
            //            {
            //                var logoutUri = $"https://{auth0Domain}/v2/logout?client_id={auth0ClientId}";

            //                var postLogoutUri = notification.ProtocolMessage.PostLogoutRedirectUri;
            //                if (!string.IsNullOrEmpty(postLogoutUri))
            //                {
            //                    if (postLogoutUri.StartsWith("/"))
            //                    {
            //                        // transform to absolute
            //                        var request = notification.Request;
            //                        postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;
            //                    }
            //                    logoutUri += $"&returnTo={ Uri.EscapeDataString(postLogoutUri)}";
            //                }

            //                notification.Response.Redirect(logoutUri);
            //                notification.HandleResponse();
            //            }
            //            return Task.FromResult(0);
            //        }
            //    }
            //});

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

            HttpConfiguration config = new HttpConfiguration();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            app.UseWebApi(config);

            //var domain = $"https://{ConfigurationManager.AppSettings["Auth0Domain"]}/";
            //var apiIdentifier = ConfigurationManager.AppSettings["Auth0ApiIdentifier"];

            //var keyResolver = new OpenIdConnectSigningKeyResolver(domain);
            //app.UseJwtBearerAuthentication(
            //    new JwtBearerAuthenticationOptions
            //    {
            //        AuthenticationType = OAuthDefaults.AuthenticationType,
            //        TokenValidationParameters = new TokenValidationParameters()
            //        {
            //            ValidAudience = "https://api.botframework.com",
            //            ValidIssuer = "https://sts.windows.net/d6d49420-f39b-4df7-a1dc-d59a935871db/",
            //            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) => keyResolver.GetSigningKey(kid)
            //        }
            //    });

            //app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
            //{
            //    Provider = new CustomBearerAuthenticationProvider()
            //});

            //PublicClientId = "self";
            //OAuthOptions = new OAuthAuthorizationServerOptions
            //{
            //    TokenEndpointPath = new PathString("/Token"),
            //    Provider = new AppOAuthProvider(PublicClientId),
            //    AuthorizeEndpointPath = new PathString("/Account/ExternalLogin"),
            //    AccessTokenExpireTimeSpan = TimeSpan.FromHours(4),
            //    AllowInsecureHttp = true //Don't do this in production ONLY FOR DEVELOPING: ALLOW INSECURE HTTP!  
            //};

            //// Enable the application to use bearer tokens to authenticate users  
            //app.UseOAuthBearerTokens(OAuthOptions);


            //app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
            //{
            //});

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