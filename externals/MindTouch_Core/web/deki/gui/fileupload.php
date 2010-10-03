<?php
if (isset ($_POST['swfupload_sid']))
{
	session_id($_POST['swfupload_sid']);
}

require_once('gui_index.php');
// copied from index.php
// (guerrics) handles loading plugins and calling hooks
require_once($IP . $wgDekiPluginPath . '/deki_plugin.php');
// load plugins
DekiPlugin::loadSitePlugins();

/**
 * End point handles file uploads via form posts and flash posts
 */
class FileUpload extends DekiFormatter
{
	protected $contentType = 'text/html';
	
	public function format()
	{
		ini_set('max_execution_time', 0);
		
		$result = $this->upload();
		
		$this->disableCaching();
		echo json_encode($result);
	}
	
	protected function upload()
	{
		global $wgTitle, $wgUser;
		$Request = DekiRequest::getInstance();

		if (!$Request->isPost())
		{
			// GET requests just give the main form; no data.
			return array(
				'success' => false,
				'errorMsg' => 'Accepts only posted data.'
			);
		}

		if ($Request->getVal('uploader') == 'flash' && isset ($_SESSION['swfupload_token']))
		{
           	$authToken = $_SESSION['swfupload_token'];
			// authenticate the global plug with user credentials
			$wgUser = DekiUser::getCurrent($authToken);
		}

		$pageId = $Request->getVal('pageId');
		$wgTitle = Title::newFromID($pageId);

		if (is_null($wgTitle))
		{
			return array(
				'success' => false,
				'errorMsg' => 'Page was not found.'
			);
		}

		global $wgRequest, $IP;
		require_once($IP . '/deki/core/deki_file_upload.php');

		if (count($_POST) == 0)
		{
			// if $_POST is null probably post_max_size is exceeded
			MTMessage::Show(wfMsg('Article.Attach.data-exceeds-max-size', DekiFileUpload::getUploadLimit()), '');

			return array(
				'success' => false,
				'errorMsg' => 'Post size exceeded.'
			);
		}

		// attempt to upload all the attached files	
		$uploadResults = array();
		
		$success = false;
		foreach ($_FILES as $postField => $fileInfo)
		{
			if (!$wgRequest->getFileName('Filedata'))
			{
				// classic uploader
				$fileNum = str_replace('file_' , '', $postField);
				$fileDescription = $wgRequest->getVal('filedesc_' . $fileNum);
			}
			else
			{
				// flash uploader
				// We can't use other name if we use the SWFUpload
				// because it'll cause the problems with compatibility
				$fileDescription = $wgRequest->getVal('filedescription');
			}

			$File = DekiFileUpload::newFromPost($postField, $pageId, $fileDescription);
			if (!is_null($File))
			{
				// add a success message
				FlashMessage::push('files', wfMsg('Article.Attach.file-upload-success', $File->getName()), 'success');

				$uploadResults[] = $File->getId();
				$success = true;
			}
			else
			{
				$uploadResults[] = false;
			}
		}

		return array(
			'success' => $success,
			'files'	  => (array)$uploadResults
		);
	}
}

new FileUpload();
