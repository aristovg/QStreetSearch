Go to https://nominatim.openstreetmap.org/ and find relation ID of the needed territory (City, State)

Generate polygon: http://polygons.openstreetmap.fr/

Get region *.osm from the country file assuming osmosis binaries are installed in bin folder (win32):

bin\osmosis.bat --read-pbf-fast file="ukraine-latest.osm.pbf" --bounding-polygon file="kyivregion.poly" --write-xml file="kyivregion.osm"

Cities and towns:
osmfilter --keep="place=" > region.xml