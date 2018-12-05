//using Microsoft.Owin;
//using System.Threading.Tasks;
//using Microsoft.Owin.Security;
//using Microsoft.Owin.Security.OAuth;
//using System.Security.Claims;
//using System.Collections.Generic;

//[assembly: OwinStartup(typeof(EPiServer.Reference.Commerce.Site.Infrastructure.Owin.Startup))]
//namespace EPiServer.Reference.Commerce.Site.Infrastructure.Owin
//{
//    public class CustomOAuthAuthorizationServerProvider: OAuthAuthorizationServerProvider
//    {
//        public override Task ValidateAuthorizeRequest(OAuthValidateAuthorizeRequestContext context)
//        {
//            context.Validated();

//            return base.ValidateAuthorizeRequest(context);
//        }

//        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
//        {
//            context.Validated(context.RedirectUri);

//            return base.ValidateClientRedirectUri(context);
//        }

//        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
//        {
//            context.Validated();

//            return base.ValidateClientAuthentication(context);
//        }

//        public override Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
//        {

//            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

//            //Dummy check here, you need to do your DB checks against memebrship system http://bit.ly/SPAAuthCode
//            if (context.UserName != context.Password)
//            {
//                context.SetError("invalid_grant", "The user name or password is incorrect");
//                //return;
//                return Task.FromResult<object>(null);
//            }

//            var identity = new ClaimsIdentity("JWT");

//            identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
//            identity.AddClaim(new Claim("sub", context.UserName));
//            identity.AddClaim(new Claim(ClaimTypes.Role, "Manager"));
//            identity.AddClaim(new Claim(ClaimTypes.Role, "Supervisor"));

//            var props = new AuthenticationProperties(new Dictionary<string, string>
//                {
//                    {
//                         "audience", (context.ClientId == null) ? string.Empty : context.ClientId
//                    }
//                });

//            var ticket = new AuthenticationTicket(identity, props);
//            context.Validated(ticket);
//            return Task.FromResult<object>(null);
//        }
//    }
//}