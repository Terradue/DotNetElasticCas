using System;
using ServiceStack.ServiceHost;
using System.Runtime.Serialization;

namespace Terradue.ElasticCas.Model
{
	[DataContract]
	public class Index
	{

		[DataMember(Name = "name")]
		public string Name {get; set;}



	}
}

