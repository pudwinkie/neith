#!/bin/sh

xbuild ./appendstdheader/appendstdheader.csproj /property:Configuration=Release
exec mono ./appendstdheader/bin/Release/appendstdheader.exe "$@"
