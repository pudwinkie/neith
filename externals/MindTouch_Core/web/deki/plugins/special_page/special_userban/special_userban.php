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

if (defined('MINDTOUCH_DEKI'))
{
	DekiPlugin::registerHook(Hooks::SPECIAL_USER_BAN, 'wfSpecialUserBan');
}

function wfSpecialUserBan($pageName, &$pageTitle, &$html)
{
	global $wgUser;
	
	if (!$wgUser->isAdmin()) {
		DekiMessage::error(wfMsg('Page.Userban.error.user'));
		if (!empty($_SERVER['HTTP_REFERER'])) {
			global $wgOut;
			$wgOut->redirect($_SERVER['HTTP_REFERER']);	
		}
		return;	
	}
	$Request = DekiRequest::getInstance();
	if ($Request->getVal('username')) 
	{
		global $wgOut;
		$wgOut->redirect('/deki/cp/bans.php?params=add&user='.urlencode($Request->getVal('username')).($Request->getVal('returnto') ? '&returnto='.urlencode($Request->getVal('returnto')): ''));
		return;
	}
}