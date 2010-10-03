<?php
require_once('gui_index.php');
// copied from index.php
// (guerrics) handles loading plugins and calling hooks
require_once($IP . $wgDekiPluginPath . '/deki_plugin.php');
// load plugins
DekiPlugin::loadSitePlugins();

/**
 * End point allows plugins to hook in AJAX functionality
 */
class DekiPluginsFormatter extends DekiFormatter
{
	const STATUS_ERROR = 0;
	const STATUS_OK = 200;
	const STATUS_ERROR_LOGIN = 401;
	const STATUS_ERROR_COMMERCIAL = 402;
	
	protected $contentType = 'text/plain';
	protected $requireXmlHttpRequest = false;
	/**
	 * By default, the plugin formatter disables caching to ease confusion.
	 */
	protected $disableCaching = true;

	protected $formatter = '';
	// @var string - determines the expected response format e.g. json, xml, text
	protected $responseFormat = 'json';
	// @var string - load plugins from a special namespace, like 'special:' (default empty)
	protected $namespace = '';
	
	public function __construct()
	{
		$Request = DekiRequest::getInstance();
		$this->formatter = $Request->getVal('formatter', null);
		$this->responseFormat = $Request->getVal('format', 'json');
		$this->namespace = $Request->getVal('namespace', null);

		if (empty($this->formatter))
		{
			exit('No hook specified. Please send a formatter name with your request.');
		}
		
		// set the content type based on the request
		$contentType = $this->contentType;
		switch ($this->responseFormat)
		{
			case 'json':
				$contentType = 'application/json';
				break;
			case 'xml':
				$contentType = 'application/xml';
				break;
			default:
		}
		$requireXmlHttpRequest = $this->requireXmlHttpRequest;
		$disableCaching = $this->disableCaching;

		// allow the formatter to override these settings
		DekiPlugin::executeHook(Hooks::AJAX_INIT . $this->formatter, array(&$contentType, &$requireXmlHttpRequest, &$disableCaching));
		
		// apply the adjusted settings for this formatter
		$this->contentType = $contentType;
		$this->requireXmlHttpRequest = $requireXmlHttpRequest;
		$this->disableCaching = $disableCaching;
		
		parent::__construct();
	}
	
	public function format()
	{
		$body = '';
		$message = '';
		$success = false;
		$status = null;
		
		// activate hooks from special pages
		if (strcasecmp($this->namespace, 'special') == 0)
		{
			SpecialPageDispatchPlugin::loadSpecialPages();
		}
		
		$result = DekiPlugin::executeHook(Hooks::AJAX_FORMAT . $this->formatter, array(&$body, &$message, &$success, &$status));
		
		if (is_null($status))
		{
			$status = $success ? self::STATUS_OK : self::STATUS_ERROR;
		}   
		
		if ($result < DekiPlugin::HANDLED)
		{
			$message = 'Unhandled request';
			$success = false;
		}

		// @TODO: handled halting
		switch ($this->responseFormat)
		{
			default:
			case 'json':
				echo json_encode(
					array(
						'success' => (bool)$success,
						'status' => $status,
						'message' => $message,
						'body' => $body
					)
				);
				break;

			case 'xml':
				// TODO: handled halting
				echo encode_xml(
					array('formatter' => array(
							'@success' => (bool)$success,
							'@status' => $status,
							'@message' => $message,
							'body' => $body
						)
					)
				);
				break;
		}
	}
}
new DekiPluginsFormatter();
