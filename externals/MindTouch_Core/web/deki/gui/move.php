<?php
require_once('gui_index.php');

/**
 * Called when moving pages or files
 */
class MoveFormatter extends DekiFormatter
{
	protected $contentType = 'application/json';


	public function format()
	{
		global $wgDekiPlug, $wgRequest;
		$method = $wgRequest->getVal('method');

		switch ($method)
		{
			case 'page':
				if (empty($_POST))
				{
					$this->body = '{"status": "500", "body": "No data"}';
					echo $this->body;
					return;
				}

				$pageId = $wgRequest->getVal('nPageId', null);
				$newParentPath = $wgRequest->getVal('sNewPath', '');
				$newPath = $wgRequest->getVal('sNewTitle', '');
				$userName = $wgRequest->getVal('sUserName', '');

				$newTitle = Article::combineName($newParentPath, wfEncodeTitle($newPath));
				$Title = Title::newFromText($newTitle);

				$result = $wgDekiPlug->At('pages', $pageId, 'move')->With('to', $Title->getPrefixedUrl())->Post();
				
				if ($result['status'] != Plug::HTTPSUCCESS)
				{
					$error = wfArrayVal($result, 'body/error');
				
					$message = !empty($error['message']) ? $error['title'] .': '. $error['message'] :
														   wfMsg('System.Error.error') .': '. $result['status'];
					
					$json = array();
					$json['status'] = $result['status'];
					$json['body'] = $message;
				
					echo json_encode($json);
					return;
				}
				else
				{
					wfMessagePush('general', wfMsg('Article.Common.page-moved'), 'success');
					$pages = wfArrayValAll($result, 'body/pages.moved/page');
					// need to find the page that we just moved since many might be moved
					foreach ($pages as $page)
					{
						if ($page['@id'] == $pageId)
						{
							if (is_array($page['path']))
							{
								$location = isset($page['path']['#text']) ? $page['path']['#text']: '';
							}
							else
							{
								$location = $page['path'];
							}

							if (strncmp($location, '/', 1) != 0)
							{
								$location = '/' . $location;
							}
							
							// need to get the encoded title path
							$Title = Title::newFromText($location);

							$json = array();
							$json['status'] = $result['status'];;
							$json['body'] = $Title->getFullURL();
							
							echo json_encode($json);
							return;
						}
					}
				}
				
				break;

			case 'fileinfo':
				if (!isset($_GET['file_id']))
				{
					$this->body = '{"status": "500", "body": "No data"}';
					echo $this->body;
					return;
				}
				$fileId = (int)$_GET['file_id'];
				// retrieve the file name
				$result = $wgDekiPlug->At('files', $fileId, 'info')->Get();

				if ($result['status'] != Plug::HTTPSUCCESS)
				{
					$error = wfArrayVal($result, 'body/error');
					
					$message = !empty($error['message']) ? $error['title'] .': '. $error['message'] :
														   wfMsg('System.Error.error') .': '. $result['status'];
					$this->body = sprintf('{"status": "%s", "body": "%s"}', $result['status'], $message);

					echo $this->body;
					return;
				}
				else
				{
					$file = wfArrayVal($result, 'body/file');

					$this->body = sprintf('{"status": "%s", "body": "%s"}', $result['status'], $file['filename']);

					echo $this->body;
					return;
				}
				break;

			case 'file':
				if (empty($_POST))
				{
					$this->body = '{"status": "500", "body": "No data"}';
					echo $this->body;
					return;
				}
				else
				{
					// current page
					$pageId = $_POST['nPageId'];
					// the page to move the file to
					$newPageId = $_POST['nNewPageId'];
					$fileId = $_POST['nFileId'];
					$userName = $_POST['sUserName'];
					// allow file renaming
					$filename = isset($_POST['filename']) ? $_POST['filename'] : null;
				}
				
				$Plug = DekiPlug::getInstance()->At('files', $fileId, 'move')->With('to', $newPageId);
				if (!is_null($filename))
				{
					$Plug = $Plug->With('name', $filename);
				}
				$Result = $Plug->Post();

				$response = array();
				$response['status'] = $Result->getStatus();
				$response['message'] = $Result->getError();
				if ($Result->isSuccess())
				{
					// provide the redirect url
					$href = $Result->getVal('body/file/page.parent/uri.ui');
					$response['body'] = $href;
				}

				// display the response
				$this->body = json_encode($response);
				echo $this->body;
				return;

			default:
				$this->body = '';
				echo $this->body;
		}
	}
}

new MoveFormatter();
