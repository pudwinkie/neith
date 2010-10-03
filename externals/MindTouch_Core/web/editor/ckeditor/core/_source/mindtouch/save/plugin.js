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

/**
 * @file Save plugin.
 */

(function()
{
	function unmaximize( editor )
	{
		var maximizeCommand = editor.getCommand( 'maximize' );

		if ( maximizeCommand && maximizeCommand.state == CKEDITOR.TRISTATE_ON )
		{
			editor.execCommand( 'maximize' );
		}
	}

	var saveCmd =
	{
		modes : {wysiwyg : 1, source : 1},
		
		editorFocus : false,

		exec : function( editor )
		{
			this.editor = editor;

			unmaximize( editor );

			if ( Deki.Editor && Deki.Editor.CheckPermissions )
			{
				Deki.Editor.CheckPermissions( this.save, this );
			}
			else
			{
				this.save();
			}
		},

		save : function()
		{
			var editor = this.editor,
				$form = editor.element.$.form;

			if ( $form && !editor.config.mindtouch.isReadOnly )
			{
				editor.fire( 'save' );

				try
				{
					$form.submit();
				}
				catch( e )
				{
					// If there's a button named "submit" then the form.submit
					// function is masked and can't be called in IE/FF, so we
					// call the click() method of that button.
					if ( $form.submit.click )
						$form.submit.click();
				}
			}
		},
		
		canUndo : false
	};
	
	var cancelCmd =
	{
		modes : {wysiwyg : 1, source : 1},
		
		editorFocus : false,

		exec : function( editor )
		{
			unmaximize( editor );
			editor.fire( 'cancel' );
		},
		
		canUndo : false
	};

	var pluginName = 'mindtouchsave';

	CKEDITOR.plugins.add( pluginName,
	{
		lang : [ 'en', 'cs', 'de', 'en-au', 'es', 'et', 'fr-ca', 'fr', 'he', 'hu', 'it', 'ja', 'ko', 'nl', 'pt-br', 'ru', 'sv' ],
		
		init : function( editor )
		{
			editor.addCommand( pluginName, saveCmd );
			editor.addCommand( 'mindtouchcancel', cancelCmd );
			
			var keystrokes = editor.keystrokeHandler.keystrokes;
			keystrokes[ CKEDITOR.CTRL + CKEDITOR.SHIFT + 83 /*S*/ ] = pluginName;

			editor.ui.addButton( 'MindTouchSave',
				{
					label : editor.lang.save,
					command : pluginName
				});
			
			editor.ui.addButton( 'MindTouchCancel',
					{
						label : editor.lang.cancel,
						command : 'mindtouchcancel',
						icon : editor.config.mindtouch.commonPath + '/icons/icons.gif',
						iconOffset : 100
					});
		}
	});
})();
