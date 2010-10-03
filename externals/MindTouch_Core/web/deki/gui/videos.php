<?php
require_once('gui_index.php');

class VideosFormatter extends DekiFormatter
{
	protected $contentType = 'text/html';
	//protected $requireXmlHttpRequest = true;
	
	public function __construct()
	{
		$this->format = DekiRequest::getInstance()->getVal('format');
		$this->format = $this->format == 'php' ? 'php' : 'html';
		if ($this->format == 'php')
		{
			$this->contentType = 'application/php';
		}
		
		parent::__construct();
	}

	public function format() 
	{
		global $wgVideoRssUrl, $wgCacheDirectory;
		$Request = DekiRequest::getInstance();

		if (empty($wgVideoRssUrl)) 
		{
			echo('<!-- no rss feed set -->');
			return;
		}

		$videoLimit = $Request->getInt('limit', 2);
		if ($videoLimit < 2)
		{
			$videoLimit = 2;
		}

		
		//cache RSS output
		define('MAGPIE_CACHE_DIR', $wgCacheDirectory);
		require_once('includes/magpie-0.72/rss_fetch.inc');
		$rss = fetch_rss( $wgVideoRssUrl );
		if (empty($rss->items)) 
		{
			return;
		}

		
		$i = 0; 
		//wfprintr($rss, true);
		$output = $this->format == 'html' ? '' : array();
		foreach ($rss->items as $item) 
		{
			$i++;
			if ($i > $videoLimit) 
			{
				break;
			}
			// such a horrible hack, cause magpie or PHP won't read empty elements; Viddler returns media:thumbnail...
			preg_match("/(<img.* alt=\"(.*)\" \/>)/Uis", $item['description'], $matches);
			$thumbnail = str_replace($matches[2], '', $matches[0]);
			
			if ($this->format == 'html')
			{
				$output .= '<div class="video"><p class="content"><a href="'.$item['guid'].'">'.$thumbnail.'</a></p>';
				$output .= '<p class="permalink"><a href="'.$item['link'].'" title="'.htmlspecialchars($item['title']).'">'
					.(strlen($item['title']) > 36 ? substr($item['title'], 0, 33).'...': $item['title'])
					.'</a></p>';
				$output .= '<p class="date">'.date('Y/m/d', wfTimestamp(TS_UNIX, strtotime($item['pubdate']))).'</p></div>';
			}
			else
			{
				$video = array(
					'title' => htmlspecialchars($item['title']),
					'link' => $item['link'],
					'thumb' => $thumbnail,
					'date.published' => $item['pubdate']
				);
				$output['videos']['video'][] = $video;
			}
		}

		//do very simple HTML output
		if ($this->format == 'html')
		{
			echo('<html>');
			echo('<head><title>'.$rss->channel['title'].'</title>');
			echo('<link href="/skins/common/reset.css" rel="stylesheet" type="text/css" />');
			echo('<link href="/deki/cp/assets/videos.css" rel="stylesheet" type="text/css" />');
			echo('<base target="_blank" />');
			echo('</head>');
			echo('<body id="updateBody">');
			echo($output);
			echo('</html>');
		}
		else
		{
			echo serialize($output);
		}
	}
}

new VideosFormatter();
