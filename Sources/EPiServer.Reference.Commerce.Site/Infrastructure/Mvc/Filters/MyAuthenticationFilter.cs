using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.Mvc.Filters
{
    public class MyAuthenticationFilter : IAuthenticationFilter
    {
        private readonly string _authenticationType;

        /// <summary>Initializes a new instance of the <see cref="HostAuthenticationFilter"/> class.</summary>
        /// <param name="authenticationType">The authentication type of the OWIN middleware to use.</param>
        public MyAuthenticationFilter(string authenticationType)
        {
            if (authenticationType == null)
            {
                throw new ArgumentNullException("authenticationType");
            }

            _authenticationType = authenticationType;
        }

        /// <summary>Gets the authentication type of the OWIN middleware to use.</summary>
        public string AuthenticationType
        {
            get { return _authenticationType; }
        }

        /// <inheritdoc />
        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            HttpRequestMessage request = context.Request;

            if (request == null)
            {
                throw new InvalidOperationException("Request mut not be null");
            }


            //In my case, i need try autenticate the request with BEARER token (Oauth)
            IAuthenticationManager authenticationManager = GetAuthenticationManagerOrThrow(request);

            cancellationToken.ThrowIfCancellationRequested();
            AuthenticateResult result = await authenticationManager.AuthenticateAsync(_authenticationType);
            ClaimsIdentity identity = null;

            if (result != null)
            {
                identity = result.Identity;

                if (identity != null)
                {
                    context.Principal = new ClaimsPrincipal(identity);
                }
            }
            else
            {
                try
                {
                    identity = new ClaimsIdentity(new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, "Brock"),
                            new Claim(ClaimTypes.Email, "test@test.com")
                        }, _authenticationType);

                    context.Principal = new ClaimsPrincipal(identity);
                    authenticationManager.SignIn(identity);

                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            HttpRequestMessage request = context.Request;

            if (request == null)
            {
                throw new InvalidOperationException("Request mut not be null");
            }

            IAuthenticationManager authenticationManager = GetAuthenticationManagerOrThrow(request);

            authenticationManager.AuthenticationResponseChallenge = AddChallengeAuthenticationType(
                authenticationManager.AuthenticationResponseChallenge, _authenticationType);

            return Task.FromResult(true);
        }

        public bool AllowMultiple
        {
            get { return true; }
        }

        private static AuthenticationResponseChallenge AddChallengeAuthenticationType(
            AuthenticationResponseChallenge challenge, string authenticationType)
        {
            List<string> authenticationTypes = new List<string>();
            AuthenticationProperties properties;

            if (challenge != null)
            {
                string[] currentAuthenticationTypes = challenge.AuthenticationTypes;

                if (currentAuthenticationTypes != null)
                {
                    authenticationTypes.AddRange(currentAuthenticationTypes);
                }

                properties = challenge.Properties;
            }
            else
            {
                properties = new AuthenticationProperties();
            }

            authenticationTypes.Add(authenticationType);

            return new AuthenticationResponseChallenge(authenticationTypes.ToArray(), properties);
        }

        private static IAuthenticationManager GetAuthenticationManagerOrThrow(HttpRequestMessage request)
        {
            var owinCtx = request.GetOwinContext();
            IAuthenticationManager authenticationManager = owinCtx != null ? owinCtx.Authentication : null;

            if (authenticationManager == null)
            {
                throw new InvalidOperationException("IAuthenticationManagerNotAvailable");
            }

            return authenticationManager;
        }
    }

    public class AuthenticationFailureResult : IHttpActionResult
    {
        public AuthenticationFailureResult(object jsonContent, HttpRequestMessage request)
        {
            JsonContent = jsonContent;
            Request = request;
        }

        public HttpRequestMessage Request { get; private set; }

        public object JsonContent { get; private set; }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            response.RequestMessage = Request;
            response.Content = new ObjectContent(JsonContent.GetType(), JsonContent, new JsonMediaTypeFormatter());
            return response;
        }
    }
}