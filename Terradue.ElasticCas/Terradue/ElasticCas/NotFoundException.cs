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

namespace Terradue.ElasticCas
{
	public class NotFoundException : Exception
	{

		public NotFoundException(string message) : base(message) {}

	}
}

