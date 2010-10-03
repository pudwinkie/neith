<?php
require_once('gui_index.php');

class AttachmentsFormatter extends DekiFormatter
{
	protected $contentType = 'application/json';
	protected $requireXmlHttpRequest = true;

	private $Request;

	public function format()
	{
		$this->Request = DekiRequest::getInstance();

		$action = $this->Request->getVal( 'action' );

		$result = '';

		switch ($action)
		{
			case 'getbyids':
				$result = $this->getByIds();
				break;
			case 'delete':
				$result = $this->delete();
				break;
			default:
				header('HTTP/1.0 404 Not Found');
				exit(' '); // flush the headers
		}
		
		$this->disableCaching();

		echo $result;
	}

	private function getByIds()
	{
		global $wgDekiPlug;

		$fileIds = $this->Request->getVal( 'fileIds' );

		$fileIds = explode( ',', $fileIds );
		$fileIds = array_filter( $fileIds, array( $this, 'filterEmptyIds' ) );

		$files = array();

		foreach ( $fileIds as $fileId )
		{
			$Preview = DekiFilePreview::newFromId($fileId);

			if (!is_null($Preview))
			{
				$fileInfo = array(
					'href' => $Preview->getHref()
				);
				
				if ($Preview->hasPreview())
				{
					$fileInfo['width'] = $Preview->getWidth();
					$fileInfo['height'] = $Preview->getHeight();
				}
				
				$files[] = $fileInfo;
			}
		}

		return json_encode( $files );
	}

	private function delete()
	{
		$fileId = $this->Request->getInt('fileId');
		$result = DekiFile::delete($fileId);

		if ($result === true)
		{
			wfMessagePush('files', wfMsg('Skin.Common.file-deleted'), 'success');
		}
		else
		{
			// bubble exceptions
			$Result->handleResponse();
		}
	}

	private function filterEmptyIds($id)
	{
		return ! empty($id);
	}
}

new AttachmentsFormatter();
