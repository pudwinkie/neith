#!/bin/bash
basedir=$(cd $(dirname $0) && pwd)
workdir=`pwd`
binoutdir="${workdir}/.build-temp"
clean_options="/t:Clean /p:Configuration=Release"
build_options="/t:Build /p:Configuration=Release"
exec_get_assm_version="$basedir/get-assembly-version.sh"
exec_get_assm_runtime_version="$basedir/get-assembly-runtime-version.sh"

for sln in $*; do
  if [ -e $binoutdir ];
  then
    rm -rf $binoutdir
  fi

  sln_file=${sln##*/}
  sln_filename=${sln_file%.*}
  sln_trunkname=${sln_filename%%-*}
  sln_suffix=${sln_filename#$sln_trunkname}

  # version specific parameters
  sln_runtime_version_suffix=${sln_suffix%-combined}

  if [ $sln_runtime_version_suffix = "-netfx4.0" ]
  then
    # netfx4.0
    env_file='/opt/mono/2.8/env'
    expected_runtime_version='v4.0.30319'
  else
    # netfx2.0, netfx3.5
    env_file='/opt/mono/2.6/env'
    expected_runtime_version='v2.0.50727'
  fi

  # clean and build
  bindir="${binoutdir}/${sln_filename}"

  (. $env_file; xbuild $sln $clean_options)
  (. $env_file; xbuild $sln $build_options "/p:OutputPath=$bindir")

  outassm="$bindir/$sln_trunkname.dll"

  if [ ! -e $outassm ]
  then
    outassm="${outassm%%.dll}.exe"
  fi

  # check assembly image runtime version
  runtime_version=`$exec_get_assm_runtime_version "$outassm"`

  if [ -n "${runtime_version#$expected_runtime_version}" ]
  then
    echo "invalid runtime version: $runtime_version (expected=$expected_runtime_version)"
    exit
  fi

  # create package
  version=`$exec_get_assm_version "$outassm"`

  packagename="${sln_trunkname}-${version}${sln_suffix}"

  (cd $bindir && zip -r ${workdir}/${packagename}.zip *)
done

rm -rf $binoutdir
