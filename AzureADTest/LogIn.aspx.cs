using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Owin.Builder;
using Owin;

namespace AzureADTest
{
	public partial class LogIn : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			IAppBuilder app = new AppBuilder();

			Startup objStartup = new Startup();
			objStartup.ConfigureAuth(app);
		}
	}
}