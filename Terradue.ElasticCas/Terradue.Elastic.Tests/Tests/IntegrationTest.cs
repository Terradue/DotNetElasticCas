using System;
using Funq;
using Terradue.ElasticCas.Services;
using NUnit.Framework;
using System.Web;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Testing;
using Mono.Addins;

namespace Terradue.Elastic.Tests {

    [TestFixture]
    public class IntegrationTest {

        private Container container;

        [TestFixtureSetUp]
        public void Setup() {

            // Initialize the add-in engine
            try {
                AddinManager.Initialize();
                AddinManager.Registry.Update(null);
            } catch (Exception e) {
                throw e;
            }

            //LoadStaticObject();

            container = new Container();

            // Register the Service so new instances are autowired with your dependencies
            container.RegisterAutoWired<IndexService>();


        }

        public IndexService GetIndexService() {

            var service = container.Resolve<IndexService>();
            service.SetResolver(new BasicResolver(container));

            return service;

        }

        public TypesIngestionService GetTypesIngestionService() {

            var service = container.Resolve<TypesIngestionService>();
            service.SetResolver(new BasicResolver(container));

            return service;

        }

    }
}

