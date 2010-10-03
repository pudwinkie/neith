<?php
require_once('gui_index.php');

class UpdatesFormatter extends DekiFormatter
{
	protected $contentType = 'text/html';

	public function format() 
	{
		global $wgRssUrl, $wgCacheDirectory;
		if (empty($wgRssUrl)) 
		{
			echo('<!-- no rss feed set -->');
			return;
		}
		
		//cache RSS output
		define('MAGPIE_CACHE_DIR', $wgCacheDirectory);
		require_once('includes/magpie-0.72/rss_fetch.inc');
		$rss = fetch_rss( $wgRssUrl );
		if (empty($rss->items)) 
		{
			return;
		}
		//do very simple HTML output
		echo('<html>');
		echo('<head><title>'.$rss->channel['title'].'</title><link href="/skins/common/updates.css" rel="stylesheet" type="text/css" /><base target="_blank" /></head>');
		echo('<body id="updateBody">');
		foreach ($rss->items as $item) 
		{
			echo('<h2><span class="timestamp">'.date('Y/m/d', wfTimestamp(TS_UNIX, strtotime($item['updated']))).'</span>'
				.' - <a href="'.$item['link'].'">'.$item['title'].'</a></h2>');
			echo('<div class="content">'.$item['atom_content'].'</div>');
		}
		echo('</html>');
	}
}

new UpdatesFormatter();
