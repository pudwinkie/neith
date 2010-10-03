<?php
/***
 * PHP Alt: a lightweight entry point for MindTouch Deki
 *
 * @authors royk, guerrics
 */

/**
 * Include the entire front-end for now
 */
define('MINDTOUCH_DEKI', true);
require_once('../../includes/Defines.php');
require_once('../../LocalSettings.php');
require_once('../../includes/Setup.php');


class DekiFormatter
{
	protected $contentType = 'text/plain';
	protected $charset = 'UTF-8';
	protected $requireXmlHttpRequest = false;
	protected $disableCaching = false;

	public function __construct()
	{
		$this->checkXmlHttpRequest();
		$this->setContentType($this->contentType, $this->charset);
		
		if ($this->disableCaching)
		{
			$this->disableCaching();
		}
		
		$this->format();
	}
	
	protected function checkXmlHttpRequest()
	{
		if ($this->requireXmlHttpRequest)
		{
			$Request = DekiRequest::getInstance();
			if (!$Request->isXmlHttpRequest())
			{
				// TODO: how to handle?
				// requesting client is not an XmlHttpRequest
				header('Location: /');
				exit(' ');
			}
		}
	}

	protected function setContentType($contentType = null, $charset = null)
	{
		if (!is_null($contentType))
		{
			$type = empty($contentType) ? 'text/plain' : $contentType;
			$charset = empty($charset) ? 'UTF-8' : $charset;
			header('Content-Type: ' . $type . '; charset=' . $charset);
		}
	}
	
	protected function disableCaching()
	{
		header("Expires: Mon, 26 Jul 1997 05:00:00 GMT");  // disable IE caching
		header("Last-Modified: " . gmdate( "D, d M Y H:i:s") . " GMT");
		header("Cache-Control: no-cache, must-revalidate");
		header("Pragma: no-cache");
	}
	
	/**
	 * @stub method called upon formatter creation
	 */
	protected function format() {}
}
