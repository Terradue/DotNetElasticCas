using NUnit.Framework;
using System;
using Mono.Addins;

namespace Terradue.ElasticCas.Tests {

    [TestFixture]
    public class BaseTest {

        protected Exception caughtException = null;
        private const string listeningOn = "http://localhost:8081/";
        private AppHost appHost;
        // extends AppHostHttpListenerBase

        [TestFixtureSetUp]
        public void TestFixtureSetUp() {

            AddinManager.Initialize();
            AddinManager.Registry.Update(null);

        }

        [TearDown]
        public void RunAfterAnyTests() {
            if (caughtException != null)
            {
                Console.WriteLine(string.Format("TestFixtureSetUp failed in {0} - {1}", this.GetType(), caughtException.Message));
            }  
            Console.Out.Flush();
        }

    }
}

