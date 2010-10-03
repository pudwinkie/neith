<?php
# Deki Wiki web-based config/installation
# http://www.mindtouch.com/
#
# This program is free software; you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation; either version 2 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License along
# with this program; if not, write to the Free Software Foundation, Inc.,
# 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
# http://www.gnu.org/copyleft/gpl.html

$wgIsVM = false;
$wgIsMSI = false;
$wgIsEnterprise = false;

function wfSetDefaultPaths() {
	global $wgPathIdentify, $wgPathConvert, $wgPathHTML2PS, $wgPathPS2PDF, $wgPathMono, $wgPathConf;
	global $wgIsVM, $wgIsMSI, $wgAttachPath, $wgLucenePath, $wgPathPrince;
	global $IP;
	
	$wgPathIdentify = null;
	$wgPathConvert = null;
	$wgPathHTML2PS = null;
	$wgPathPS2PDF = null;
	$wgPathPrince = null;
	$wgPathMono = null;
	$wgAttachPath = $IP.'/attachments'; // absolute path used for API config key 'storage/fs/path'
	$wgLucenePath = $IP.'/bin/cache/luceneindex/$1';
	
	// any settings specific to a windows installs
	if (wfIsWindows()) 
	{
		$wgLucenePath = '\\bin\\cache\\luceneindex\\\$1';
	}
	
	// if we're running a VM, set the default values
	if ($wgIsVM) 
	{
		$wgPathIdentify = '/usr/bin/identify';
		$wgPathConvert = '/usr/bin/convert';
		$wgPathMono = '/usr/bin/mono';
		$wgPathPrince = '/usr/bin/prince';
	}
	
	// if we're running a MSI, set the default values
	if ($wgIsMSI) 
	{
		$wgPathIdentify = realpath('../../api/bin/identify.exe');
		$wgPathConvert = realpath('../../api/bin/convert.exe');
		$wgPathConf = realpath('../../api/bin');
        $wgAttachPath = realpath('../../data/files');
        $wgLucenePath = realpath('../../data/index').'\\$1';
        $wgPathPrince = realpath('../../redist/Prince/engine/bin/prince.exe');
	}
}
?>
