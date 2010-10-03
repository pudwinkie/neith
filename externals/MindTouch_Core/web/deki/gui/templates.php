<?php
require_once('gui_index.php');

class TemplatesFormatter extends DekiFormatter
{
	protected $contentType = 'application/json';
	protected $requireXmlHttpRequest = true;

	private $Request;

	public function format()
	{
		$this->Request = DekiRequest::getInstance();

		$action = $this->Request->getVal('action');
		
		$result = '';
		
		switch ($action)
		{
			case 'getcontent':
				$result = $this->getContent();
				break;
			
			case 'getitems':
				$result = $this->getItems();
				echo json_encode($result);
				exit();
				break;
			
			default:
				header('HTTP/1.0 404 Not Found');
				exit(' '); // flush the headers
		}
		
		echo $result;
	}
		
	protected function getContent()
	{
		$templateId = $this->Request->getVal('templateId');
		$pageId = $this->Request->getVal('pageId');
		
		// Artilce::getParameters() returns GET keys
		// and Article::loadContent() sends it as param
		// see #0005839
		$this->Request->remove('pageId');
		$this->Request->remove('templateId');
		
		// default to no language
		$lang = null;

		if (!empty($pageId) && intval($pageId) > 0)
		{
			$Article = new Article(Title::newFromId($pageId));
			$lang = $Article->getLanguage();
		}

		$Article = new Article(Title::newFromId($templateId));
		$Article->loadContent('include', null, true, $lang);

		return $Article->mContent;
	}
	
	/**
	 * Generate the items list
	 * @return string
	 */
	protected function getItems()
	{
		$pageId = $this->Request->getVal('pageId');
		$items = '';

		$Result = DekiPlug::getInstance()->At('pages', $pageId, 'tree')->Get();
		if ($Result->isSuccess())
		{
			$page = $Result->getVal('body/pages/page');
			// convert to items string
			$key = $this->buildTemplateListItem($page);
			$keys = explode('|', $key);
			$name = array_shift($keys);
			$isSite = array_shift($keys);
			$items = implode('|', $keys);
		}
		
		return array(
			'items' => $items
		);
	}
	
	/**
	 * Builds the ridiculous key required for the template dialog
	 * @note copied from /skins/common/popups/select_template.php
	 * @param string $key
	 */
	private function buildTemplateListItem($page, $key = '')
	{
		$Page = new XArray($page);
		$href = $Page->getVal('subpages/@href', null);
		$subpages = $Page->getAll('subpages/page', array());
		if (empty($key))
		{
			$key = $Page->getVal('@id') . '|';
			if (empty($subpages))
			{
				$key .= is_null($href) ? '0|' : '2';
			}
			else
			{
				$key .= '1';
			}
		}
		
		foreach ($subpages as &$subpage)
		{
			$Info = DekiPageInfo::newFromArray($subpage);
			
			$paths = $Info->getParents();
			$templatePath = implode(HPS_SEPARATOR, $paths);
			
			$key .= '|' . $templatePath;
			$key = $this->buildTemplateListItem($subpage, $key);
		}
		
		return $key;
	}
}

new TemplatesFormatter();
