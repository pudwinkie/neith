<?php
require_once('gui_index.php');
require_once('EditPage.php');

require_once($IP . $wgDekiPluginPath . '/deki_plugin.php');
// load plugins
DekiPlugin::loadSitePlugins();

class LoadEditor extends DekiFormatter
{
	protected $contentType = 'application/json';
	protected $requireXmlHttpRequest = true;

	public function format()
	{
		global $wgTitle, $wgArticle, $wgOut;
		global $wgScriptPath;
		global $IP, $wgEditors, $wgDefaultEditor, $wgEditor;

		$Request = DekiRequest::getInstance();

		$text = $Request->getVal( 'text' );
		$pageId = $Request->getVal( 'pageId' );
		$sectionId = $Request->getVal( 'sectionId' );
		$redirect = $Request->getVal( 'redirect' );

		$sectionId = ( empty( $sectionId ) ) ? null : $sectionId;
		$pageId = ( empty( $pageId ) ) ? 0 : (int) $pageId;

		$wgTitle = Title::newFromText( $text );
		
		$Article = new Article( $wgTitle );
		$Article->mRedirected = $redirect === 'no';
		$Article->loadContent( 'edit', $sectionId );
		
		if ( $pageId > 0 )
		{
			$wgArticle = $Article;
		}

		$editorScripts = array();
		$script = '';

		$hookResult = DekiPlugin::executeHook( Hooks::EDITOR_LOAD, array( $Article, &$editorScripts, &$script ) );
		
		$lang = $this->getLanguage($Article);
		
		if ($lang) 
		{
			$script .= "Deki.EditorLang = '" . $lang . "';";
		}
		
		// if hook is unhandled or is unregistred, load default editor
		if ( DekiPlugin::UNHANDLED == $hookResult || DekiPlugin::UNREGISTERED == $hookResult )
		{
			$wgEditor = ( isset($wgEditor) && isset($wgEditors[$wgEditor]) ) ? $wgEditor : $wgDefaultEditor;

			$editorDirectory = $wgScriptPath . '/editor' . $wgEditors[$wgEditor]['directory'];

			$editorScripts[] = $editorDirectory . '/core/' . $wgEditors[$wgEditor]['core']; // core of the editor
			$editorScripts[] = $editorDirectory . '/editor/editor.js'; // default editor class

			$script .= "Deki.EditorPath = '" . $editorDirectory . "';";

			if ( $wgEditor == 'FCKeditor' || $wgEditor == 'CKEditor' )
			{
				global $wgFCKToolbarSet, $wgFCKFloatingToolbar;

				$script .= "FCKToolbarSet = '" . $wgFCKToolbarSet . "';";

				$FCKToolbarLocation = ( $wgFCKFloatingToolbar ) ? "Out:xToolbar" : "In";
				$script .= "FCKToolbarLocation = '" . $FCKToolbarLocation . "';";
			}
		}
		
		if ( $Request->getVal( 'source' ) == 'true' )
		{
			$script .= "Deki.EditorReadOnly = true;";
		}
		
		if ( $Request->getVal( 'editor' ) == 'false' )
		{
			$script .= "Deki.EditorWysiwyg = false;";
		}

		$script .= Skin::jsLangVars( array('GUI.Editor.alert-changes-made-without-saving'), false );

		$script = '<script type="text/javascript">' . $script . '</script>';
		
		// add parent editor class
		// every editor class should extend this class
//		$commonDirectory = Skin::getCommonPath();
//		array_unshift($editorScripts, $commonDirectory . '/editor.js');

		$edit = new EditPage( $Article );
		$edit->textbox1 = $Article->getContent( true );
		$edit->setSection( $sectionId );

		// adds html to wgOut
		$edit->editForm( 'edit', false, true );

		$editorContent = array(
			'edittime' => $Article->getTimestamp(),
			'content'  => $wgOut->getHTML(),
			'script'   => $script,
			'scripts'  => $editorScripts
		);

		// IE caches ajax calls...
		$this->disableCaching();

		echo json_encode( $editorContent );
	}
	
	private function getLanguage($Article)
	{
		global $wgLanguageCode;
		
		$lang = null;
		
		if ( !is_null($Article->getLanguage()) )
		{
			$lang = $Article->getLanguage();
		}
		elseif ($wgLanguageCode)
		{
			$lang = strtolower($wgLanguageCode);
		}
		
		return $lang;
	}
}

new LoadEditor();
