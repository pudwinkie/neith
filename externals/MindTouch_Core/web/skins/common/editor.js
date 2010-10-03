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

var DekiEditor = function( sEditAreaId )
{
	this.EditArea = sEditAreaId;

	this.Instance = null;
	
	this.IsLoading = false;
	this.IsStarted = false;
	this.ReadOnly = false;

	this.SectionToEdit = null;
	this.CurrentSection = '';
	this.OldContent = null;
	this.InitContent = null;
	
	this.Container = null;

	// onbeforeunload sometimes fires twice in IE
	var onBeforeUnloadFired = false;

	var resetOnBeforeUnloadFired = function()
	{
		onBeforeUnloadFired = false;
	};

	window.onbeforeunload = function()
	{
		var result;
		if ( !onBeforeUnloadFired )
		{
			onBeforeUnloadFired = true;
			if ( Deki.Editor && Deki.Editor.IsChanged() )
			{
				result = wfMsg('GUI.Editor.alert-changes-made-without-saving');
			}
		}
		window.setTimeout(resetOnBeforeUnloadFired, 1000);
		return result;
	};

	this.Init();
}

DekiEditor.prototype = 
{
	Init: function()
	{
	},
	
	BeforeStart : function()
	{
	},
	
	Start : function( editorContent, sectionToEdit, currentSection )
	{
		this.SectionToEdit = ( sectionToEdit ) ? sectionToEdit : Deki.$( "#pageText" ); // edit page
		
		if ( this.SectionToEdit.length == 0 )
		{
			alert( 'You did not define the ID pageText in your skin.' );
			this.Cancel();
			return;
		}
		
		if ( YAHOO.lang.isString( currentSection ) )
		{
			this.CurrentSection = currentSection;
		}
		
		this.BeforeStart();
		
		this.OldContent = this.SectionToEdit.html();

		if ( YAHOO.lang.isValue( editorContent ) )
		{
			if ( this.SectionToEdit.find( '#' + this.EditArea ).length == 0 )
			{
				this.SectionToEdit.html( editorContent.content );
			}
			
			this.SectionToEdit.append( editorContent.script );
			
			Deki.$( '#wpEditTime' ).val( editorContent.edittime );
			Deki.$( '#wpSection' ).val( this.CurrentSection );
		}
		
		this.ReadOnly = Deki.EditorReadOnly || false;
		
		var wait = Deki.$( "#formLoading" );
		wait.show();

		this.Instance = null;
		
		if ( this.IsSupproted() && Deki.EditorWysiwyg !== false )
		{
			this.CreateEditor();
		}
		else
		{
			var oSelf = this;

			Deki.$( "#wpFormButtons input[name=doSave]" ).click(function() {
				var form = this.form;

				oSelf.CheckPermissions( function()
					{
						this.Save();
						form.submit();

					}, oSelf );
				
				return false;
			});

			Deki.$( "#wpFormButtons input[name=doCancel]" ).click(function() {
				oSelf.Cancel();
			});
			
			var textarea = Deki.$( "#" + this.EditArea );
			textarea.show();
			this.InitContent = textarea.val();
			Deki.$( "#wpFormButtons" ).show();
		}
		
		if (!Deki.PageNotRedirected) {
			Deki.$('#deki-page-title').addClass('ui-state-with-editor');
		}

		wait.hide();
		
		if ( !YAHOO.lang.isValue( this.CurrentSection ) )
			Deki.$( '.hideforedit' ).hide();
		
		this.IsStarted = true;
		this.IsLoading = false;
	},
	
	/**
	 * Creates the editor instance
	 * @abstract
	 */
	CreateEditor : function()
	{
	},
	
	IsSupproted : function()
	{
		return true;
	},

	/**
	 * Do an AJAX request to ensure that server is still up
	 * and user has permissions to save the page
	 *
	 * @param successCallback - function to call if check is success
	 * @param scope - the scope for successCallback
	 *
	 */
	CheckPermissions : function( successCallback, scope )
	{
		if ( this.ReadOnly )
		{
			return false;
		}

		scope = scope || this;

		Deki.$( '#quicksavewait' ).show();
		Deki.$.ajax(
			{
				url : Deki.Gui.ROOT_PATH + '/editor.php',
				data :
					{
						method : 'checkPermissions',
						pageId : Deki.PageId,
						pageTitle : Deki.PageTitle
					},
				dataType : 'json',
				success : function( data, status )
				{
					if ( status === 'success' && data.success === true )
					{
						successCallback.call( scope );
					}
					else
					{
						Deki.$( '#quicksavewait' ).hide();
						Deki.Ui.Message( data.message, data.body );
					}
				},
				error : function( xhr, textStatus, errorThrown )
				{
					Deki.$( '#quicksavewait' ).hide();
					Deki.Ui.Message( 'We are unable to save this page', 'A server error has occurred. To avoid losing your work, copy the page contents to a new file and retry saving again.' );
				}
			}
		);

		return true;
	},
	
	BeforeSave : function()
	{
	},
	
	Save : function()
	{
		if ( this.ReadOnly )
		{
			return false;
		}
		
		this.BeforeSave();

		Deki.$( '#quicksavewait' ).show();
		this.IsStarted = false;
	},

	BeforeCancel : function()
	{
	},
	
	Cancel : function()
	{
		var sCancelMessage;
		
		if ( !this.ReadOnly )
		{
			sCancelMessage = ( YAHOO.lang.isFunction( window.onbeforeunload ) ) ?
				window.onbeforeunload() : this.IsChanged();
		}
		
		if ( YAHOO.lang.isValue( sCancelMessage ) )
		{
			if ( !confirm("Are you sure you want to navigate away from the editor?\n\n"
					+ sCancelMessage
					+ "\n\nPress OK to continue, or Cancel to stay on the current editor.") )
			{
				return false;
			}
		}
		
		this.IsStarted = false;
		this.IsLoading = false;

		this.BeforeCancel();

		Deki.$( '#title' ).show(); // if we're editing an existing page
		Deki.$('.hideforedit').show();
		
		if (typeof Deki.CancelUrl != 'undefined')
		{
			window.location = Deki.CancelUrl;
		}
		else if ( YAHOO.lang.isBoolean( Deki.StaticEditor ) && Deki.StaticEditor )
		{
			window.history.back();
		}
		else if ( this.SectionToEdit )
		{
			if ( !YAHOO.lang.isNull(this.OldContent) )
			{
				this.SectionToEdit.html( this.OldContent );
			}
			
			HookSectionEditBehavior();
			
			this.SectionToEdit = null;
			this.Instance = null;
			this.CurrentSection = '';
			this.OldContent = null;
			this.InitContent = null;
		}
		
		Deki.$('#deki-page-title').removeClass('ui-state-with-editor');
		
		return true;
	},
	
	GetEditorHeight : function()
	{
		if ( !this.Container || YAHOO.lang.isValue( this.CurrentSection ) )
		{
			return 400;
		}

		var newHeight = YAHOO.util.Dom.getViewportHeight()
				- YAHOO.util.Dom.getY( this.Container )
				- ( YAHOO.env.ua.ie > 0 ? 7 : 12 );

		if ( newHeight < 400 )
		{
			newHeight = 400;
		}

		return newHeight;
	},
	
	IsChanged : function()
	{
	}
}
