# ElasticCas

ElasticCas is an **OpenSearch** engine built over **elasticsearch**.

## What is OpenSearch ?

OpenSearch is a collection of simple formats for the sharing of search results. [http://www.opensearch.org/](http://www.opensearch.org/) 

## What is elasticsearch ?

Elasticsearch is a flexible and powerful open source, distributed, real-time search and analytics engine. [http://www.elasticsearch.org/](http://www.elasticsearch.org/)

## How does it work ?

ElasticCas put documents collection in elasticsearch that stores complex real world entities as structured JSON documents.
```js
$ curl -XPOST 'http://localhost:8082/catalogue/twitter/tweet' -d '{
    "items":[
    	{
		    "user" : "manu",
    		"post_date" : "2014-09-24T15:10:12",
	    	"message" : "trying out ElasticCas"
        },
        {
		    "user" : "bob",
    		"post_date" : "1973-05-24T12:45:33",
	    	"message" : "Knockin' on Heaven's Door"
        }
    ]   
}'
```

All fields are indexed by default so that an OpenSearch Description is built dynamically anyhow.
```js
$ curl 'http://localhost:8082/catalogue/twitter/tweet/description'
```

```xml
<OpenSearchDescriptionOpenSearchDescription xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns="http://a9.com/-/spec/opensearch/1.1/">
	<ShortName>twitter Elastic Catalogue</ShortName>
	<Description>This Search Service performs queries in the index twitter. There are several URL templates that return the results in different formats.This search service is in accordance with the OGC 10-032r3 specification.</Description>
	<Contact>info@terradue.com</Contact>
	<Url type="application/atom+xml" template="http://127.0.0.1:8081/catalogue/twitter/tweet/search?format=atom&count={count?}&startPage={startPage?}&startIndex={startIndex?}&q={searchTerms?}&lang={language?}" rel="results" pageOffset="1" indexOffset="1" />
	<Url type="application/json" template="http://127.0.0.1:8081/catalogue/twitter/tweet/search?format=json&count={count?}&startPage={startPage?}&startIndex={startIndex?}&q={searchTerms?}&lang={language?}" rel="results" pageOffset="1" indexOffset="1" />
	<Url type="application/opensearchdescription+xml" template="http://127.0.0.1:8081/catalogue/twitter/tweet/description" rel="self" pageOffset="1" indexOffset="1" />
	<Developer>Terradue Development Team</Developer>
	<Attribution>Terradue</Attribution>
	<SyndicationRight>open</SyndicationRight>
	<AdultContent>false</AdultContent>
	<Language>en-us</Language>
	<OutputEncoding>UTF-8</OutputEncoding>
	<InputEncoding>UTF-8</InputEncoding>
	<DefaultUrl type="application/atom+xml" template="http://127.0.0.1:8081/catalogue/twitter/tweet/search?format=atom&count={count?}&startPage={startPage?}&startIndex={startIndex?}&q={searchTerms?}&lang={language?}" rel="results" pageOffset="1" indexOffset="1" />
</OpenSearchDescription>
```

From OpenSearch query and its parameters, ElasticCas builds simple or complex queries according to the document type (plugins).
```js
$ curl -XGET 'http://localhost:8082/catalogue/twitter/tweet/search?q=bob&format=json'
```

Finally, it returns results in various formats (plugins).
```js
{
    "items":[
    	{
		    "user" : "manu",
    		"post_date" : "2014-09-24T15:10:12",
	    	"message" : "trying out ElasticCas"
        }
    ]   
}
```

## Plugins and Extensibility

ElasticCas is the chassis for building a domain specific complex distributed restful search and analytics system. Alone, it provides the full basic OpenSearch functionnalities such as the free text query powered with elasticsearch capabilities
```
http://localhost:8082/catalogue/tweeter/tweet/search?q=message:Heaven
```

Pluging extensions into ElasticCas brings a new format, document type, or search functionnality.
Find all ElasticCas plugins:
- Type : https://github.com/Terradue?query=DotNetElasticCas
- Format : https://github.com/Terradue?query=DotNetOpenSearch

## Getting started

Terradue.ElasticCas is available as NuGet package in releases.

Install-Package Terradue.ElasticCas

## Build

Terradue.ElasticCas repository is a solution designed to be easily deployed anywhere. 

To compile it yourself, you’ll need:

* Visual Studio 2012 or later, or Xamarin Studio

To clone it locally click the "Clone in Desktop" button above or run the 
following git commands.

```
git clone git@github.com:Terradue/Terradue.ElasticCas.git Terradue.ElasticCas
```

### Questions, bugs, and suggestions

Please file any bugs or questions as [issues](https://github.com/Terradue/DotNetElasticCas/issues/new) 

### Want to contribute

Fork the repository [here](https://github.com/Terradue/DotNetElasticCas/fork) and send us pull requests.

### Copyright and License

Copyright (c) 2014 Terradue

Licensed under the GPL v2 License
