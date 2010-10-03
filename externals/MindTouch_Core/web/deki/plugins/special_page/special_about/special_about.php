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
	DekiPlugin::registerHook(Hooks::SPECIAL_ABOUT, array('SpecialAbout', 'create'));
}

class SpecialAbout extends SpecialPagePlugin
{
	protected $pageName = 'About';

	
	public static function create($pageName, &$pageTitle, &$html)
	{
		$Special = new self($pageName, basename(__FILE__, '.php'));
		
		// set the page title
		$pageTitle = $Special->getPageTitle();
		$html = $Special->output();
	}
	
	public function &output()
	{
		$html = wfMsg('Page.About.product', DekiSite::getProductLink());
		$html.= wfMsg('Page.About.credits', '<a href="http://www.famfamfam.com/lab/icons/silk/">famfamfam.com</a>');
		$html.= wfMsg('Page.About.getting-help',
			'<a href="http://developer.mindtouch.com">'. wfMsg('Skin.Common.mt-dev') .'</a>',
			'<a href="http://developer.mindtouch.com">'. 'wiki' .'</a>',
			'<a href="http://forums.developer.mindtouch.com">'. 'forums' .'</a>'
		);
		
		return $html;
	}
}
