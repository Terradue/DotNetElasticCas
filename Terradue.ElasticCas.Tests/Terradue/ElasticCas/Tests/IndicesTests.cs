using System;
using NUnit.Framework;
using Terradue.ElasticCas.Request;
using Terradue.ElasticCas.Services;
using Newtonsoft.Json;
using Terradue.ElasticCas.Responses;

namespace Terradue.ElasticCas.Tests {
    public class IndicesTests : BaseTest {

        IndexService service;

        [TestFixtureSetUp]
        public void InitIndicesServices(){

            try {
                service = new IndexService();
            } catch ( Exception ex ){
                caughtException = ex; 
            }
        }

        [Test]
        public void CreateIndex(){

            CreateIndexRequest request = new CreateIndexRequest();
            request.IndexName = "testindex";

            IndexInformation response = service.Put(request);

            Assert.That(response.Name == "testindex");

            Console.Out.Write(JsonConvert.SerializeObject(response));

        }

        [Test]
        public void DeleteIndex(){

            DeleteIndexRequest request = new DeleteIndexRequest();
            request.IndexName = "testindex";

            var response = service.Delete(request);

        }
    }
}

