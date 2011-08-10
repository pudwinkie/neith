#!/bin/bash
base=$(cd $(dirname $0) && pwd)
libdir=$1
libname=$(basename $(cd $libdir && pwd))
bindir='build-release'
build_options="/t:Clean;Build /p:Configuration=Release"

xbuild $libdir/Test/$libname.Tests.csproj $build_options
nunit-console2 $libdir/Test/bin/Release/$libname.Tests.dll -out $libdir/test-out.txt -err $libdir/test-err.txt -labels
xbuild $libdir/$libname.csproj $build_options /p:OutputPath=$bindir
version=`$base/get-assembly-version.sh "$libdir/$bindir/$libname.dll"`
(cd $libdir/$bindir; tar -czvf "../$libname-$version-bin.tar.gz" .)

clibdir="$libdir/combined"

if test -e $clibdir ; then
  xbuild $clibdir/$libname-Combined.Tests.csproj $build_options
  nunit-console2 $clibdir/bin/Release/*.Tests.dll -out $libdir/test-combined-out.txt -err $libdir/test-combined-err.txt -labels
  xbuild $clibdir/$libname-Combined.csproj $build_options /p:OutputPath=$bindir
  (cd $clibdir/$bindir; tar -czvf "../../$libname-$version-bin-combined.tar.gz" .)
fi

if test -e "$libdir/test-err.txt" ; then
  echo "### test error"
  cat $libdir/test-err.txt
fi
if test -e "$libdir/test-combined-err.txt" ; then
  echo "### test error (combined)"
  cat $libdir/test-err.txt
fi

