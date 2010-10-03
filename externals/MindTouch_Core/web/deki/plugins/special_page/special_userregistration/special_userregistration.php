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
	require_once('skins/captcha.php');
	DekiPlugin::registerHook(Hooks::SPECIAL_USER_REGISTRATION, 'wfSpecialUserRegistration');
}

function wfSpecialUserRegistration($pageName, &$pageTitle, &$html)
{
	$Special = new SpecialUserRegistration($pageName, basename(__FILE__, '.php'));
	
	// set the page title
	$pageTitle = $Special->getPageTitle();
	$html = $Special->output();
}

class SpecialUserRegistration extends SpecialPagePlugin
{
	protected $pageName = 'UserRegistration';

	public function output()
	{
		// add some css
		$this->includeSpecialCss('special_userregistration.css');

		$Request = DekiRequest::getInstance();
		$User = DekiUser::getCurrent();

		if (!$User->isAnonymous())
		{
			// if user already has an account, redirect
			self::redirectTo();
			return;
		}

		global $wgAnonAccCreate;
		if (isset($wgAnonAccCreate) && !$wgAnonAccCreate)
		{
			DekiMessage::error(wfMsg('Page.UserRegistration.error.anonymous-disabled'));
			self::redirectTo();
			return;
		}

		if ($Request->isPost())
		{
			do
			{
				$username = $Request->getVal('username');
				$email = $Request->getVal('email');
				$password = $Request->getVal('password');
				$passwordVerify = $Request->getVal('password_verify');

				if (!captcha::check())
				{
					// remove the captcha text from the post
					$Request->remove('captcha_input');
					DekiMessage::error(wfMsg('Page.UserRegistration.error.captcha'));
					break;
				}

				// validate username, email, & password
				if (!self::validateUsername($username) || !self::validateEmail($email) || !self::validatePassword($password, $passwordVerify))
				{
					break;
				}

				// everything is good, create the user
				
				// check if any plugins want to handle the user creation
				$result = DekiPlugin::executeHook(
					Hooks::MAIN_CREATE_USER,
					array(
						&$username,
						&$password,
						&$email
					)
				);
				
				if ($result == DekiPlugin::HANDLED_HALT)
				{
					// user creation is complete?
					$User = DekiUser::getCurrent();
					DekiPlugin::executeHook(Hooks::MAIN_CREATE_USER_COMPLETE, array($User));
					break;
				}
				
				// default creation process
				// create the user object
				$User = new DekiUser(null, $username, null, $email);
				$Result = $User->create(null, null, $password);
				
				if ($Result->getStatus() == 409) 
				{
					DekiMessage::error(wfMsg('Page.UserRegistration.error.exists', htmlspecialchars($username)));
					break;
				}
				elseif (!$Result->handleResponse())
				{
					// problem when trying to create the user
					DekiMessage::error(wfMsg('Page.UserRegistration.error.creation'));
					break;
				}

				// new user was created successfully
				DekiMessage::success(wfMsg('Page.UserRegistration.success.created', $User->toHtml()));
				
				// login
				DekiUser::login($username, $password);
								
				// send the user an email
				$result = self::sendUserWelcomeEmail($User, $password);				
				if (!$result)
				{
					DekiMessage::error(wfMsg('Page.UserRegistration.error.email.send'));
					break;
				}
				// /end user email
				
				// calling complete here allows users to redirect to their own destination
				$User = DekiUser::getCurrent();
				$result = DekiPlugin::executeHook(Hooks::MAIN_CREATE_USER_COMPLETE, array($User));
				if ($result == DekiPlugin::HANDLED_HALT)
				{
					break;
				}
				
				// determine where to redirect the user
				global $wgRedirectToUserPageOnCreate;
				if (isset($wgRedirectToUserPageOnCreate) && $wgRedirectToUserPageOnCreate)
				{
					self::redirect($User->getUrl());
				}
				else
				{
					self::redirectTo();
				}

				return;
			} while (false);
		}


		$html = self::getNewAccountForm();

		return $html;
	}

	/**
	 * Sends the welcome email to a user with a password they can login with
	 */
	protected static function sendUserWelcomeEmail(&$User, $password)
	{
		global $wgServer, $wgSitename;
		// generate the MindTouchU url
		$uUrl = ProductURL::UNIVERSITY . '?email='.$User->getEmail() . '&signup=yes';

		$subject = wfMsg('Page.UserRegistration.email.subject', $wgSitename);
		$body = wfMsg('Page.UserRegistration.email.body.text', $User->getName(), $wgServer, $password, $wgSitename, $uUrl);
		$bodyHtml = wfMsg('Page.UserRegistration.email.body.html', $User->toHtml(), $wgServer, $password, $wgSitename, $uUrl);

		return DekiMailer::sendEmail($User->getEmail(), $subject, $body, $bodyHtml);
	}
	
	protected static function validateUsername(&$username)
	{
		if (empty($username))
		{
			DekiMessage::error(wfMsg('Page.UserRegistration.error.username'));
			return false;
		}

		return true;
	}

	protected static function validateEmail(&$email)
	{
		if (empty($email) || !wfValidateEmail($email))
		{
			DekiMessage::error(wfMsg('Page.UserRegistration.error.email'));
			return false;
		}

		return true;
	}

	protected static function validatePassword(&$password, &$verify)
	{
		if ($password != $verify)
		{
			DekiMessage::error(wfMsg('Page.UserRegistration.error.password.match'));
			return false;
		}
		else if (strlen($password) < 4)
		{
			DekiMessage::error(wfMsg('Page.UserRegistration.error.password.length'));
			return false;
		}

		return true;
	}

	protected static function getNewAccountForm()
	{
		$Title = Title::newFromText('UserRegistration', NS_SPECIAL);
		// build the markup
		$html = '<form class="user-registration" method="post" action="'. $Title->getLocalUrl() .'">';
		
		// username field
		$html .= '<div class="field">';
			$html .= DekiForm::singleInput('text', 'username', null, array(), wfMsg('Page.UserRegistration.form.username'));
		$html .= '</div>';

		// email field
		$html .= '<div class="field">';
			$html .= DekiForm::singleInput('text', 'email', null, array('autocomplete' => 'on'), wfMsg('Page.UserRegistration.form.email'));
		$html .= '</div>';

		// password field
		$html .= '<div class="field">';
			$html .= DekiForm::singleInput('password', 'password', null, array(), wfMsg('Page.UserRegistration.form.password'));
		$html .= '</div>';
		
		// password confirmation
		$html .= '<div class="field">';
			$html .= DekiForm::singleInput('password', 'password_verify', null, array(), wfMsg('Page.UserRegistration.form.password.verify'));
		$html .= '</div>';

		// captcha
		$captcha = captcha::form();
		$html .= '<div class="captcha">';
		$html .= '<div class="field">';
			$html .= '<label for="captcha_input">'. wfMsg('Page.UserRegistration.form.captcha') .'</label>' . $captcha['text'] . $captcha['hidden'];
		$html .= '</div>';
		$html .= '<div class="captcha-image">';
			$html .= $captcha['image'];
		$html .= '</div>';
		$html .= '</div>';
		
		// submit button
		$html .= DekiForm::singleInput('button', 'action', 'login', array(), wfMsg('Page.UserRegistration.form.submit'));
		$html .= '</form>';

		return $html;
	}
}
