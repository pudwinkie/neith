<?php
/*
 * MindTouch Deki - enterprise collaboration and integration platform
 * Copyright (C) 2006-2009 MindTouch, Inc.
 * www.mindtouch.com  oss@mindtouch.com
 *
 * For community documentation and downloads visit www.opengarden.org;
 * please review the licensing section.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along
 * with this program; if not, write to the Free Software Foundation, Inc.,
 * 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 * http://www.gnu.org/copyleft/gpl.html
 */

// necessary for LocalSettings.php
define('MINDTOUCH_DEKI', true); 

// chdir() will attempt to load LocalSettings.php magically;
// if this fails, you will need to explicitly set the path
chdir($_SERVER['DOCUMENT_ROOT']);
require_once('includes/Defines.php');
require_once('LocalSettings.php');
// we need to access the api, so include some libs for just that
require_once('deki_setup_lite.php');


// guerrics: should these editor paths be defined in DefaultSettings?
$wgEditor = isset($wgEditor) && isset($wgEditors[$wgEditor]) ? $wgEditor : $wgDefaultEditor;
$configDirectory = $IP . '/editor' . $wgEditors[$wgEditor]['directory'] . '/editor';


// create an instance of a remote etag cache
$Cache = new RemoteCacheHandler(EtagCache::TYPE_JAVASCRIPT); // charset=utf-8
// site properties are required for the custom fck config
$SiteProperties = DekiSiteProperties::getInstance();

// load deki's default config file, before user config
$Cache->addFile($configDirectory . '/fckconfig.js');

if ($wgFCKSource == true)
{
	$Cache->addFile($configDirectory . '/dekiplugins.js');
}
else
{
	// use the minified plugins
	$Cache->addFile($configDirectory . '/dekiplugins.min.js');
}

// get the user config from site properties, check the etag
$etag = $SiteProperties->getFckConfigEtag(); 
if (!is_null($etag))
{
	$uri = $SiteProperties->getFckConfigUri();
	$Cache->addResouce($uri, $etag);
}

// create the cache file
$Cache->process();
