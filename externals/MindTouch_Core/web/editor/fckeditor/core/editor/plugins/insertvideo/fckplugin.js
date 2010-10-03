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

var FCKInsertVideoCommand = function() {}

FCKInsertVideoCommand.prototype =
{
	GetState : MindTouchDeki.GetState,

	Execute : function()
	{
		var url = FCKConfig.DekiCommonPath + '/popups/popup-video.php';
		MindTouchDeki.OpenDialog( 'InsertVideo', url, "400px", "118px", null, this._InsertVideo, this );
	},

	_InsertVideo : function( param )
	{
		FCKUndo.SaveUndoStep() ;


		if ( param.f_content )
		{
			FCK.InsertHtml('{{media("' + param.f_content + '")}}');
		}

		return true;
	}
}

FCKCommands.RegisterCommand( 'MindTouchDeki_InsertVideo', new FCKInsertVideoCommand );

var oInsertVideoItem = new FCKToolbarButton( 'MindTouchDeki_InsertVideo', FCKLang.InsertVideoLbl, FCKLang.InsertVideo, null, false, true, FCKConfig.DekiCommonPath + "/icons/film.png" ) ;
FCKToolbarItems.RegisterItem( 'MindTouchDeki_InsertVideo', oInsertVideoItem ) ;
