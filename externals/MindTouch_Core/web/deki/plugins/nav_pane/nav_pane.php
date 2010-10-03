<?php
/*
 * MindTouch Deki - enterprise collaboration and integration platform
 * Copyright (C) 2006-2009 MindTouch, Inc.
 * www.mindtouch.com  oss@mindtouch.com
 *
 * For community documentation and downloads visit www.opengarden.org;
 *  please review the licensing section.
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

if (defined('MINDTOUCH_DEKI')) :

DekiPlugin::registerHook(Hooks::SKIN_NAVIGATION_PANE, 'wfSkinNavigationPane');
DekiPlugin::registerHook(Hooks::MAIN_PROCESS_OUTPUT, 'wfSkinNavigationIncludes');

function wfSkinNavigationIncludes()
{
	$type = strtolower(wfGetConfig('ui/nav-type', 'compact'));
    switch ($type)
    {
		case 'none':
		
			// no navigation tree, nothing to do
			return;
		case 'expandable':
		
			// expandable AJAX tree
			DekiPlugin::includeCss('nav_pane', 'expandablenav.css');
			DekiPlugin::includeJavascript('nav_pane', 'expandablenav.js');
			break;
			
		case 'compact':
		default:
		
			// original compact tree
			DekiPlugin::includeJavascript('nav_pane', 'nav.js');
			break;
    }
}

/*
 * @param $Title - currently requested title object
 * @param string &$html - markup to embed into the skin
 */
function wfSkinNavigationPane($Title, &$html)
{
	global $wgDekiPlug, $wgNavPaneWidth, $wgNavPaneCallEnabled;
	$type = strtolower(wfGetConfig('ui/nav-type', 'compact'));

	// check if tree is disabled
	if (($wgNavPaneCallEnabled === false) || ($type === 'none')) 
	{
		return '';	
	}
	
	$pageTitle = $Title->getPrefixedText() == wfHomePageInternalTitle() 
		? 'home'
		: '='.urlencode(urlencode($Title->getPrefixedText())); //double urlencode is for mod_proxy
	
	$result = $wgDekiPlug
		->At('site', 'nav', $pageTitle, 'full')
		->With('width', $wgNavPaneWidth)
		->With('type', $type)
		->Get();
	if ($result['status'] == 200)
	{
		$html =
		// add the pane width via javascript
		'<script type="text/javascript">'.
			'var navMaxWidth = '.(int)$wgNavPaneWidth .';'.
			(($type === 'compact') ? 'YAHOO.util.Event.onAvailable("siteNavTree", Deki.nav.init, Deki.nav, true);' : '').
		'</script>'.
		'<div id="siteNavTree">'.
			$result['body']['tree'].
		'</div>';
	}
	else
	{
		$html = '<div id="siteNavTree"></div>';
	}
}

endif;
