using System;
using System.Collections.Specialized;
using Terradue.OpenSearch;
using Nest;
using System.Linq;
using Terradue.ElasticCas.Types;
using System.Xml;
using Terradue.OpenSearch.Schema;
using System.Collections.Generic;

namespace Terradue.ElasticCas.OpenSearch {
    
    public static class OpenSearchHelpers {
        
        public static NameValueCollection GetElasticCasOpenSearchParameters() {

            var osdic = OpenSearchFactory.GetBaseOpenSearchParameter();

            osdic.Set("update", "{dct:modified?}");
            osdic.Set("dlgrp", "{t2:dlGroup?}");

            return osdic;

        }

        public static QueryContainer DescribeFilters(NameValueCollection nvc) {

            QueryContainer query = null;


            if (nvc.AllKeys.Contains("q")) {
                query &= Query<object>.QueryString(q => q.Query(nvc["q"]));
            }

            if (nvc.AllKeys.Contains("update")) {
                query &= DescribeUpdateFilters(nvc);
            }

            if (nvc.AllKeys.Contains("dlgrp")) {
                query &= DescribeDownloadGroupFilters(nvc);
            }

            return query;

        }

        public static QueryContainer DescribeUpdateFilters(NameValueCollection nvc) {

            if (string.IsNullOrEmpty(nvc["update"]))
                return null;
            return Query<IElasticObject>.Filtered(q => q.Filter(fi => fi.Range(r => r.OnField(f => f.Modified).GreaterOrEquals(nvc["update"]))));
        }

        public static QueryContainer DescribeDownloadGroupFilters(NameValueCollection nvc) {

            if (string.IsNullOrEmpty(nvc["dlgrp"]))
                return null;
            return Query<IElasticObject>.Nested(p => p.Path(f => f.Links).Filter(fi => fi.Term(r => r.Links[0].AttributeExtensions[new XmlQualifiedName("group")], nvc["dlgrp"])));
        }

        public static List<OpenSearchDescriptionUrlParameter> GetDefaultParametersDescription(int count) {
            var parameters = OpenSearchFactory.GetDefaultParametersDescription(count);

            var param = new OpenSearchDescriptionUrlParameter();
            param.Name = "update";
            param.Value = "{dct:modified?}";
            param.Title = "date after which dataset are updated (RFC-3339)";
            param.Minimum = "0";

            parameters.Add(param);

            param = new OpenSearchDescriptionUrlParameter();
            param.Name = "dlgroup";
            param.Value = "{t2:dlGroup?}";
            param.Title = "a string that identifies the download group";
            param.Minimum = "0";

            parameters.Add(param);



            return parameters;
        }
    }
}

