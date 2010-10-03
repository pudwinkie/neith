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

Deki.Dialog.Wysiwyg = function(dialog)
{
	this.dialog = dialog;
};

Deki.Dialog.Wysiwyg.prototype =
{
	_pluginInfo : {
		name : "WYSIWYG plugin"
	},

	editor : null,

	BeforeOpen : function()
	{
		if ( ! Deki.Editor || ! Deki.Editor.IsStarted )
			return ;

		if ( typeof FCKeditorAPI != "undefined" )
		{
			this.editor = FCKeditorAPI.GetInstance('editarea');
			this.editor.FocusManager.Lock() ;

			if ( YAHOO.env.ua.ie && this.editor )
			{
				try
				{
					this.editor.Selection.Save( true ) ;
				}
				catch (e) {}
			}

			return;
		}
		else if ( typeof CKEDITOR != "undefined" )
		{
			this.editor = CKEDITOR.instances.editarea;
			
			if ( this.editor.mode == 'wysiwyg' && CKEDITOR.env.ie )
			{
				var selection = this.editor.getSelection();
				
				if ( selection )
					selection.lock();
				
				/*
				 * IE BUG: If the initial focus went into a non-text element (e.g. button),
				 * then IE would still leave the caret inside the editing area.
				 */
				var $selection = this.editor.document.$.selection,
				$range = $selection.createRange();

				if ( $range )
				{
					if ( $range.parentElement && $range.parentElement().ownerDocument == this.editor.document.$
					  || $range.item && $range.item( 0 ).ownerDocument == this.editor.document.$ )
					{
						var $myRange = document.body.createTextRange();
						$myRange.moveToElementText( this.dialog._oContainer );
						$myRange.collapse( true );
						$myRange.select();
					}
				}
			}
		}
	},

	BeforeClose : function()
	{
		if ( ! Deki.Editor || ! Deki.Editor.IsStarted )
			return ;

		if ( typeof FCKeditorAPI != "undefined" && this.editor )
		{
			this.editor.FocusManager.Unlock() ;

			if ( YAHOO.env.ua.ie )
			{
				this.editor.Selection.Restore() ;
				this.editor.Selection.Release() ;
			}
			else
			{
				try
				{
					this.editor.Focus();
				} catch (e) {}
			}

			if ( !this.editor.EditMode )
			{
				var editor = this.editor;
				setTimeout( function()
					{
						editor.Events.FireEvent( 'OnSelectionChange' ) ;
					}, 0 ) ;
			}
		}
		else if ( typeof CKEDITOR != "undefined" && this.editor )
		{
			this.editor.focus();
			
			if ( this.editor.mode == 'wysiwyg' && CKEDITOR.env.ie )
			{
				var selection = this.editor.getSelection();
				
				if ( selection )
					selection.unlock( true );
			}
		}
	}
};
