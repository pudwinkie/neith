<?php
require_once('gui_index.php');

class OpenSearchFormatter extends DekiFormatter
{
	protected $contentType = null;


	public function format() 
	{
		global $wgDreamServer, $wgDekiApi, $wgRequest;
		$type = $wgRequest->getVal('type');
		$sortby = $wgRequest->getVal('sortby');
		$constraint = $wgRequest->getVal('constraint');

		if (!empty($type) && $type == 'description')
		{
			$Plug = new Plug($wgDreamServer, null);
			$scheme = (isset($_SERVER['HTTPS']) &&  $_SERVER['HTTPS'] == "on") ? 'https': 'http';
			$r = $Plug->At($wgDekiApi)
				->At('site', 'opensearch', 'description')
				->With('dream.in.scheme', $scheme)
				->With('dream.in.host', $_SERVER['HTTP_HOST'])
				->Get();
			if ($r['status'] == 200) 
			{
				header('Content-type: '.$r['type']);
				echo($r['body']);
			}
		}
		//rss feed subscription
		else 
		{
			$query = $wgRequest->getVal('q');
			if (!is_null($query))
			{
				$Plug = new Plug($wgDreamServer, null);
				$r = $Plug->At($wgDekiApi)
					->At('site', 'opensearch')
					->With('q', $query)
					->With('constraint', $constraint)
					->With('sortby', $sortby)
					->With('dream.in.host', $_SERVER['HTTP_HOST'])
					->Get();
				if ($r['status'] == 200) 
				{
					header('Content-type: '.$r['type']);
					echo($r['body']);
				}
			}
		}
	}
}

new OpenSearchFormatter();
