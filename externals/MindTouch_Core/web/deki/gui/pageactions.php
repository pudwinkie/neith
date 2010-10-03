<?php
require_once('gui_index.php');

class PageActions extends DekiFormatter
{
	protected $contentType = 'application/json';
	protected $requireXmlHttpRequest = true;

	private $Request;

	public function format()
	{
		$this->Request = DekiRequest::getInstance();
		
		$action = $this->Request->getVal( 'action' );

		switch ( $action )
		{
			case 'setrestrictions':
				$success = $this->setPageRestrictions();
				$result = array(
					'success' => $success
				);
				break;

			case 'delete':
				$result = $this->deletePage();
				break;
			
			case 'setalerts':
				$result = $this->setPageAlerts();
				break;

			default:
				header('HTTP/1.0 404 Not Found');
				exit(' '); // flush the headers
		}
		
		echo json_encode($result);
	}

	private function setPageRestrictions()
	{
		global $wgDekiPlug, $wgUser, $wgDefaultRestrictRole;
		
		$pageId = $this->Request->getVal( 'titleid' );
		$restrictType = $this->Request->getVal( 'protecttype' );
		$listIds = $this->Request->getVal( 'userids' );
		$cascade = $this->Request->getVal( 'cascade' );
		
		$listIds = empty($listIds) ? array(): explode(',', $listIds);
		
		$users = array();
		$groups = array();
		
		foreach ($listIds as $id)
		{
			if (strncmp($id, 'u', 1) == 0)
			{
				$users[] = substr($id, 1);
			}
			if (strncmp($id, 'g', 1) == 0)
			{
				$groups[] = substr($id, 1);
			}
		}
	
		//can't lock yourself out of the page!
		if (empty($users) && empty($groups))
		{
			$users[] = $wgUser->getId();
		}
	
		$grants = array();
		$groups = array_unique($groups);
		$users = array_unique($users);
	
		foreach ($users as $userId)
		{
			$grants[] = array('permissions' => array('role' => $wgDefaultRestrictRole), 'user' => array('@id' => $userId));
		}
		foreach ($groups as $groupId)
		{
			$grants[] = array('permissions' => array('role' => $wgDefaultRestrictRole), 'group' => array('@id' => $groupId));
		}
	
		//generate the XML document to PUT for grant list
		$xml = array(
			'security' => array(
				'permissions.page' => array('restriction' => $restrictType),
				'grants' => array('@restriction' => $restrictType, array('grant' => $grants))
			)
		);
	
		$Plug = $wgDekiPlug->At('pages', $pageId, 'security');

		if ($cascade == 'true')
		{
			$Plug = $Plug->With('cascade', 'delta');
		}

		$Plug = $Plug->Put($xml);
		
		if (MTMessage::HandleFromDream($Plug))
		{
			wfMessagePush('general', wfMsg('Article.Common.permissions-updated'), 'success');
		}
		
		return true;
	}

	private function deletePage()
	{
		$titleId = $this->Request->getVal( 'titleid' );
		$includeChildren = $this->Request->getBool( 'cascade' );
		
		$title = Title::newFromID($titleId);
		
	    if (!$title)
	    {
	        return array(
	        	'success' => false,
	        	'message' => 'Topic already deleted'
	        );
	    }
	    
	    $article = new Article($title);
	    
	    if (!$article->userCanDelete())
	    {
	        return array(
	        	'success' => false,
	        	'message' => 'Topic cannot be deleted'
	        );
	    }
	    
	    $error = $article->doDelete($redirectTitle, $includeChildren);
	    
	    return array(
	    	'success' => true,
	    	'redirectTo' => $redirectTitle 
	    );
	}
	
	private function setPageAlerts()
	{
		$pageId = $this->Request->getVal('pageId');
		// status should be an integer status from DekiPageAlert
		$status = $this->Request->getInt('status');
		
		$Title = Title::newFromId($pageId);
		if (is_null($Title))
		{
			// invalid page id specified
			return array(
				'success' => false,
				'message' => 'Invalid page specified'
			);	
		}
		
		$Article = new Article($Title);
		$parentIds = $Article->getParentIds();
		
		$Alert = new DekiPageAlert($pageId, $parentIds);
		$currentStatus = $Alert->getStatus();

		if ($currentStatus == DekiPageAlert::STATUS_PARENT)
		{
			return array(
				'success' => false,
				'message' => 'A parent page is already subscribed'
			);	
		}
		else if ($currentStatus == $status)
		{
			// no change
			$isSubscribed = $Alert->isSubscribed();
			return array(
				'success' => true,
				'subscribed' => $isSubscribed,
				'message' => $isSubscribed ? wfMsg('Page.PageAlerts.status.on') : wfMsg('Page.PageAlerts.status.off')
			);		
		}
		else
		{
			// perform the status change
			$Result = $Alert->setStatus($status);
			if ($Result->isSuccess())
			{
				$isSubscribed = $Alert->isSubscribed();
				return array(
					'success' => true,
					'subscribed' => $isSubscribed,
					'message' => $isSubscribed ? wfMsg('Page.PageAlerts.status.on') : wfMsg('Page.PageAlerts.status.off')
				);
			}
			else
			{
				// some error occurred
				$isSubscribed = $Alert->isSubscribed();
				return array(
					'success' => false,
					'subscribed' => false,
					'message' => $Result->getError()
				);				
			}

		}
	}
}

new PageActions();
