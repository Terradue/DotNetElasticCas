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
```xml
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
From OpenSearch query and its parameters, ElasticCas builds simple or complex queries according to the document type (plugins).
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
Finally, it returns results in various formats (plugins).

## Plugins and Extensibility

ElasticCas is the chassis for building a domain specific complex distributed restful search and analytics system. Alone, it provides the full basic OpenSearch functionnalities such as the free text query powered with elasticsearch capabilities
```
http://localhost:8082/catalogue/tweeter/tweet/search?q=tag:wow
```

Pluging extensions into ElasticCas brings a new format, document type, or search functionnality.



## Getting started


### Build


### Questions, bugs, and suggestions

Please file any bugs or questions as [issues](https://github.com/Terradue/DotNetElasticCas/issues/new) 

### Want to contribute

Fork the repository [here](https://github.com/Terradue/DotNetElasticCas/fork) and send us pull requests.

### Copyright and License

Copyright (c) 2014 Terradue

Licensed under the GPL v2 License
