using System;
using System.Configuration;
using System.IdentityModel.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using AzureADTest.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security.ActiveDirectory;

namespace AzureADTest
{
    public partial class Startup
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string appKey = ConfigurationManager.AppSettings["ida:ClientSecret"];
        private static string aadInstance = EnsureTrailingSlash(ConfigurationManager.AppSettings["ida:AADInstance"]);
        private static string tenantId = ConfigurationManager.AppSettings["ida:TenantId"];
        private static string postLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];

        private string authority = aadInstance + tenantId;
        private static string graphResourceId = "https://graph.windows.net";

        public void ConfigureAuth(IAppBuilder app)
        {
			//WindowsAzureActiveDirectoryBearerAuthenticationOptions options = new WindowsAzureActiveDirectoryBearerAuthenticationOptions
			//                                                                 {
			//                                                                  Tenant = ConfigurationManager.AppSettings["aad:Audience"],
			//                                                                  TokenValidationParameters = new TokenValidationParameters
			//                                                                  {
			//                                                                            ValidAudience = ConfigurationManager.AppSettings["aad:Audience"]
			//                                                                        }
			//                                                                 };

			//app.UseWindowsAzureActiveDirectoryBearerAuthentication(options);

			app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

			app.UseCookieAuthentication(new CookieAuthenticationOptions());

			app.UseOpenIdConnectAuthentication(
				new OpenIdConnectAuthenticationOptions
				{
					ClientId = clientId,
					Authority = authority,
					PostLogoutRedirectUri = postLogoutRedirectUri,
					Notifications = new OpenIdConnectAuthenticationNotifications()
					{
						//
						// If there is a code in the OpenID Connect response, redeem it for an access token and refresh token, and store those away.
						//
						AuthorizationCodeReceived = (context) =>
						{
							var code = context.Code;
							ClientCredential credential = new ClientCredential(clientId, appKey);
							string signedInUserID = context.AuthenticationTicket.Identity.FindFirst(ClaimTypes.NameIdentifier).Value;
							AuthenticationContext authContext = new AuthenticationContext(authority, new ADALTokenCache(signedInUserID));
							AuthenticationResult result = authContext.AcquireTokenByAuthorizationCodeAsync(
							  code, new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)), credential, graphResourceId).Result;

							return Task.FromResult(0);
						}
					}
				}
				);

			// This makes any middleware defined above this line run before the Authorization rule is applied in web.config
			app.UseStageMarker(PipelineStage.Authenticate);
		}

        private static string EnsureTrailingSlash(string value)
        {
            if (value == null)
            {
                value = string.Empty;
            }

            if (!value.EndsWith("/", StringComparison.Ordinal))
            {
                return value + "/";
            }

            return value;
        }
    }
}
