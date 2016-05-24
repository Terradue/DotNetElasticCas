using System;
using NUnit.Framework;
using System.Linq;
using Terradue.ElasticCas.Types;
using System.IO;
using Terradue.ElasticCas.Model;
using Terradue.ElasticCas.Controllers;

namespace Terradue.Elastic.Tests {

    [TestFixture]
    public class IndexTest : IntegrationTest {

        public void CreateIndex(string indexName) {

            var service = base.GetIndexService();
            service.Delete(new Terradue.ElasticCas.Request.DeleteIndexRequest(){ IndexName = indexName });
            var createresponse = service.Put(new Terradue.ElasticCas.Request.CreateIndexRequest(){IndexName = indexName});

            Assert.AreEqual(indexName, createresponse.Name);

            Assert.AreEqual(0, createresponse.Mappings.Count);



        }

        [Test]
        public void DeleteIndex() {

            CreateIndex("test");

            var service = base.GetIndexService();
            var deleteresponse = service.Delete(new Terradue.ElasticCas.Request.DeleteIndexRequest(){ IndexName = "test" });

            Assert.IsTrue(deleteresponse.Acknowledged);

        }

        [Test]
        public void FillIndexWithTestData() {

            CreateIndex("twitter");

            var service = base.GetTypesIngestionService();

            FileStream fstream = new FileStream("../Samples/twitter.json", FileMode.Open, FileAccess.Read);
            var json = GenericJsonCollection.DeserializeFromStream(fstream);

            var ecf = new ElasticCasFactory("test");
            IOpenSearchableElasticType type = ecf.GetOpenSearchableElasticTypeByNameOrDefault("twitter", "tweet");

            var ingestresponse = service.Bulk(type, json);



        }
    }
}

