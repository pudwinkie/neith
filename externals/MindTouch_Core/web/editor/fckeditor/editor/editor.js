/*
 * MindTouch Deki - enterprise collaboration and integration platform
 * Copyright (C) 2006-2009 MindTouch, Inc.
 * www.mindtouch.com  oss@mindtouch.com
 *
 * For community documentation and downloads visit www.opengarden.org;
 * please review the licensing section.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along
 * with this program; if not, write to the Free Software Foundation, Inc.,
 * 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 * http://www.gnu.org/copyleft/gpl.html
 */

Deki.FCKEditor = function( sEditAreaId )
{
	this.Name = "FCKeditor";
	this.constructor.superclass.constructor.call( this, sEditAreaId );
}

YAHOO.lang.extend( Deki.FCKEditor, DekiEditor );

Deki.FCKEditor.prototype.CreateEditor = function()
{
	Deki.EditorStyles.push(Deki.EditorPath + "/editor/fck_editorarea.css");
	
	// Autogrow doesn't work in Opera. #0004447
	if ( !FCKToolbarLocation || Deki.$.browser.opera )
	{
		FCKToolbarLocation = 'In';
	}

	if ( FCKToolbarLocation != "In" )
	{
		this.CreateToolbarContainer();
	}

	var oFCKeditor = new FCKeditor( this.EditArea );

	oFCKeditor.Config["CustomConfigurationsPath"] = Deki.BaseHref + Deki.EditorPath
			+ "/editor/fckconfig.php";
	oFCKeditor.Config["EditorAreaCSS"] = Deki.EditorStyles;

	oFCKeditor.Config["ToolbarLocation"] = FCKToolbarLocation;

	if ( FCKToolbarLocation != "In" )
		oFCKeditor.Config["ToolbarCanCollapse"] = false;

	oFCKeditor.Config["StylesXmlPath"] = Deki.BaseHref + Deki.EditorPath
			+ "/editor/fckstyles.xml";

	oFCKeditor.ToolbarSet = ( this.ReadOnly ) ? 'Deki_ReadOnly' : FCKToolbarSet;
	
	if ( Deki.EditorLang )
	{
		var EditorLang = Deki.EditorLang;
		
		if ( typeof(Deki.FCKEditor.AvailableLanguages[Deki.EditorLang]) == 'undefined' )
		{
			EditorLang = Deki.EditorLang.split('-');
			EditorLang = EditorLang[0];
		}
		
		oFCKeditor.Config["DefaultLanguage"] = EditorLang;
	}
	
	oFCKeditor.Config["DekiCommonPath"] = Deki.PathCommon;
	oFCKeditor.Config["UserName"] = Deki.UserName;
	oFCKeditor.Config["Today"] = Deki.Today;
	oFCKeditor.Config["ReadOnly"] = this.ReadOnly;
	
	oFCKeditor.BasePath = Deki.BaseHref + Deki.EditorPath + '/core/';
	oFCKeditor.ReplaceTextarea();
}
	
Deki.FCKEditor.prototype.BeforeSave = function()
{
	this.RemoveToolbarContainer();
}
	
Deki.FCKEditor.prototype.BeforeCancel = function()
{
	this.RemoveToolbarContainer();
}

Deki.FCKEditor.prototype.RemoveToolbarContainer = function()
{
	if ( FCKToolbarLocation != "In" && this.ToolbarContainer )
	{
		Deki.$(this.ToolbarContainer).remove();
	}	
}
	
Deki.FCKEditor.prototype.CreateToolbarContainer = function()
{
	this.ToolbarContainer = document.createElement('div');
	this.ToolbarContainer.style.width = '100%';
	this.ToolbarContainer.style.height = '1%';

	this.Toolbar = document.createElement('div');
	this.Toolbar.id = 'xToolbar';
	this.Toolbar.style.position = 'relative';
	this.Toolbar.style.zIndex = '999';
	this.Toolbar.style.overflow = 'hidden';

	this.ToolbarContainer.appendChild(this.Toolbar);

	this.SectionToEdit.before(this.ToolbarContainer);
}

Deki.FCKEditor.prototype.FCKDockToolbar = function()
{
	var docScrollY = YAHOO.util.Dom.getDocumentScrollTop();
	var containerY = YAHOO.util.Dom.getY(this.ToolbarContainer);

	var newToolbarY = null;

	if ( docScrollY > containerY )
	{
		var oIFrame = YAHOO.util.Dom.get( this.EditArea + '___Frame' );
		var maxDock = YAHOO.util.Dom.getY(oIFrame) + oIFrame.offsetHeight
				- this.ToolbarContainer.offsetHeight;

		if ( docScrollY < maxDock )
		{
			newToolbarY = docScrollY - containerY;
		}
	}
	else
	{
		var toolbarY = YAHOO.util.Dom.getY( this.Toolbar );

		if ( toolbarY != containerY )
		{
			newToolbarY = 0;
		}
	}

	if ( !YAHOO.lang.isNull( newToolbarY ) )
	{
		$( this.Toolbar ).stop().animate( { top : newToolbarY + 'px' }, 'fast', 'linear' );
	}
}
	
Deki.FCKEditor.prototype.FCKOnComplete = function( editorInstance )
{
	this.Instance = editorInstance;
	this.Container = this.Instance.EditorWindow.parent.frameElement;
	
	var oSelf = this;

//	editorInstance.Events.AttachEvent('OnSave', function( editorInstance ) { oSelf.Save.apply( oSelf ) });
	editorInstance.Events.AttachEvent('OnAfterLinkedFieldUpdate', function( editorInstance ) { oSelf.Save.apply( oSelf ) });
	editorInstance.Events.AttachEvent('OnCancel', function( editorInstance ) { oSelf.Cancel.apply( oSelf ) });

	editorInstance.Config.PageTitle = Deki.PageTitle;
	editorInstance.Config.PageId = Deki.PageId;

	if ( FCKToolbarLocation != "In" )
	{
		if ( this.Toolbar )
		{
			YAHOO.util.Event.on(window, "scroll", this.FCKDockToolbar, this, true);
		}
	}
	else
	{
		editorInstance.EditorWindow.parent.frameElement.style.height = this.GetEditorHeight() + 'px';
	}

	$( '#deki-new-page-title' ).keypress( function( ev )
		{
			if ( ev.which == 13 )
			{
				oSelf.Instance.Focus();
				return false;
			}
		});
}
	
Deki.FCKEditor.prototype.IsChanged = function()
{
	if ( this.IsStarted && (( this.Instance && this.Instance.IsDirty() ) ||
		( this.InitContent && this.InitContent != Deki.$( "#" + this.EditArea ).val() ) ||
		Deki.$( '#wpArticleSaveFailed' ).val() == 'true' ) )
	{
		return true;
	}

	return false;
}

Deki.FCKEditor.prototype.IsSupproted = function()
{
	return FCKeditor_IsCompatibleBrowser();
}

Deki.Editor = new Deki.FCKEditor( 'editarea' );

function FCKeditor_OnComplete( editorInstance )
{
	Deki.Editor.FCKOnComplete( editorInstance );
}

Deki.FCKEditor.AvailableLanguages =
{
	af		: 'Afrikaans',
	ar		: 'Arabic',
	bg		: 'Bulgarian',
	bn		: 'Bengali/Bangla',
	bs		: 'Bosnian',
	ca		: 'Catalan',
	cs		: 'Czech',
	da		: 'Danish',
	de		: 'German',
	el		: 'Greek',
	en		: 'English',
	'en-au'	: 'English (Australia)',
	'en-ca'	: 'English (Canadian)',
	'en-uk'	: 'English (United Kingdom)',
	eo		: 'Esperanto',
	es		: 'Spanish',
	et		: 'Estonian',
	eu		: 'Basque',
	fa		: 'Persian',
	fi		: 'Finnish',
	fo		: 'Faroese',
	fr		: 'French',
	'fr-ca'	: 'French (Canada)',
	gl		: 'Galician',
	gu		: 'Gujarati',
	he		: 'Hebrew',
	hi		: 'Hindi',
	hr		: 'Croatian',
	hu		: 'Hungarian',
	is		: 'Icelandic',
	it		: 'Italian',
	ja		: 'Japanese',
	km		: 'Khmer',
	ko		: 'Korean',
	lt		: 'Lithuanian',
	lv		: 'Latvian',
	mn		: 'Mongolian',
	ms		: 'Malay',
	nb		: 'Norwegian Bokmal',
	nl		: 'Dutch',
	no		: 'Norwegian',
	pl		: 'Polish',
	pt		: 'Portuguese (Portugal)',
	'pt-br'	: 'Portuguese (Brazil)',
	ro		: 'Romanian',
	ru		: 'Russian',
	sk		: 'Slovak',
	sl		: 'Slovenian',
	sr		: 'Serbian (Cyrillic)',
	'sr-latn'	: 'Serbian (Latin)',
	sv		: 'Swedish',
	th		: 'Thai',
	tr		: 'Turkish',
	uk		: 'Ukrainian',
	vi		: 'Vietnamese',
	zh		: 'Chinese Traditional',
	'zh-cn'	: 'Chinese Simplified'
}

