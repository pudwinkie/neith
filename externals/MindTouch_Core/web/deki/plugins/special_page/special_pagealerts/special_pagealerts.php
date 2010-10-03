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
	DekiPlugin::registerHook(Hooks::SPECIAL_PAGE_ALERTS, 'wfSpecialPageAlerts');
}

function wfSpecialPageAlerts($pageName, &$pageTitle, &$html)
{
	$Special = new SpecialPageAlerts($pageName, basename(__FILE__, '.php'));
	
	// set the page title
	$pageTitle = $Special->getPageTitle();
	$html = $Special->output();	
}

class SpecialPageAlerts extends SpecialPagePlugin
{
	protected $pageName = 'PageAlerts';

	public function output()
	{
		$html = '';
		
		// add some css
		$this->includeSpecialCss('special_pagealerts.css');
		
		$Request = DekiRequest::getInstance();
		
		$pageId = $Request->getInt('id', 0);
		// attempt to fetch the page information
		$Title = Title::newFromId($pageId);		
		
		if (is_null($Title))
		{
			self::redirectHome();
			return;
		}
		
		// make sure the user is not anonymous
		$User = DekiUser::getCurrent();
		if ($User->isAnonymous())
		{
			// notify the user that they must be logged in first
			DekiMessage::error(wfMsg('Page.PageAlerts.error.anonymous'));
			
			$UserLogin = Title::newFromText('UserLogin', NS_SPECIAL);
			self::redirectTo($UserLogin, $Title);
			return;
		}

		// load the requested article
		$Article = new Article($Title);
		// determine alert status
		$Alert = new DekiPageAlert($Article->getId(), $Article->getParentIds());
		// below performs the actual api hit
		$status = $Alert->getStatus();
		
		if ($Request->isPost())
		{
			do 
			{
				$status = $Request->getVal('status');
				$options = array(
					DekiPageAlert::STATUS_OFF,
					DekiPageAlert::STATUS_SELF,
					DekiPageAlert::STATUS_TREE
				);
				$status = $Request->getEnum('status', $options, DekiPageAlert::STATUS_OFF);
	
				// set the alert status
				$Result = $Alert->setStatus($status);
	
				if (is_null($Result))
				{
					// no change, counts as a success
				}
				else if (!$Result->handleResponse())
				{
					// error occurred
					break;
				}
				
				// redirect to the article after the alerts have been set
				DekiMessage::success(wfMsg('Page.PageAlerts.success'));
				self::redirectTo($Title);
			} while (false);
		}
		
		$html .= $this->getAlertsForm($Title, $Alert);
		
		return $html;
	}
	
	public function getAlertsForm($Title, $Alert)
	{
		$status = $Alert->getStatus();
		
		$htmlPageTitle = htmlspecialchars($Title->getDisplayText());
		$html = '<h2>'. $htmlPageTitle . '</h2>';
		
		if ($status == DekiPageAlert::STATUS_PARENT)
		{
			$parentId = $Alert->getSubscriberId();
			
			$ParentTitle = Title::newFromId($parentId);
			$link = '<a href="'. $this->getTitle()->getLocalUrl('id='. $parentId) .'">'. $ParentTitle->getDisplayText() .'</a>';
			
			$html .= wfMsg('Page.PageAlerts.notice.parent', $link);
		}
		else
		{
			// used as the parameters array to set which radio is checked
			$checked = array('checked' => true);
					
			$html .= '<form method="post" class="page-alerts">';
				$html .= '<legend>'. wfMsg('Page.PageAlerts.form.legend') .'</legend>';
				$html .= '<div class="field">';
					$html .= DekiForm::singleInput('radio', 'status', DekiPageAlert::STATUS_SELF, $status == DekiPageAlert::STATUS_SELF ? $checked : null, wfMsg('Page.PageAlerts.form.self'));
				$html .= '</div>';
				$html .= '<div class="field">';
					$html .= DekiForm::singleInput('radio', 'status', DekiPageAlert::STATUS_TREE, $status == DekiPageAlert::STATUS_TREE ? $checked : null, wfMsg('Page.PageAlerts.form.tree'));
				$html .= '</div>';
				$html .= '<div class="field">';
					$html .= DekiForm::singleInput('radio', 'status', DekiPageAlert::STATUS_OFF, $status == DekiPageAlert::STATUS_OFF ? $checked : null, wfMsg('Page.PageAlerts.form.off.verbose'));
				$html .= '</div>';
				
				$html .= '<div class="submit">';
					$html .= DekiForm::singleInput('button', 'action', 'save', null, wfMsg('Page.PageAlerts.form.submit'));
					$html .= wfMsg('Page.PageAlerts.form.cancel', $Title->getLocalUrl(), $htmlPageTitle);
				$html .= '</div>';
			$html .= '</form>';
		}
		
		return $html;
	}
}
