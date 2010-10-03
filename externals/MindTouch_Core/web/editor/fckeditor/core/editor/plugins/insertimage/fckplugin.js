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

var FCKInsertImageCommand = function() {}

FCKInsertImageCommand.prototype =
{
	GetState : MindTouchDeki.GetState,

	Execute : function()
	{
		this._Image = FCK.Selection.GetSelectedElement() ;

		if ( this._Image && this._Image.tagName != 'IMG' && !( this._Image.tagName == 'INPUT' && this._Image.type == 'image' ) )
			this._Image = null ;

		// build the params object to pass to the dialog
		var oParams = new Object();
		if ( this._Image )
		{
			var Dom = parent.YAHOO.util.Dom;
			// determine the image wrapping
			var sWrap = '';
			if (Dom.hasClass(this._Image, 'rwrap'))
			{
				sWrap = 'right';
			}
			else if (Dom.hasClass(this._Image, 'lwrap'))
			{
				sWrap = 'left';
			}
			else
			{
				sWrap = 'default';
			}
			
			var nWidth = parseInt( FCKDomTools.GetCurrentElementStyle( this._Image, 'width' ) ) ||
				parseInt( this._Image.style.width ) || FCKDomTools.GetAttributeValue( this._Image, 'width' ) || 0 ;
			
			var nHeight = parseInt( FCKDomTools.GetCurrentElementStyle( this._Image, 'height' ) ) ||
				parseInt( this._Image.style.height ) || FCKDomTools.GetAttributeValue( this._Image, 'height' ) || 0 ;

			oParams = {
				'bInternal' : Dom.hasClass(this._Image, 'internal'),
				'sSrc' : this._Image.src,
				'sAlt' : this._Image.alt,
				'sWrap' : sWrap,
				'nWidth' : nWidth,
				'nHeight' : nHeight
			};
		}

		// general params regardless of the image state
		oParams.nPageId = FCKConfig.PageId;
		oParams.sUserName = FCKConfig.UserName;

		var url = FCKConfig.DekiCommonPath + '/popups/image_dialog.php?contextID=' + FCKConfig.PageId;

		if ( this._Image )
		{
			url += "&update=true" ;
		}

		MindTouchDeki.OpenDialog( 'InsertImage', url, "600px", "370px", oParams, this._InsertImage );
	},

	_InsertImage : function( oParams )
	{
		FCKUndo.SaveUndoStep();

		this._Image = FCK.InsertElement( 'img' );

		// try block for IE and bad images
		try
		{
			// set the image source
			this._Image.src = oParams.sSrc;
			this._Image.setAttribute( '_fcksavedurl', oParams.sSrc ) ;

			// set the image attributes
			if (oParams.nWidth || oParams.nHeight)
			{
				this._Image.style.width = oParams.nWidth + 'px';
				this._Image.style.height = oParams.nHeight + 'px';
			}

			if (oParams.sAlt)
			{
				this._Image.alt = oParams.sAlt;
			}

			var sInternalClass = (oParams.bInternal) ? 'internal ' : '';
			// >MT: Bugfix: 0002630: left floating image is not aligned properly
			this._Image.setAttribute('class', sInternalClass + oParams.sWrapClass);

			switch (oParams.sWrap)
			{
				case 'left':
				case 'right':
					this._Image.align = oParams.sWrap;
					break;
				default :
					if(this._Image.align)
					{
						this._Image.removeAttribute('align', 2);
					}
					break;
			}
			
			if ( oParams.sFullSrc && oParams.sFullSrc.length > 0 )
			{
				var elLink = FCKTools.GetElementAscensor( this._Image, 'A' ) ;

				if ( !elLink )
					elLink = FCK.EditorDocument.createElement( 'a' ) ;
				
				elLink.href = oParams.sFullSrc ;
				elLink.setAttribute( '_fcksavedurl', oParams.sFullSrc ) ;
				
				if ( oParams.sAlt )
					elLink.setAttribute( 'title', oParams.sAlt ) ;
				
				elLink = this._Image.parentNode.insertBefore( elLink, this._Image ) ;
				FCKDomTools.MoveNode( this._Image, elLink, true ) ;
			}
		}
		catch (e) {}
	}
}

FCKCommands.RegisterCommand( 'MindTouchDeki_InsertImage', new FCKInsertImageCommand );

var oInsertImageItem = new FCKToolbarButton( 'MindTouchDeki_InsertImage', FCKLang.InsertImageLbl, FCKLang.InsertImage, null, false, true, 37 ) ;
FCKToolbarItems.RegisterItem( 'MindTouchDeki_InsertImage', oInsertImageItem ) ;
