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

var FCKInsertTemplateCommand = function() {}

FCKInsertTemplateCommand.prototype =
{
	GetState : MindTouchDeki.GetState,

	Execute : function()
	{
		var sUrl = FCKConfig.DekiCommonPath + '/popups/select_template.php';
		var oParams = { contextTopicID : FCKConfig.PageId };

		MindTouchDeki.OpenDialog( 'InsertTemplate', sUrl, "400px", "110px", oParams, this._InsertTemplate, this );
	},

	_InsertTemplate : function( param )
	{
		if ( param.f_template )
		{
			var oRange = new FCKDomRange( FCK.EditorWindow ) ;
			oRange.MoveToSelection() ;
			
			var node = oRange.StartBlock, clone ;
			
			if ( node && oRange.CheckIsCollapsed() && node.nodeName.toLowerCase() == 'p' )
			{
				node = oRange.StartBlock ;
				clone = FCKDomTools.CloneElement( node ) ;
				
				clone.innerHTML = node.innerHTML ;
				clone.innerHTML = clone.innerHTML.replace( /&nbsp;/ig, ' ' ) ;
				FCKDomTools.TrimNode( clone ) ;				
			}
			
			FCK.InsertHtml( param.f_template ) ;
			
			if ( clone )
			{
				FCKDomTools.TrimNode( node ) ;
				
				var lastNode = FCKDomTools.GetLastChild( node ) ;
				
				// remove empty paragraph
				// see #5998
				if ( clone.innerHTML.length == 0 )
				{
					FCKDomTools.RemoveNode( node, true ) ;
				}
				
				if ( lastNode )
				{
					oRange.MoveToElementEditEnd( lastNode ) ;
					oRange.Select() ;
				}
			}
		}
	}
}

FCKCommands.RegisterCommand( 'MindTouchDeki_InsertTemplate', new FCKInsertTemplateCommand );

var oInsertTemplateItem = new FCKToolbarButton( 'MindTouchDeki_InsertTemplate', FCKLang.Templates, FCKLang.Templates, null, false, true, FCKConfig.DekiCommonPath + "/icons/ed_template.gif" ) ;
FCKToolbarItems.RegisterItem( 'MindTouchDeki_InsertTemplate', oInsertTemplateItem ) ;
