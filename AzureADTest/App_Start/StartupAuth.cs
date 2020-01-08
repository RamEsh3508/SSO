﻿using System;
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

namespace AzureADTest
{
    public partial class Startup
    {
        private static string strClientId = "18566f3c-966e-49f0-9d32-92edaa0bdd6e";
        private static string strAppKey = "uF1gXcacJG5TaOHDbhTyuctQTuImUiE5fIOv28esiIU=";
        private static string strInstance = "https://login.microsoftonline.com/";
        private static string strTenantId = "8b67b292-ebf3-4d29-89a6-47f7971c2e16";
        private static string strRedirectUri = "https://localhost:44358/";

        private string strAuthority = strInstance + strTenantId;
        private static string strGraphResourceId = "https://graph.windows.net";

        public void ConfigureAuth(IAppBuilder app)
        {
	        app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

			app.UseCookieAuthentication(new CookieAuthenticationOptions());

			app.UseOpenIdConnectAuthentication(
				new OpenIdConnectAuthenticationOptions
					{
						ClientId = strClientId,
						Authority = strAuthority,
						PostLogoutRedirectUri = strRedirectUri,
						Notifications = new OpenIdConnectAuthenticationNotifications()
						{
							AuthorizationCodeReceived = (context) =>
							{
								var strCode = context.Code;
								ClientCredential credential = new ClientCredential(strClientId, strAppKey);
								string strSignedInUserID = context.AuthenticationTicket.Identity.FindFirst(ClaimTypes.NameIdentifier).Value;
								AuthenticationContext authContext = new AuthenticationContext(strAuthority, new ADALTokenCache(strSignedInUserID));
								AuthenticationResult result = authContext.AcquireTokenByAuthorizationCodeAsync(
								  strCode, new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)), credential, strGraphResourceId).Result;

								return Task.FromResult(0);
							}
						}
					}
				);

			app.UseStageMarker(PipelineStage.Authenticate);

			SamlConfiguration objSamlConfiguration = new SamlConfiguration();
			//app.UseSaml2Authentication(objSamlConfiguration.CreateSaml2Options());
		}
    }
}
