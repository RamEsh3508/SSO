using System;
using System.IO;
using System.Linq;
using Owin;
using SAML2.Config;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Owin.Security.Cookies;
using Owin.Security.Saml;

namespace AzureADTest
{
	public class Saml_Owin_Configuration
	{
		public void CreateConfig(IAppBuilder appBuilder)
		{
			var config = GetSamlConfiguration();

			appBuilder.UseCookieAuthentication(new CookieAuthenticationOptions
			                                   {
				                                   AuthenticationType = "SAML2",
				                                   AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Active
			                                   });
			appBuilder.UseSamlAuthentication(new SamlAuthenticationOptions
			                                 {
				                                 Configuration = config,
				                                 RedirectAfterLogin = "/core"
			                                 });
			appBuilder.Run(async c =>
			               {
				               if(c.Authentication.User?.Identity != null && c.Authentication.User.Identity.IsAuthenticated)
				               {
					               await c.Response.WriteAsync(c.Authentication.User.Identity.Name + "\r\n");
					               await c.Response.WriteAsync(c.Authentication.User.Identity.AuthenticationType + "\r\n");
					               foreach(var claim in c.Authentication.User.Identities.SelectMany(i => i.Claims))
						               await c.Response.WriteAsync(claim.Value + "\r\n");
					               await c.Response.WriteAsync("authenticated");
				               }
				               else
				               {
					               c.Authentication.Challenge(c.Authentication.GetAuthenticationTypes().Select(d => d.AuthenticationType).ToArray());
				               }
			               });
		}

		private Saml2Configuration GetSamlConfiguration()
		{
			var myconfig = new Saml2Configuration
			               {
				               ServiceProvider = new ServiceProvider
				                                 {
					                                 SigningCertificate = new X509Certificate2(FileEmbeddedResource("Sustainsys.Saml2.Tests.pfx")),
					                                 Server = "https://localhost:44358/",
					                                 Id = "8b67b292-ebf3-4d29-89a6-47f7971c2e16"
							   },
				               AllowedAudienceUris = new System.Collections.Generic.List<Uri>(new[] { new Uri("https://localhost:44358/") })
			               };
			myconfig.ServiceProvider.Endpoints.AddRange(new[] {
				                                                  new ServiceProviderEndpoint(EndpointType.SignOn, "/core/saml2/login"),
				                                                  new ServiceProviderEndpoint(EndpointType.Logout, "/core/saml2/logout"),
				                                                  new ServiceProviderEndpoint(EndpointType.Metadata, "/core/saml2/metadata")
			                                                  });
			myconfig.IdentityProviders.AddByMetadataDirectory("..\\..\\Metadata");
			//myconfig.IdentityProviders.AddByMetadataUrl(new Uri("https://tas.fhict.nl/identity/saml2/metadata"));
			//myconfig.IdentityProviders.First().OmitAssertionSignatureCheck = true;
			myconfig.LoggingFactoryType = "SAML2.Logging.DebugLoggerFactory";
			return myconfig;
		}

		private byte[] FileEmbeddedResource(string path)
		{
			var assembly = System.Reflection.Assembly.GetExecutingAssembly();
			var resourceName = path;

			byte[] result;
			using(Stream stream = assembly.GetManifestResourceStream(resourceName))
			using(var memoryStream = new MemoryStream())
			{
				stream?.CopyTo(memoryStream);
				result = memoryStream.ToArray();
			}
			return result;
		}
	}
}