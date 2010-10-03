<?php

class DekiControlPanel extends DekiController
{

	protected function initializeObjects()
	{
		// set the global admin controller for error reporting
		global $wgAdminController;
		// set the active controller
		$wgAdminController = $this;

		parent::initializeObjects();
	}

	protected function initialize()
	{
		$User = DekiUser::getCurrent();
		if ($User->isAnonymous())
		{
			// redirect to control panel login
			if ($this->name != 'login')
			{
				$this->Request->redirect($this->Request->getLocalUrl('login', null, array('returnurl' => $this->Request->getFullUri())));
				return;
			}
		}
		// make sure the current user has admin rights
		else if (!$User->can('ADMIN'))
		{
			// user does not have control panel access
			DekiMessage::ui(wfMsg('Common.error.no-cp-access'));
			$this->Request->redirect('/');
			return;
		}

		// follow the normal controller code path
		parent::initialize();
	}
	
}
