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

var FCKInsertExtensionCommand = function() {}

FCKInsertExtensionCommand.prototype =
{
	GetState : MindTouchDeki.GetState,

	Execute : function()
	{
		// setup the variables to pass to the dialog window
		var oParams = {
			'sSelection' : MindTouchDeki.GetSelectedText(),
			'elParent'   : FCKSelection.GetParentElement()
		};

		var url = FCKConfig.DekiCommonPath + '/popups/extension_dialog.php';
		MindTouchDeki.OpenDialog( 'InsertExtension', url, "700px", "400px", oParams, this._InsertExtension, this );
	},

	_InsertExtension : function( oParams )
	{
		FCKUndo.SaveUndoStep() ;

		if (oParams.sDekiScript)
		{
			FCK.InsertHtml( FCKTools.HTMLEncode( oParams.sDekiScript ) );
		}
	}
}

FCKCommands.RegisterCommand( 'MindTouchDeki_InsertExtension', new FCKInsertExtensionCommand );

var oInsertExtensionItem = new FCKToolbarButton( 'MindTouchDeki_InsertExtension', FCKLang.InsertExtensionLbl, FCKLang.InsertExtension, FCK_TOOLBARITEM_ICONTEXT, false, true, [ FCKConfig.DekiCommonPath + "/icons/icons.gif", 16, 21 ] ) ;
FCKToolbarItems.RegisterItem( 'MindTouchDeki_InsertExtension', oInsertExtensionItem ) ;
