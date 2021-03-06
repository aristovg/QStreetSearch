Go to https://nominatim.openstreetmap.org/ and find relation ID of the needed territory (City, State)

Generate polygon: http://polygons.openstreetmap.fr/

Get the region *.osm file from the country file assuming osmosis binaries are installed in bin folder (win32):
```
bin\osmosis.bat --read-pbf-fast file="ukraine-latest.osm.pbf" --bounding-polygon file="kyivregion.poly" --write-xml file="kyivregion.osm"
```
Cities and towns:

```
osmfilter kyivregion.osm --keep= --keep-nodes="place= and name=" | osmconvert - --csv="name name:ru old_name old_name:ru @lat @lon" > region.csv
```

Streets (the output is an XML file of ways and nodes):

```
osmfilter kyiv.osm --keep="highway=* and name=*"  > output.xml
```
