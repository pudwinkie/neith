<?php
require_once('gui_index.php');


class EditorFormatter extends DekiFormatter
{
	protected $contentType = 'application/json';


	public function format()
	{
		$Request = DekiRequest::getInstance();
		$method = $Request->getVal('method');

		switch ($method)
		{
			case 'transform': 
				$this->transform();			
				break;

			case 'checkPermissions':
				$this->checkPermissions();
				break;

			default:
				header('HTTP/1.0 404 Not Found');
				exit(' '); // flush the headers
		}
	}

	// sorting methods for api data
	private function sortFunctionByName($a, $b) { return strcmp($a['name'], $b['name']); }

	private function transform()
	{
		global $wgDekiPlug;

		$result = $wgDekiPlug->At('site', 'functions')->With('format', 'xml')->Get();
		
		if ($result['status'] != Plug::HTTPSUCCESS)
		{
			$error = wfArrayVal($result, 'body/error');
			
			$message = !empty($error['message']) ? $error['title'] .': '. $error['message'] :
												   wfMsg('System.Error.error') .': '. $result['status'];
			$this->body = sprintf('{"status": "%s", "body": "%s"}', $result['status'], $message);
			return;
		}
		else
		{
			// array to store the list of transform extensions
			$transformers = array();

			$extensions = wfArrayValAll($result, 'body/extensions/extension');
			foreach ($extensions as $extension)
			{
				$namespace = isset($extension['namespace']) ? $extension['namespace'] : null;
				$functions = wfArrayValAll($extension, 'function');

				uasort($functions, array($this, 'sortFunctionByName'));
				foreach ($functions as $function)
				{
					if (isset($function['@transform']))
					{
						// function is a content transformation
						// guerrics: should we have a display title?
						$fq = is_null($namespace) ? $function['name'] : $namespace .'.'. $function['name'];
						$transformers[] = array('function' => $fq,
												'tags' => $function['@transform']);
					}
				}
			}

			$json = '';
			// generate the json
			foreach ($transformers as $transformer)
			{
				$json .= sprintf('{"func":"%s","tags":"%s"},', $transformer['function'], $transformer['tags']);
			}
			$json = !empty($json) ? substr($json, 0, -1) : '';

			$this->body = sprintf('{"status": "%s", "body": [%s]}', $result['status'], $json);

			echo $this->body;
			return;
		}
	}

	private function checkPermissions()
	{
		$success = false;

		$Request = DekiRequest::getInstance();

		$pageId = $Request->getInt('pageId');
		$pageTitle = $Request->getVal('pageTitle');

		if ($pageId > 0)
			$Title = Title::newFromID($pageId);
		else
			$Title = Title::newFromText($pageTitle);

		$Article = new Article($Title);

		$permissions = array();

		if ($pageId > 0)
			$permissions = $Article->getPermissions();
		else
			$permissions = $Article->getParentPermissions();

		if (is_array($permissions) && in_array('UPDATE', $permissions))
		{
			$success = true;
		}

		$this->disableCaching();

		$result = array(
			'success' => $success
		);

		if ( !$success )
		{
			$result['message'] = wfMsg('GUI.Editor.error.unable-to-save');
			$result['body'] = wfMsg('GUI.Editor.error.session-has-expired');
		}

		echo json_encode($result);
	}
}

new EditorFormatter();
