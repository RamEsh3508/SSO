using System;
using System.Configuration;
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
            string samlIdpUrl = ConfigurationManager.AppSettings["SAML_IDP_URL"];
            string x509FileNamePath = ConfigurationManager.AppSettings["x509_File_Path"];
 
            var spOptions = CreateSPOptions();
            var Saml2Options = new Saml2AuthenticationOptions(false)
            {
                SPOptions = spOptions
            };
 
            var idp = new IdentityProvider(new EntityId(samlIdpUrl + "Metadata"), spOptions)
            {
                AllowUnsolicitedAuthnResponse = true,
                Binding = Saml2BindingType.HttpRedirect,
                SingleSignOnServiceUrl = new Uri(samlIdpUrl)
            };
 
            idp.SigningKeys.AddConfiguredKey(
                new X509Certificate2(HostingEnvironment.MapPath(x509FileNamePath)));
 
            Saml2Options.IdentityProviders.Add(idp);
            new Federation(samlIdpUrl + "Federation", true, Saml2Options);
 
            return Saml2Options;
        }
 
        private static SPOptions CreateSPOptions()
        {
            string entityID = ConfigurationManager.AppSettings["Entity_ID"];
            string serviceProviderReturnUrl = ConfigurationManager.AppSettings["ServiceProvider_Return_URL"];
            string pfxFilePath = ConfigurationManager.AppSettings["Private_Key_File_Path"];
            string samlIdpOrgName = ConfigurationManager.AppSettings["SAML_IDP_Org_Name"];
            string samlIdpOrgDisplayName = ConfigurationManager.AppSettings["SAML_IDP_Org_Display_Name"];
 
            var swedish = "sv-se";
            var organization = new Organization();
            organization.Names.Add(new LocalizedName(samlIdpOrgName, swedish));
            organization.DisplayNames.Add(new LocalizedName(samlIdpOrgDisplayName, swedish));
            organization.Urls.Add(new LocalizedUri(new Uri("http://www.Sustainsys.se"), swedish));
 
            var spOptions = new SPOptions
            {
                EntityId = new EntityId(entityID),
                ReturnUrl = new Uri(serviceProviderReturnUrl),
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
                AppDomain.CurrentDomain.SetupInformation.ApplicationBase + pfxFilePath));
 
            return spOptions;
        }
	}
}