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

class UserManagement extends DekiControlPanel
{
	// needed to determine the template folder
	protected $name = 'user_management';


	public function index()
	{
		$this->executeAction('listing');
	}
	
	public function find()
	{
		if ($this->Request->isXmlHttpRequest())
		{
			$query = $this->Request->getVal('q');

			$filters = array(
				'usernamefilter' => $query,
				'fullnamefilter' => $query,
				'sortby' => 'username'
			);
			$users = DekiUser::getSiteList($filters, 1, 15);

			$return = '';
			foreach ($users as $id => $User)
			{
				$return .= $User->getName() . "\n";
			}

			$this->View->setRef('users', $return);
			$this->View->text('users');
		}
	}

    public function deactivated()
    {
    	$this->executeAction('listing', array('deactivated'));
    	
    }
    
	public function listing($status = 'activated')
	{
		global $wgLang;
		
		// set the requested action based on status
		$showActivated = $status == 'activated';
		$requestedAction = $showActivated ? 'listing' : 'deactivated';

		if ($this->Request->isPost())
		{
			$action = $this->Request->getVal('action');

			switch ($action)
			{
				default:
					// unsupported action
					$this->Request->redirect($this->getUrl('listing'));
					return;
				
				// bulk actions
				case 'activate':
				case 'deactivate':
				case 'ban':
				case 'unban':
				case 'group':
				case 'role':
					$userIds = $this->Request->getArray('user_id');
					if (empty($userIds))
					{
						DekiMessage::error($this->View->msg('Users.data.no-selection'));
						$this->Request->redirect($this->getUrl($requestedAction));
						return;
					}
					$this->executeAction($action);
					return;
			}
		}
		

		// build the listing table
		$Table = new DekiTable(
			$this->name,
			$requestedAction,
			array('id', 'username', 'email', 'role', 'service', 'date.lastlogin'),
			$status == 'activated' ? 'page' : 'pagex'
		);
		$Table->setResultsPerPage(Config::RESULTS_PER_PAGE);
		// enable searching for this table
		$Table->setSearchField(array('usernamefilter', 'fullnamefilter'));
		// set the default sorting
		$Table->setDefaultSort('username');
		// space the columns
		$Table->setColWidths('18', '', '150', '150', '100', '100', '80');

		// create the table header
		$Table->addRow();
		$Th = $Table->addHeading(DekiForm::singleInput('checkbox', 'all', '', array()));
		$Th->addClass('last checkbox');
		$Th = $Table->addSortHeading($this->View->msg('Users.data.username'), 'username');
		$Th->addClass('name');
		$Table->addSortHeading($this->View->msg('Users.data.email'), 'email');
		$Table->addSortHeading($this->View->msg('Users.data.authentication'), 'service');
		$Table->addSortHeading($this->View->msg('Users.data.role'), 'role');
		$Table->addSortHeading($this->View->msg('Users.data.active'), 'date.lastlogin');
		$Th = $Table->addHeading('&nbsp;');
		$Th->addClass('last edit');
		
		// grab the results
		$Plug = $this->Plug->At('users');
		// add status filters
		$Plug = $Plug->With('activatedfilter', $showActivated ? 'true' : 'false');
		
		$Result = $Table->getResults($Plug, '/body/users/@querycount');
		$Result->handleResponse();
		
		$users = $Result->getAll('/body/users/user', array());
		
		$resultCount = count($users);
		if ($resultCount == 0)
		{
			$Table->addRow();
			$Td = $Table->addCol('<div class="none">'. $this->View->msg('Users.data.empty') .'</div>');
			$Td->setAttribute('colspan', 7);
			$Td->addClass('last');
		}
		else
		{
			// @note usability enhancement prescribed by royk
			if ($resultCount == 1 && $this->Request->has('query'))
			{
				$Object = DekiUser::newFromArray(current($users));
				$editUrl = $this->getUrl('edit/'.$Object->getId());
				// redirect
				$this->Request->redirect($editUrl);
				return;
			}
			
			foreach ($users as $userArray)
			{
				$User = DekiUser::newFromArray($userArray);
				$Tr = $Table->addRow();
				if ($User->isDisabled())
				{
					$Tr->addClass('deactivated');
				}
				$Td = $Table->addCol(DekiForm::singleInput('checkbox', 'user_id[]', $User->getId(), array('id' => 'user'.$User->getId()), $User->toHtml()));
				$Td->setAttribute('colspan', 2);
				$Table->addCol($User->getEmail());
				
				// user auth service information
				if ($User->isAuthInvalid())
				{
					DekiMessage::error(wfMsg('Common.error.invalid-auth', $User->toHtml()));
					$Table->addCol('&nbsp;');
				}
				else
				{
					$Table->addCol($User->getAuthService()->toHtml());
				}
				
				$Table->addCol($User->getRole()->toHtml());
				$Table->addCol($wgLang->date($User->getLastLogin(), true));
				$Td = $Table->addCol('<a href="'. $this->getUrl('edit/' . $User->getId(), array('page'), true) .'">'. $this->View->msg('Users.edit') .'</a>');
				$Td->addClass('edit last');
			}
		}

		// init autocomplete & template search fields
		$this->setupTemplateSearch();
		$this->View->set('operations-form.action', $this->getUrl($requestedAction, array('page'), true));

		$this->View->set('addSingleUrl', $this->getUrl('add'));
		$this->View->set('addMultipleUrl', $this->getUrl('add_multiple'));

		$this->View->set('users-table', $Table->saveHtml());
		$this->View->set('searchQuery', $Table->getCurrentSearch());
		$this->View->output();
	}


	/*
	 * Bulk actions, require post data
	 */
	public function activate()		{ $this->setUserStatus(true); }
	public function deactivate()	{ $this->setUserStatus(false); }
	// helper method for setting posted users' status
	private function setUserStatus($activate)
	{
		$status = $activate ? DekiUser::STATUS_ACTIVE : DekiUser::STATUS_INACTIVE;

		if (!$this->Request->isPost())
		{
			$this->Request->redirect($this->getUrl('listing', array('page'), true));
			return;
		}
		$operate = $this->Request->getBool('operate');
		if ($operate)
		{
			$userIds = $this->Request->getArray('user_id');
			$count = 0;
			foreach ($userIds as $userId)
			{
				$User = DekiUser::newFromId($userId);
				if ($activate)
				{
					$User->enable();
				}
				// check if the user is trying to deactivate self
				else if (!$User->disable())
				{
					DekiMessage::error($this->View->msg('Users.error.deactivate.self'));
					continue;
				}
				
				$Result = $User->update();
				if ($Result->handleResponse()) 
				{
					if ($activate)
					{
						DekiMessage::success($this->View->msg('Users.success.activated', $User->getName()));
					}
					else
					{
						DekiMessage::success($this->View->msg('Users.success.deactivated', $User->getName()));
					}
				}
				else 
				{
					DekiMessage::error($this->View->msg('Users.error.user'));
				}
			}

			$this->Request->redirect($this->getUrl('listing', array('page'), true));
			return;
		}

		$userIds = $this->Request->getVal('user_id', array());
		$userList = '';
		foreach ($userIds as $userId)
		{
			$User = DekiUser::newFromId($userId);
			$userList .= '<li>' . $User->toHtml() . '<input type="hidden" name="user_id[]" value="'. $userId .'" />'.'</li>';
		}
		
		$url = $activate ? $this->getUrl('activate', array('page'), true) : $this->getUrl('deactivate', array('page'), true);
		$this->View->set('form.action', $url);
		$this->View->set('form.back', $this->getUrl('listing', array('page'), true));

		$this->View->set('users-list', $userList);
		$this->View->output();
	}
	
	public function ban()
	{
		throw new Exception('Not implemented');
	}
	
	public function unban()
	{
		throw new Exception('Not implemented');
	}
	
	public function group()
	{
		$groupcount = DekiGroup::getSiteCount(); 
		if ($groupcount == 0)
		{
			$this->Request->redirect($this->getUrl('listing', array('page'), true));
			DekiMessage::error($this->View->msg(
				'Users.error.nogroup', 
				$this->Request->getLocalUrl('group_management', 'add'))
			); 
			return;
		}
		
		// setup the group checkboxes
		$this->View->set('form.group-boxes', $this->getGroupBoxes());
		$this->View->set('form.group-boxes.count', $groupcount);
		
		$this->setUserWithTable('group');
	}

	public function role()
	{
		// setup the role selection
		$this->View->set('form.role-select', $this->getRoleSelect());

		$this->setUserWithTable('role');
	}
	// helper functions for setting bulk groups & roles
	private function setUserWithTable($action)
	{
		if (!$this->Request->isPost())
		{
			$this->Request->redirect($this->getUrl('listing'));
			return;
		}

		$operate = $this->Request->getBool('operate');
		if ($operate)
		{
			switch ($action)
			{
				case 'role':
					$roleId = $this->Request->getVal('role_id');
					$Role = DekiRole::newFromId($roleId);

					$userIds = $this->Request->getArray('user_id');
					$count = 0;
					foreach ($userIds as $userId)
					{
						$User = DekiUser::newFromId($userId);
						$User->setRole($Role);

						$Result = $User->update();
						if ($Result->isSuccess())
						{
							DekiMessage::success($this->View->msg('Users.success.role-changed', $User->getName(), $Role->getName()));
						}
						//todo: error message for failed states?
					}
					break;
				
				case 'group':
					$userIds = $this->Request->getArray('user_id');
					$groupIds = $this->Request->getVal('group_id', array());

					$groupNames = array();
					foreach ($groupIds as $groupId)
					{
						$Group = DekiGroup::newFromId($groupId);
						$Group->addUsers($userIds);
						$groupNames[] = $Group->getName();
					}

					if (!empty($groupNames))
					{
						$groups = implode('", "', $groupNames);
						$groups = '"'.$groups.'"';
						DekiMessage::success($this->View->msg('Users.success.group-added', $groups));
					}
					else
					{
						DekiMessage::error($this->View->msg('Users.error.group-added'));
					}
					break;
				
				default:
			}

			$this->Request->redirect($this->getUrl('listing', array('page'), true));
			return;
		}

		
		$url = $this->getUrl($action, array('page'), true);
		$this->View->set('form.action', $url);
		$this->View->set('form.back', $this->getUrl('listing', array('page'), true));


		$Table = new DomTable();
		$Table->addRow();
		$Table->addHeading($this->View->msg('Users.data.username'));
		$Table->addHeading($this->View->msg('Users.data.email'));
		$Table->addHeading($this->View->msg('Users.data.group'));
		$Th = $Table->addHeading($this->View->msg('Users.data.role'));
		$Th->addClass('last');
		
		$userIds = $this->Request->getArray('user_id');
		foreach ($userIds as $userId)
		{
			$User = DekiUser::newFromId($userId);
			$username = $User->toHtml() . '<input type="hidden" name="user_id[]" value="'. $userId .'" />';

			$Table->addRow();
			$Table->addCol($username);
			$Table->addCol(htmlspecialchars($User->getEmail()));
			$Table->addCol(implode(', ', $User->getGroupNames()));
			$Td = $Table->addCol($User->getRole()->toHtml());
			$Td->addClass('last');
		}

		$this->View->set('users-table', $Table->saveHtml());
		$this->View->output();
	}
	/*
	 * End bulk actions
	 */

	public function add()
	{
		if ($this->Request->isPost())
		{
			do
			{
				// attempt to create a new user with the specified information
				$username = $this->Request->getVal('username');
				$fullname = $this->Request->getVal('fullname');
				$email = $this->Request->getVal('email');
				// TODO: consolidate the field validation for edit, add, add_multiple
				$password = $this->Request->getVal('password');
				$passwordVerify = $this->Request->getVal('password_verify');
				if (!empty($password) && ($password != $passwordVerify))
				{
					DekiMessage::error($this->View->msg('Users.error.passwords'));
					break;
				}
				if (!empty($password) && strlen($password) < 4) 
				{
					DekiMessage::error($this->View->msg('Users.error.passwords-length'));
					break;
				}
				
				// create the user object
				$User = new DekiUser(null, $username, $fullname);

				$authType = $this->Request->getVal('auth_type', 'local');
				$authUsername = $this->Request->getVal('external_auth_username');
				$authPassword = $this->Request->getVal('external_auth_password');

				// set the authentication
				if ($authType == 'external')
				{
					// external authentication
					$authId = $this->Request->getVal('external_auth_id');
					$User->setAuthService($authId);
				}
				else
				{
					// local authentication
					$User->setAuthService(DekiAuthService::INTERNAL_AUTH_ID);
				}

				// validate the email after setting the auth service
				if (!$User->setEmail($email))
				{
					DekiMessage::error($this->View->msg('Users.error.email', $email)); 
					break;
				}

				// set the role
				$roleId = $this->Request->getInt('role_id', $User->getRole()->getId());
				$Role = DekiRole::newFromId($roleId);
				if ($Role == null)
				{
					// something went wrong while retrieving the role
					DekiMessage::error($this->View->msg('Users.error.role'));
					break;
				}
				else
				{
					$User->setRole($Role);
				}

				// check for an empty password
				if (empty($password))
				{
					$password = wfRandomStr();
				}
				else 
				{
					// validate passwords for internal users
					if ($User->isInternal()) 
					{
						
					}
				}
				
				// create the new user
				$Result = $User->create($authUsername, $authPassword, $password);
				if ($Result->getStatus() == 409) 
				{
					DekiMessage::error($this->View->msg('Users.error.exists', $username));
					break;
				}
				elseif (!$Result->handleResponse())
				{
					DekiMessage::error($this->View->msg('Users.error.nouser', $Result->getError()));
					break;
				}

				// notify the user of their new account
				$this->sendUserWelcomeEmail($User, $password);
				DekiMessage::success($this->View->msg('Users.success.user', $User->getName()));

				// add the user to the selected groups
				$groups = $this->Request->getVal('group_id', array());
				if (!empty($groups)) 
				{
					$User->updateGroups($groups);
					DekiMessage::success($this->View->msg('Users.success.usergroup', $User->getName()));
				}
				
				// everything checks out, redirect
				$this->Request->redirect($this->getUrl('add'));
				return;
			} while (false);
		}
		
		
		// init autocomplete & template search fields
		$this->setupTemplateSearch();
		// user add form
		$this->View->set('addMultipleUrl', $this->getUrl('add_multiple', array('page'), true));
		$this->View->set('add-form.action', $this->getUrl('add'));
		$this->View->set('add-form.back', $this->getUrl('listing', array('page'), true));

		// setup the role selection
		$this->View->set('form.role-select', $this->getRoleSelect());

		// setup external auth selections
		$this->View->set('form.auth-section', $this->renderAction('auth_form_section'));

		// setup the group checkboxes
		$this->View->set('form.group-boxes', $this->getGroupBoxes());
		$this->View->set('form.group-boxes.count', DekiGroup::getSiteCount());


		$this->View->output();
	}

	public function add_multiple()
	{
		if ($this->Request->isPost())
		{
			do
			{
				$authType = $this->Request->getVal('auth_type', 'local');
				$authId = $this->Request->getVal('external_auth_id');
				$authUsername = $this->Request->getVal('external_auth_username');
				$authPassword = $this->Request->getVal('external_auth_password');
				$groups = $this->Request->getArray('group_id');

				// set the authentication
				if ($authType == 'local')
				{
					// local authentication
					$authId = DekiAuthService::INTERNAL_AUTH_ID;
				}

				// set the role
				$roleId = $this->Request->getInt('role_id', 0);
				$Role = DekiRole::newFromId($roleId);
				if ($Role == null)
				{
					// something went wrong while retrieving the role
					DekiMessage::error($this->View->msg('Users.error.role'));
					break;
				}
				
				// attempt to create the users
				$userErrors = array();
				$userSuccesses = array();

				$userList = $this->Request->getVal('user_csv');
				$lines = explode("\n", $userList);
				foreach ($lines as $line)
				{
					@list($username, $email) = explode(',', $line, 2);
					$username = trim($username);
					$email = trim($email);

					// create the user object
					$User = new DekiUser(null, $username, $username);

					// validate the email
					if (!$User->setEmail($email))
					{
						// could not create this user
						$userErrors[] = array(
							'name' => $username,
							'email' => $email,
							'error' => $this->View->msg('Users.error.email', $email)
						);
						continue;
					}

					$User->setRole($Role);
					$User->setAuthService($authId);
					
					// create the new user
					$Result = $User->create($authUsername, $authPassword);
					if (!$Result->isSuccess())
					{
						// could not create this user
						$userErrors[] = array(
							'name' => $username,
							'email' => $email,
							'error' => htmlspecialchars($Result->getError())
						);
					}
					else
					{
						$userSuccesses[] = array(
							'name' => $username,
							'email' => $email,
							'id' => $User->getId()
						);
						
						// user was created successfully
						if ($User->isInternal())
						{
							// set the user's temporary password
							$newPassword = wfRandomStr();
							$Result = $User->changePassword($newPassword, null, true);
	
							if ($Result->isSuccess())
							{
								$this->sendUserWelcomeEmail($User, $newPassword);
							}
						}
						else
						{
							// external users cannot have their passwords reset
							$this->sendUserWelcomeEmail($User);
						}
					}
				}
				
				$successes = count($userSuccesses);
				if ($successes > 0)
				{
					$userIds = array();
					foreach ($userSuccesses as $user)
					{
						$userIds[] = $user['id'];
					}

					foreach ($groups as $groupId)
					{
						$Group = DekiGroup::newFromId($groupId);
						if (!$Group->addUsers($userIds))
						{
							DekiMessage::error($this->View->msg('Users.error.group', $Group->getName())); 
						}
					}
					DekiMessage::success($this->View->msg('Users.success.multiple', $successes));
				}

				$errors = count($userErrors);
				if ($errors > 0)
				{
					DekiMessage::error($this->View->msg('Users.error.multiple', $errors)); 

					// update the post field for the form, only show users with errors
					foreach ($userErrors as $user)
					{
						// report an error message for each user
						DekiMessage::error(
							$this->View->msgRaw('Users.error.multiple.user', htmlspecialchars($user['name']), $user['error'])
						);
						$userList = $user['name'] . ',' . $user['email'] . "\n";
					}
					$_POST['user_csv'] = $userList;
					
					DekiMessage::error($this->View->msg('Users.error.multiple.end'));
					break;
				}
				
				// everything checks out, redirect
				$this->Request->redirect($this->getUrl('add_multiple'));
				return;
			} while (false);
		}
		

		// init autocomplete & template search fields
		$this->setupTemplateSearch();
		// user add form
		$this->View->set('addSingleUrl', $this->getUrl('add', array('page'), true));
		$this->View->set('add-form.action', $this->getUrl('add_multiple'));
		$this->View->set('add-form.back', $this->getUrl('listing', array('page'), true));
		
		$this->View->set('add-form.user_csv', $this->Request->getVal('user_csv'));

		// setup the role selection
		$this->View->set('form.role-select', $this->getRoleSelect($this->Request->getVal('role_id', null)));

		// setup external auth selections
		$this->View->set('form.auth-section', $this->renderAction('auth_form_section'));

		// setup the group checkboxes
		$this->View->set('form.group-boxes', $this->getGroupBoxes());
		$this->View->set('form.group-boxes.count', DekiGroup::getSiteCount());

		$this->View->output();		
	}

	public function edit($id = null)
	{
		// need to find the user
		$User = DekiUser::newFromId($id);
		if (is_null($User))
		{
			DekiMessage::error($this->View->msg('Users.error.notfound-user'));
			$this->Request->redirect($this->getUrl('listing', array('page'), true));
			return;
		}
		
		// check if we are updating the info
		if ($this->Request->isPost())
		{
			do
			{
				// check if the authentication changed, apply
				$authType = $this->Request->getVal('auth_type');
				// save the current auth service id to check if it changed
				$oldAuthId = $User->getAuthService()->getId();
				if ($authType == 'external')
				{
					// external authentication
					$authId = $this->Request->getVal('external_auth_id');
					$User->setAuthService($authId);
				}
				else
				{
					// local authentication
					$User->setAuthService(DekiAuthService::INTERNAL_AUTH_ID);
				}

				// check if a change had been made
				if ($oldAuthId != $User->getAuthService()->getId())
				{
					// update the auth provider
					$authUsername = $this->Request->getVal('external_auth_username');
					$authPassword = $this->Request->getVal('external_auth_password');
					$Result = $User->update($authUsername, $authPassword);

					if (!$Result->handleResponse(array()))
					{
						// there was an error
						DekiMessage::error($this->View->msg('Users.error.auth'));
						break;
					}
					else
					{
						DekiMessage::success($this->View->msg('Users.success.auth'));
					}
				}


				// check if groups changed, apply
				// now attempt to set the user groups, other updates were successful
				$groups = $this->Request->getVal('group_id', array());
				$User->updateGroups($groups);
				// suppress the success message and only show the failure
				//DekiMessage::success($this->View->msg('Users.success.usergroups'));


				// validate the user fields
				$password = $this->Request->getVal('password');
				$passwordVerify = $this->Request->getVal('password_verify');
				if (!empty($password) && ($password != $passwordVerify))
				{
					DekiMessage::error($this->View->msg('Users.error.passwords'));
					break;
				}
				
				$email = $this->Request->getVal('email');
				// validate the email
				if (!$User->setEmail($email))
				{
					DekiMessage::error($this->View->msg('Users.error.email', htmlspecialchars($email)));
					break;
				}
				// name update only supported for local users
				// cannot update the anonymous user's name
				$User->setName($this->Request->getVal('name'));

				// set the role
				$roleId = $this->Request->getInt('role_id', $User->getRole()->getId());
				$Role = DekiRole::newFromId($roleId);

				if ($Role == null)
				{
					// something went wrong while retrieving the role
					DekiMessage::error($this->View->msg('Users.error.role'));
					break;
				}
				else
				{
					$User->setRole($Role);
				}
				
				$activeUser = $this->Request->getBool('status', true);
				if ($activeUser) 
				{
					$User->enable();
				}
				else if (!$User->disable())
				{
					DekiMessage::error($this->View->msg('Users.error.deactivate.self'));
				}

				// update the user fields
				$Result = $User->update();
				if (!$Result->handleResponse())
				{
					// there was an error
					DekiMessage::error($this->View->msg('Users.error.update'));
					break;
				}

				// change the user's password
				if (!empty($password))
				{
					$Result = $User->changePassword($password);
					if (!$Result->handleResponse())
					{
						// there was an error
						DekiMessage::error($this->View->msg('Users.error.password'));
						break;
					}
					else
					{
						DekiMessage::success($this->View->msg('Users.success.password'));
					}
				}

				// bugfix #8262: Only show success after all updates succeeded (password change, etc.)
				DekiMessage::success($this->View->msgRaw('Users.success.update', $User->toHtml()));
				
				// user was updated successfully, redirect to listing
				$this->Request->redirect($this->getUrl('listing', array('page'), true));
				return;
			} while (false);
		}
		
		
		// init autocomplete & template search fields
		$this->setupTemplateSearch();
		// begin setting up the view variables
		$this->View->set('edit-form.action', $this->getUrl('edit/' . $User->getId(), array('page'), true));
		$this->View->set('edit-form.back', $this->getUrl('listing', array('page'), true));
		$this->View->set('user.name', $this->Request->getVal('name', $User->getUsername()));
		$this->View->set('user.fullname', $User->getFullname());
		$this->View->set('user.email', $this->Request->getVal('email', $User->getEmail()));
		$this->View->set('user.isInternal', $User->isInternal());
		$this->View->set('user.isAnonymous', $User->isAnonymous());
		
		// setup the role selection
		$this->View->set('form.role-select', $this->getRoleSelect($this->Request->getVal('role_id', $User->getRole()->getId())));
		
		$this->View->set('form.status', 
			DekiForm::multipleInput(
				'radio', 
				'status', 
				array(
					'true' => $this->View->msg('Users.status.active'), 
					'false' => $this->View->msg('Users.status.deactive')
				), 
				$User->isDisabled() ? 'false' : 'true'
			)
		);

		// setup external auth selections
		$auth = array(
			$User->getAuthService()->isInternal() ? 'local' : 'external',
			$User->getAuthService()->getId()
		);
		$this->View->set('form.auth-section', $this->renderAction('auth_form_section', $auth));

		// setup the group checkboxes
		$this->View->set('form.group-boxes', $this->getGroupBoxes($User));
		$this->View->set('form.group-boxes.count', DekiGroup::getSiteCount());
		
		$this->View->output();
	}
	
	/**
	 * Sends the welcome email to a user with a password they can login with
	 * @param DekiUser $User - Welcome email recipient
	 * @param $newPassword - new password to be emailed. Ignored if user is external.
	 */
	private function sendUserWelcomeEmail(&$User, $newPassword = '')
	{
		global $wgServer, $wgSitename;
		// generate the MindTouchU url
		$uUrl = ProductURL::UNIVERSITY . '?email='.$User->getEmail() . '&signup=yes';

		if ($User->isInternal())
		{
			// internal email
			$subject = $this->View->msg('Users.email.subject', $wgSitename);
			$body = $this->View->msg('Users.email.body.text', $User->getName(), $wgServer, $newPassword, $wgSitename, $uUrl);
			$bodyHtml = $this->View->msg('Users.email.body.html', $User->getName(), $wgServer, $newPassword, $wgSitename, $uUrl);
		}
		else
		{
			// external email, unknown user password
			$subject = $this->View->msg('Users.email.external.subject', $wgSitename);
			$body = $this->View->msg('Users.email.external.body.text', $User->getName(), $wgServer, '', $wgSitename, $uUrl);
			$bodyHtml = $this->View->msg('Users.email.external.body.html', $User->getName(), $wgServer, '', $wgSitename, $uUrl);
		}
		
		return DekiMailer::sendEmail($User->getEmail(), $subject, $body, $bodyHtml);
	}


	private function getRoleSelect($selectedRoleId = null)
	{
		if (is_null($selectedRoleId)) 
		{
			global $wgNewAccountRole; //this gets the default role for created users & groups	
			$siteRoles = DekiRole::getSiteList();
			foreach ($siteRoles as $Role) 
			{
				if (strcmp($wgNewAccountRole, $Role->getName()) == 0) 
				{
					$selectedRoleId = $Role->getId();	
				}
			}
		}
		// setup the role selection
		$siteRoles = DekiRole::getSiteList();

		$data = array();
		foreach ($siteRoles as $Role) 
		{
			$data[$Role->getId()] = $Role->getName();
		}

		return DekiForm::multipleInput('select', 'role_id', $data, $selectedRoleId);
	}

	/*
	 * Renders a form section for setting authentication
	 */
	protected function auth_form_section($authType = 'local', $authId = null)
	{
		$this->View->set('auth-form-section.authType', $authType);
		$this->View->set('auth-form-section.authId', $authId);
		
		// build the data for the auth select
		$siteAuth = DekiAuthService::getSiteList();
		$data = array();
		
		foreach ($siteAuth as $Service)
		{
			$id = $Service->getId();
			if ($id != 1)
			{
				$data[$id] = $Service->getDescription();
			}
		}
		$this->View->setRef('auth-form-section.external-options', $data);

		$this->View->output();
	}

	private function getGroupBoxes($User = null)
	{
		$siteGroups = DekiGroup::getSiteList(null, 'all');
		
		$data = array();
		foreach ($siteGroups as $Group)
		{
			$id = $Group->getId();
			// if a user object was passed in then determine if they are a part of the group
			$isGroupMember = is_null($User) ? false : $User->isGroupMember($id);

			// determine if the group is local
			if ($Group->isInternal())
			{
				$data[$id] = array(
					'label' => $Group->getName(),
					'checked' => $isGroupMember
				);
			}
			else
			{
				// external groups cannot be edited and are ALWAYS disabled
				$data[$id] = array(
					'label' => $Group->getName(),
					'checked' => $isGroupMember,
					'disabled' => true
				);
			}
		}

		return DekiForm::multipleInput('checkbox', 'group_id', $data);
	}

	
	/**
	 * Initializes the template variables for searching and adds
	 * the autocomplete for this controller
	 */
	protected function setupTemplateSearch()
	{
		$this->View->includeCss('jquery.autocomplete.css');
		$this->View->includeJavascript('jquery.autocomplete.js');
		$this->View->includeJavascript('users.autocomplete.js');
		
		$this->View->set('template.search.action', $this->getUrl('listing'));
		$this->View->set('template.search.title', $this->View->msg('Users.search.label'));
	}
}

new UserManagement();
