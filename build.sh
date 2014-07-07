#/bin/sh

cd Terradue.OpenSearch.GeoJson
xbuild
cd -
cd Terradue.OpenSearch.Kml
xbuild
cd -
cd Terradue.ElasticCas.EarthObservation
xbuild
cd -
xbuild
