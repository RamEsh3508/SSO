using System;
using System.Configuration;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using AzureADTest.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security.ActiveDirectory;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.WsFederation;
using Sustainsys.Saml2;
using Sustainsys.Saml2.Configuration;
using Sustainsys.Saml2.Metadata;
using Sustainsys.Saml2.Owin;
using Sustainsys.Saml2.WebSso;

namespace AzureADTest
{
    public partial class Startup
	{
        private static string strClientId = "18566f3c-966e-49f0-9d32-92edaa0bdd6e";
        private static string strAppKey = "uF1gXcacJG5TaOHDbhTyuctQTuImUiE5fIOv28esiIU=";
        private static string strInstance = "https://login.microsoftonline.com/";
        private static string strTenantId = "8b67b292-ebf3-4d29-89a6-47f7971c2e16";
        private static string strRedirectUri = "https://localhost:44358/";

        private readonly string strAuthority = strInstance + strTenantId;
        private static string strGraphResourceId = "https://graph.windows.net";

        public void ConfigureAuth(IAppBuilder app)
        {
			OpenIdConfiguration(app);

			//SamlConfiguration(app);

			//Saml_Owin_Configuration objSamlOwinConfiguration = new Saml_Owin_Configuration();
			//objSamlOwinConfiguration.CreateConfig(app);

			//OAuthConfiguration(app);

			//WSFederation(app);
        }

        private void OpenIdConfiguration(IAppBuilder app)
        {
			app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

			app.UseCookieAuthentication(new CookieAuthenticationOptions());

			app.UseOpenIdConnectAuthentication
			(
				new OpenIdConnectAuthenticationOptions
				{
					ClientId = strClientId,
					Authority = strAuthority,
					PostLogoutRedirectUri = strRedirectUri
				}
			);

			app.UseStageMarker(PipelineStage.Authenticate);
		}

        private void SamlConfiguration(IAppBuilder app)
        {
			var cookieOptions = new CookieAuthenticationOptions
			                    {
				                    LoginPath = new PathString("/Account/Login"),
				                    AuthenticationType = "Application",
				                    AuthenticationMode = AuthenticationMode.Passive
			                    };

			app.UseCookieAuthentication(cookieOptions);

			app.SetDefaultSignInAsAuthenticationType(cookieOptions.AuthenticationType);

			app.UseSaml2Authentication(CreateSaml2Options());

			app.UseStageMarker(PipelineStage.Authenticate);
		}
		
        private Saml2AuthenticationOptions CreateSaml2Options()
        {
	        var spOptions = new SPOptions
	                        {
								EntityId = new EntityId("https://sts.windows.net/8b67b292-ebf3-4d29-89a6-47f7971c2e16/"),
								ReturnUrl = new Uri("https://localhost:44358/"),
	                        };

	        var attributeConsumingService = new AttributeConsumingService
	                                        {
		                                        IsDefault = true,
		                                        ServiceNames = { new LocalizedName("Saml2", "en") }
	                                        };

	        attributeConsumingService.RequestedAttributes.Add(
	                                                          new RequestedAttribute("urn:password")
	                                                          {
		                                                          FriendlyName = "AzureADTest",
		                                                          IsRequired = true,
		                                                          NameFormat = RequestedAttribute.AttributeNameFormatUri
	                                                          });

	        attributeConsumingService.RequestedAttributes.Add(
	                                                          new RequestedAttribute("Minimal"));

	        spOptions.AttributeConsumingServices.Add(attributeConsumingService);

			var Saml2Options = new Saml2AuthenticationOptions(false)
	                           {
		                           SPOptions = spOptions
	                           };

	        var idp = new IdentityProvider(new EntityId("https://sts.windows.net/8b67b292-ebf3-4d29-89a6-47f7971c2e16/"), spOptions)
			          {
				          AllowUnsolicitedAuthnResponse = true,
				          Binding = Saml2BindingType.HttpRedirect,
				          SingleSignOnServiceUrl = new Uri("https://localhost:44358/")
			          };

			idp.SigningKeys.AddConfiguredKey(new X509Certificate2(HostingEnvironment.MapPath("~/App_Data/AzureADTest.cer") ?? throw new InvalidOperationException()));

			Saml2Options.IdentityProviders.Add(idp);

			return Saml2Options;
		}

        private void OAuthConfiguration(IAppBuilder app)
        {
	        app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions()
	                                        {
		                                        AllowInsecureHttp = true,
		                                        TokenEndpointPath = new PathString("/token"),
		                                        AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(30),
		                                        Provider = new OAuthAuthorizationServerProvider
                                               {
                                                   OnValidateClientAuthentication = async c => c.Validated(),
                                                   OnGrantResourceOwnerCredentials = async c =>
	                                                 {
	                                                     var identity = new ClaimsIdentity(c.Options.AuthenticationType);
	                                                     identity.AddClaims(new[] { new Claim(ClaimTypes.Name, c.UserName), new Claim(ClaimTypes.Role, "user") });
	                                                     identity.AddClaims(new[] { new Claim(ClaimTypes.Name, c.UserName), new Claim(ClaimTypes.Role, "manager") });
	                                                     c.Validated(identity);
	                                                 }
                                               },
	                                        });

	        app.UseStageMarker(PipelineStage.Authenticate);
		}

        private void WSFederation(IAppBuilder app)
        {
	        const string strRealm = "https://MSAzure01.onmicrosoft.com/AzureADTest";
			const string strInstance = "https://login.microsoftonline.com";
			const string strTenant = "MSAzure01.onmicrosoft.com";
			string strMetadata = $"{strInstance}/{strTenant}/federationmetadata/2007-06/federationmetadata.xml";

			app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

	        app.UseCookieAuthentication(new CookieAuthenticationOptions());

	        app.UseWsFederationAuthentication
		    (
              new WsFederationAuthenticationOptions
              {
                  Wtrealm = strRealm,
                  MetadataAddress = strMetadata
              }
            );

	        app.UseStageMarker(PipelineStage.Authenticate);
		}
	}
}
