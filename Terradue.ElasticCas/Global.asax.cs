using Funq;
using System;
using System.Web;
using System.Collections;
using System.ComponentModel;
using System.Web.SessionState;
using ServiceStack;
using ServiceStack.Text;
using ServiceStack.WebHost.Endpoints;
using ServiceStack.Common.Web;
using PlainElastic.Net;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface.ServiceModel;
using System.Net;
using ServiceStack.ServiceModel.Serialization;
using ServiceStack.WebHost.Endpoints.Formats;

namespace Terradue.ElasticCas {


	/// <summary>Global class</summary>
	public class Global : HttpApplication {
		public AppHost appHost;

		/// <summary>Application initialisation</summary>
		protected void Application_Start(object sender, EventArgs e) {
			appHost = new AppHost();
			appHost.Init();

		}

		protected void Application_Error(object sender, EventArgs e) {
			Context.IsErrorResponse();
		}
	}
}

