<?php
require_once('gui_index.php');

class DekiPropertiesFormatter extends DekiFormatter
{
	protected $contentType = 'application/json';
	protected $requireXmlHttpRequest = true;
	protected $disableCaching = true;

	private $Request;

	public function format()
	{
		$this->Request = DekiRequest::getInstance();
		$action = $this->Request->getVal('action');
		
		$result = '';

		switch ($action)
		{
			case 'edit':
				$result = $this->getEditHtml();
				break;
				
			case 'save':
				$result = $this->saveProperty();
				break;
			
			default:
				header('HTTP/1.0 404 Not Found');
				exit(' '); // flush the headers
		}
		
		echo json_encode($result);
	}

	private function getEditHtml()
	{
		$Table = new DomTable();
		$Tr = $Table->addRow();
		// remove any automagical classes
		$Tr->setAttribute('class', '');
		
		$Td = $Table->addCol('&nbsp;');
		$Td->addClass('name');
		$Td->setAttribute('colspan', 2);
		
		$Td = $Table->addCol(
			'<input type="text" name="name" />'
		);
		$Td->addClass('value');
			
		$Td = $Table->addCol('
			<button class="save"><span>'. wfMsg('Page.Properties.form.edit.save') .'</span></button>
			<button class="cancel"></span>' . wfMsg('Page.Properties.form.edit.cancel') . '</span></button>
		');
		$Td->addClass('edit');
		
		$result = array(
			'success' => true,
			'html' => $Tr->saveHtml()
		);
		
		return $result;
	}

	private function saveProperty()
	{
		$result = array(
			'success' => false
		);
		
		$id = $this->Request->getVal('id');
		$type = $this->Request->getEnum('type', array('page', 'user'), 'page');
		
		$propertyName = $this->Request->getVal('name');
		$propertyValue = $this->Request->getVal('value');
		
		if (!is_null($propertyName) && $propertyName != '')
		{
			$Properties = $type == 'user' ? new DekiUserProperties($id) : new DekiPageProperties($id);
			$Properties->setCustom($propertyName, $propertyValue);
			$Result = $Properties->update();
			
			if ($Result->isSuccess())
			{
				$result['success'] = true;
			}
			else
			{
				// failed
				$result['message'] = $Result->getError();
			}
		}
		else
		{
			$result['message'] = wfMsg('GUI.Properties.error.invalid');
		}

		return $result;
	}
}

new DekiPropertiesFormatter();
