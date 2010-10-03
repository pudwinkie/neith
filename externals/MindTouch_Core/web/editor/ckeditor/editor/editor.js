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

Deki.CKEditor = function( sEditAreaId )
{
	this.Name = "CKEditor";
	this.constructor.superclass.constructor.call( this, sEditAreaId );
}

YAHOO.lang.extend( Deki.CKEditor, DekiEditor );

Deki.CKEditor.prototype.CreateEditor = function()
{
	var oSelf = this;

	var $newPageTitle = Deki.$( '#deki-new-page-title' );
	var contentsCss = [CKEDITOR.basePath + 'contents.css', Deki.EditorPath + '/editor/contents.css'];
	
	// Configuration
	var config = 
		{
			customConfig : Deki.EditorPath + '/editor/config.js',
			contentsCss : contentsCss.concat( Deki.EditorStyles ),
			mindtouch :
			{
				commonPath : Deki.PathCommon,
				userName : Deki.UserName,
				today : Deki.Today,
				isReadOnly : this.ReadOnly,
				pageTitle : Deki.PageTitle,
				pageId : Deki.PageId
			},
			
			on :
			{
	            save : function( ev )
	            {
	            	oSelf.Save.call(oSelf);
	            },
	            
	            cancel : function( ev )
	            {
	            	oSelf.Cancel.call(oSelf);
	            },

				instanceReady : function( ev )
				{
					$newPageTitle.focus();
					$newPageTitle.select();
				}
			}
		};
	
	if ( Deki.EditorLang )
	{
		var EditorLang = Deki.EditorLang;
		
		if ( !(EditorLang in CKEDITOR.lang.languages) )
		{
			EditorLang = Deki.EditorLang.split('-');
			EditorLang = EditorLang[0];
		}
		
		if ( EditorLang in CKEDITOR.lang.languages )
		{
			config.language = EditorLang;
		}
	}
	
	config.toolbar = ( this.ReadOnly ) ? 'ReadOnly' : FCKToolbarSet;
	
	// Skin styles
	CKEDITOR.document.appendStyleSheet( Deki.EditorPath + '/editor/skin.css' );
	
	var mindtouchSourcePath = CKEDITOR.basePath + '_source/mindtouch';
	
	// Plug-ins
	CKEDITOR.plugins.addExternal( 'atd', mindtouchSourcePath + '/atd/', null );
	CKEDITOR.plugins.addExternal( 'attachimage', mindtouchSourcePath + '/attachimage/', null );
	CKEDITOR.plugins.addExternal( 'autogrow', mindtouchSourcePath + '/autogrow/', null );
	CKEDITOR.plugins.addExternal( 'ckoverrides', mindtouchSourcePath + '/ckoverrides/', null );
	CKEDITOR.plugins.addExternal( 'combobutton', mindtouchSourcePath + '/combobutton/', null );
	CKEDITOR.plugins.addExternal( 'definitionlist', mindtouchSourcePath + '/definitionlist/', null );
	CKEDITOR.plugins.addExternal( 'dsbar', mindtouchSourcePath + '/dsbar/', null );
	CKEDITOR.plugins.addExternal( 'extensions', mindtouchSourcePath + '/extensions/', null );
	CKEDITOR.plugins.addExternal( 'floatingtoolbar', mindtouchSourcePath + '/floatingtoolbar/', null );
	CKEDITOR.plugins.addExternal( 'inlinestyle', mindtouchSourcePath + '/inlinestyle/', null );
	CKEDITOR.plugins.addExternal( 'label', mindtouchSourcePath + '/label/', null );
	CKEDITOR.plugins.addExternal( 'menubuttons', mindtouchSourcePath + '/menubuttons/', null );
	CKEDITOR.plugins.addExternal( 'mindtouch', mindtouchSourcePath + '/mindtouch/', null );
	CKEDITOR.plugins.addExternal( 'mindtouchdialog', mindtouchSourcePath + '/dialog/', null );
	CKEDITOR.plugins.addExternal( 'mindtouchimage', mindtouchSourcePath + '/image/', null );
	CKEDITOR.plugins.addExternal( 'mindtouchlink', mindtouchSourcePath + '/link/', null );
	CKEDITOR.plugins.addExternal( 'mindtouchkeystrokes', mindtouchSourcePath + '/keystrokes/', null );
	CKEDITOR.plugins.addExternal( 'mindtouchsave', mindtouchSourcePath + '/save/', null );
	CKEDITOR.plugins.addExternal( 'mindtouchtemplates', mindtouchSourcePath + '/templates/', null );
	CKEDITOR.plugins.addExternal( 'styletools', mindtouchSourcePath + '/style/', null );
	CKEDITOR.plugins.addExternal( 'tableadvanced', mindtouchSourcePath + '/table/', null );
	CKEDITOR.plugins.addExternal( 'tableconvert', mindtouchSourcePath + '/tableconvert/', null );
	CKEDITOR.plugins.addExternal( 'tablesort', mindtouchSourcePath + '/tablesort/', null );
	CKEDITOR.plugins.addExternal( 'transformations', mindtouchSourcePath + '/transformations/', null );
	CKEDITOR.plugins.addExternal( 'video', mindtouchSourcePath + '/video/', null );

	var createEditorInstance = function()
	{
		// Create an instance
		this.Instance = CKEDITOR.replace( this.EditArea, config );

		$newPageTitle.keypress( function( ev )
			{
				if ( ev.which == 13 )
				{
					try
					{
						oSelf.Instance.focus();
					}
					catch( ex ) {}

					return false;
				}
			});
	}
	
	if ( ! /(&|\?)cksource=true/.test( window.location.search ) )
	{
		CKEDITOR.scriptLoader.load( CKEDITOR.basePath + 'mindtouch.js', createEditorInstance, this );
	}
	else
	{
		createEditorInstance.call( this );
	}
}

Deki.CKEditor.prototype.BeforeCancel = function()
{
	if ( this.Instance )
	{
		this.Instance.destroy();
		this.Instance = null;
	}
}

Deki.CKEditor.prototype.IsChanged = function()
{
	if ( this.IsStarted && (( this.Instance && this.Instance.checkDirty() ) ||
		( this.InitContent && this.InitContent != Deki.$( "#" + this.EditArea ).val() ) ||
		Deki.$( '#wpArticleSaveFailed' ).val() == 'true' ) )
	{
		return true;
	}
	
	return false;
}

Deki.CKEditor.prototype.IsSupproted = function()
{
	return CKEDITOR.env.isCompatible;
}

Deki.Editor = new Deki.CKEditor( 'editarea' );
