<?php
require_once('gui_index.php');

class UserOrGroupSearch extends DekiFormatter
{
	protected $contentType = 'application/json';
	protected $requireXmlHttpRequest = true;
	
	protected $Request;
	
	protected $groupSuffix;

	public function format()
	{
		$this->Request = DekiRequest::getInstance();
		$mode = $this->Request->getVal('mode');
		$jsonEncoded = $this->Request->getVal('format', 'text') == 'json' ? true : false;
		
		$this->groupSuffix = wfMsg('System.Common.group-suffix');
		
		$result = '';
		
		switch ($mode)
		{
			case 'userorgroup':
				$result = $this->getUserOrGroup($jsonEncoded);
				break;
			case 'users':
				$result = $this->getUsers();
				break;
			case 'usersandgroups':
				$result = $this->getUsersAndGroups();
				break;
			default:
				header('HTTP/1.0 404 Not Found');
				exit(' '); // flush the headers
		}
		
		echo $result;		
	}
	
	private function getUserOrGroup($jsonEncoded = false)
	{
		$name = $this->Request->getVal('name');
		
		$result = '';

		if (substr($name, strlen($name) - strlen($this->groupSuffix)) == $this->groupSuffix)
		{
			$groupName = substr($name, 0, strlen($name) - strlen($this->groupSuffix));
			$Group = DekiGroup::newFromText($groupName);
			
			if ($Group)
			{
				$result = 'g' . $Group->getId();
			}
		}
		else
		{
			$User = DekiUser::newFromText($name);
			
			if (is_null($User))
			{
				$Group = DekiGroup::newFromText($name);
				
				if (!is_null($Group))
				{
					$result = 'g' . $Group->getId();
				}
			}
			else
			{
				$result = 'u' . $User->getId();
			}
		}
		
		if (!$jsonEncoded)
		{
			// simple return value
			return $result;
		}
		else
		{
			// advanced return value
			$success = $result != '';
			
			$return = array(
				'success' => $success,
				'message' => $success ? '' : wfMsg('GUI.UserGroupSearch.error.unknown')
			);
			
			if ($success)
			{
				// parse the result in php
				if (strncmp($result, 'g', 1) == 0)
				{
					// group
					$return['group'] = substr($result, 1);	
				}
				else
				{
					// user
					$return['user'] = substr($result, 1);
				}
			}
			
			return json_encode($return);
		}
	}
	
	/***
	 * Returns a list of all users who match that query
	 * 
	 */
	private function getUsers($jsonEncoded = true)
	{
		$query = $this->Request->getVal('query');
		
		$result = array();
		
		if ( strlen($query) > 1 )
		{
			$filters = array(
				'usernamefilter' => $query,
				'fullnamefilter' => $query,
				'sortby' => 'username'
			);
			
			$users = DekiUser::getSiteList($filters, 1, 10);
			
			foreach ($users as $User)
			{
				$result['results'][] = array(
					'item' => $User->toHtml(),
					'id' => $User->getUsername()
				);
			}
		}
		
		return ($jsonEncoded) ? json_encode($result) : $result;		
	}

	/***
	 * Returns a list of all groups who match that query
	 * 
	 */
	private function getGroups($jsonEncoded = true, $withGroupSuffix = false)
	{
		$query = $this->Request->getVal('query');
		
		$result = array();
		
		if ( strlen($query) > 1 )
		{
			$filters = array();
			$filters['groupnamefilter'] = $query;
			$filters['sortby'] = 'name';
			
			$groups = DekiGroup::getSiteList($filters, 1, 10);
			
			foreach ($groups as $Group)
			{
				$groupName = ($withGroupSuffix) ? $Group->toHtml() . $this->groupSuffix : $Group->toHtml();
				$result['results'][] = array(
					'item' => $groupName,
					'id' => $groupName
				);
			}
		}
		
		return ($jsonEncoded) ? json_encode($result) : $result;		
	}
	
	private function getUsersAndGroups()
	{
		$result = array();

		$groups = $this->getGroups(false, true);
		$users = $this->getUsers(false);
		
		$result = array_merge_recursive($groups, $users);

		return json_encode($result);
	}
}
new UserOrGroupSearch();
