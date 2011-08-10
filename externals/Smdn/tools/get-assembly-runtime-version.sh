#!/bin/bash
base=$(cd $(dirname $0) && pwd)
src="$base/get-assembly-runtime-version.cs"
exe="$base/get-assembly-runtime-version.exe"

if [ ! -e $exe ];
then
  gmcs $src
elif [ `stat --print=%Y $exe` -lt `stat --print=%Y $src` ];
then
  gmcs $src
fi

exec mono $exe "$@"

