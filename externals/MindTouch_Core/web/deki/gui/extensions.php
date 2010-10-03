<?php
require_once('gui_index.php');

class ExtensionsFormatter extends DekiFormatter
{
	protected $contentType = 'text/html';
	protected $requireXmlHttpRequest = false;
	

	public function format()
	{
		$Request = DekiRequest::getInstance();
		
		$action = $Request->getVal('action');

		switch ($action)
		{
			case 'autocomplete':
			case 'ac':
				$this->setContentType('application/json');
				$start = microtime(true);
				$query = $Request->getVal('query');

				$result = $this->searchExtensions($query);
				$end = microtime(true);
				// add the processing time to the result
				$result['execution'] = ($end - $start);
				
				echo json_encode($result);
				return;

			default:
				$Result = DekiPlug::getInstance()->At('site', 'functions')->WithApiKey()->Get();
				echo $Result->getVal('body');
		}
	}
	
	private function searchExtensions($query)
	{
		if (empty($query))
		{
			return array();
		}
		
		// TODO: cache the extensions on the filesystem in serialized PHP
		// search through the cached blob for the query
		
		$Result = DekiPlug::getInstance()->At('site', 'functions')->With('format', 'xml')->Get();
		$extensions = $Result->getAll('body/extensions/extension');
		
		$functions = array();
		foreach ($extensions as &$extension)
		{
			$namespace = !empty($extension['namespace']) ? $extension['namespace'] . '.' : '';
			foreach ($extension['function'] as &$function)
			{
				$functions[] = $namespace . $function['name'];
			}
			unset($function);
		}
		unset($extension);
		// ~200 ms
		
		$length = strlen($query);
		$results = array();
		foreach ($functions as &$function)
		{
			if (strncasecmp($query, $function, $length) == 0)
			{
				$results[] = $function;
			}
		}
		
		return array('results' => &$results);
	}
}

new ExtensionsFormatter();
