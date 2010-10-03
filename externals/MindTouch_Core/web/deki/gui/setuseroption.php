<?php
require_once('gui_index.php');

class SetUserOption extends DekiFormatter
{
	protected $contentType = 'application/json';
	protected $requireXmlHttpRequest = true;

	public function format()
	{
		global $wgUser;
		
		$Request = DekiRequest::getInstance();
		
		$key = $Request->getVal( 'key' );
		$value = ( $Request->getVal( 'value' ) == 'true' ) ? '1' : null;
		
		$wgUser->setOption($key, $value);
		$wgUser->saveSettings();
	}
}

new SetUserOption();
