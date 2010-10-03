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

var FCKAttachImageCommand = function() {
}

FCKAttachImageCommand.prototype = {

	GetState : function()
	{
		if ( parent.Deki && parent.Deki.PageId == 0 )
			return FCK_TRISTATE_DISABLED ;
		
		return MindTouchDeki.GetState() ;
	},

	Execute : function()
	{
		// general params regardless of the image state
		oParams = {
			titleID : FCKConfig.PageId,
			commonPath : FCKConfig.DekiCommonPath
		}

		var Dialog = parent.Deki.Dialog ;
		
		var dialog = new Dialog();
		dialog.setConfig({
					'src' : FCKConfig.DekiCommonPath + '/popups/attach_dialog.php?filter=images',
					'width' : '650px',
					'height' : 'auto',
					'buttons': [
						Dialog.BTN_OK,
						Dialog.BTN_CANCEL
					],
					'args' : oParams,
					'callback' : this._InsertImage
				}) ;

		dialog.render() ;
		dialog.show() ;
	},

	_InsertImage : function( aFiles )
	{
		FCKUndo.SaveUndoStep() ;
		
		var fileIds = []; 
		
		for ( var i = 0 ; i < aFiles.length ; i++ )
		{
			if ( aFiles[i] !== false )
			{
				fileIds.push( aFiles[i] ) ;
			}
		}
		
		var data = {
			'fileIds' : fileIds.join( ',' )
		} ;
			
		MindTouchDeki.$.get( '/deki/gui/attachments.php?action=getbyids', data, function( files ) {
			
			var YAHOO = parent.YAHOO ;
			
			if ( YAHOO.lang.isArray( files ) )
			{
				for ( var i = 0 ; i < files.length ; i++ )
				{
					var file = files[i];
					
					if ( ! YAHOO.lang.isObject( file ) )
						continue;
					
					FCK.InsertElement( 'p' ) ;
					
					var oElement = null ;
					
					if ( file['width'] && file['height']  )
					{
						oElement = FCK.InsertElement( 'img' ) ;
						oElement.src = MindTouchDeki.StripHost( file['href'] ) ;
						oElement.style.width = file['width'] + 'px' ;
						oElement.style.height = file['height'] + 'px' ;
						oElement.alt = '' ;
					}
					else
					{
						oElement = FCK.InsertElement( 'a' ) ;
						
						var sUri = MindTouchDeki.StripHost( file['href'] ) ;
						oElement.href = oElement.innerHTML = oElement.title = sUri ;
					}
					
					MindTouchDeki.$( oElement ).addClass( 'internal' ) ;					
				}
				
            	if ( parent.Deki.Plugin && parent.Deki.Plugin.FilesTable )
            	{
            		parent.Deki.Plugin.FilesTable.Refresh( FCKConfig.PageId ) ;
            	}
            	
            	FCK.Events.FireEvent( 'OnSelectionChange' ) ;
			}

		}, 'json' ) ;
	}
}

FCKCommands.RegisterCommand( 'MindTouchDeki_AttachImage', new FCKAttachImageCommand ) ;

var oAttachImageItem = new FCKToolbarButton( 'MindTouchDeki_AttachImage', FCKLang.AttachImageLbl, FCKLang.AttachImage, null, false, true, [ FCKConfig.DekiCommonPath + "/icons/icons.gif", 16, 42 ] ) ;
FCKToolbarItems.RegisterItem( 'MindTouchDeki_AttachImage', oAttachImageItem ) ;
