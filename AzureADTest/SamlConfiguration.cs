using System;
using System.Security.Cryptography.X509Certificates;
using System.Web.Hosting;
using Sustainsys.Saml2;
using Sustainsys.Saml2.Configuration;
using Sustainsys.Saml2.Owin;
using Sustainsys.Saml2.WebSso;
using Sustainsys.Saml2.Metadata;

namespace AzureADTest
{
	public class SamlConfiguration
	{
		public Saml2AuthenticationOptions CreateSaml2Options()
        {
            string samlIdpUrl = "http://localhost:44358/";
            string x509FileNamePath = "~/App_Data/AzureADTest.cer";
 
            var spOptions = CreateSPOptions();
            var Saml2Options = new Saml2AuthenticationOptions(false)
            {
                SPOptions = spOptions
            };
 
            var idp = new IdentityProvider(new EntityId("https://sts.windows.net/8b67b292-ebf3-4d29-89a6-47f7971c2e16/"), spOptions)
            {
                AllowUnsolicitedAuthnResponse = true,
                Binding = Saml2BindingType.HttpRedirect,
                SingleSignOnServiceUrl = new Uri(samlIdpUrl)
            };
 
            idp.SigningKeys.AddConfiguredKey(
                new X509Certificate2(HostingEnvironment.MapPath(x509FileNamePath)));
 
            Saml2Options.IdentityProviders.Add(idp);
            new Federation(samlIdpUrl, true, Saml2Options);
 
            return Saml2Options;
        }
 
        private static SPOptions CreateSPOptions()
        {
            string strEntityID = "https://sts.windows.net/8b67b292-ebf3-4d29-89a6-47f7971c2e16/";
            string strServiceProviderReturnUrl = "https://ramesh.knowledgeowl.com/help/saml-login";
            string strPfxFilePath = "/App_Data/Sustainsys.Saml2.Tests.pfx";
            string strSamlIdpOrgName = "AzureADTest";
            string strSamlIdpOrgDisplayName = "AzureADTest";
 
            var swedish = "sv-se";
            var organization = new Organization();
            organization.Names.Add(new LocalizedName(strSamlIdpOrgName, swedish));
            organization.DisplayNames.Add(new LocalizedName(strSamlIdpOrgDisplayName, swedish));
            organization.Urls.Add(new LocalizedUri(new Uri("http://www.Sustainsys.se"), swedish));
 
            var spOptions = new SPOptions
            {
                EntityId = new EntityId(strEntityID),
                ReturnUrl = new Uri(strServiceProviderReturnUrl),
                Organization = organization
            };
         
            var attributeConsumingService = new AttributeConsumingService
            {
                IsDefault = true,
            };
 
            attributeConsumingService.RequestedAttributes.Add(
                new RequestedAttribute("urn:someName")
                {
                    FriendlyName = "Some Name",
                    IsRequired = true,
                    NameFormat = RequestedAttribute.AttributeNameFormatUri
                });
 
            attributeConsumingService.RequestedAttributes.Add(
                new RequestedAttribute("Minimal"));
 
            spOptions.AttributeConsumingServices.Add(attributeConsumingService);
 
            spOptions.ServiceCertificates.Add(new X509Certificate2(
                AppDomain.CurrentDomain.SetupInformation.ApplicationBase + strPfxFilePath));
 
            return spOptions;
        }
	}
}