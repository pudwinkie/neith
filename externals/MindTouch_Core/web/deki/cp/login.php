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

define('DEKI_ADMIN', true);
require_once('index.php');


class ControlPanelLogin extends DekiControlPanel
{
	// needed to determine the template folder
	protected $name = 'login';
	
	public function index()
	{
		// make sure the user is not logged in
		if (!DekiUser::getCurrent()->isAnonymous())
		{
			$this->Request->redirect($this->Request->getLocalUrl('dashboard'));
			return;
		}

		$this->View->setTemplateFile('/canvas.php');

		if ($this->Request->isPost())
		{
			$authId = $this->Request->getInt('auth_id');
			$username = $this->Request->getVal('username');
			$password = $this->Request->getVal('password');

			if (DekiUser::login($username, $password, $authId))
			{
				// successful login
				$returnurl = $this->Request->getVal('returnurl');
				// validate the url
				if (!XUri::isUrl($returnurl))
				{
					$returnurl = null;
				}

				$this->Request->redirect(empty($returnurl) ? $this->Request->getLocalUrl('dashboard') : $returnurl);
				return;	
			}

			// invalid username or password specified
			DekiMessage::error($this->View->msg('Login.error.auth'));
		}
		
		$authList = DekiAuthService::getSiteList();
		if (count($authList) > 1)
		{
			$authOptions = array();
			foreach ($authList as $authId => $AuthService)
			{
				$authOptions[$AuthService->getId()] = $AuthService->getDescription();
			}
			$this->View->setRef('authOptions', $authOptions);
		}

		$defaultId = DekiAuthService::getDefaultProviderId();
		if (is_null($defaultId))
		{
			$defaultId = DekiAuthService::INTERNAL_AUTH_ID;
		}
		$this->View->set('defaultAuthId', $defaultId);
		$this->View->set('returnurl', $this->Request->getVal('returnurl'));

		$this->View->output();
	}
	
}

new ControlPanelLogin();
