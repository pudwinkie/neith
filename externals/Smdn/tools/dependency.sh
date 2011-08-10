#!/bin/sh
DIR=$(cd $(dirname $0) && pwd)

xbuild $DIR/dependency/dependency.csproj /property:Configuration=Release
exec mono $DIR/dependency/bin/Release/dependency.exe $DIR/../
