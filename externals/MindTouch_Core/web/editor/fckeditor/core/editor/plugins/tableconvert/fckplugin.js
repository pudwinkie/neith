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

var ConvertTextToTable = function() {}

ConvertTextToTable.prototype =
{
	Execute : function()
	{
		var oDoc = FCK.EditorDocument ;

		// Save an undo snapshot before doing anything.
		FCKUndo.SaveUndoStep() ;

		var oRange = new FCKDomRange( FCK.EditorWindow ) ;
		oRange.MoveToSelection() ;

		var iterator = new FCKDomRangeIterator( oRange.Clone() ) ;
		var block ;

		var aParagraphs = [] ;

		while ( ( block = iterator.GetNextParagraph() ) )
		{
			aParagraphs.push( block ) ;
		}

		var node, position ;

		node = FCKTools.GetLastItem( aParagraphs ) ;

		if ( node )
		{
			node = node.nextSibling ;
			position = "before" ;
		}

		if ( ! node )
		{
			node = oDoc.body ;
			position = "child" ;
		}

		var eTable = oDoc.createElement( 'TABLE' ) ;
		eTable.cellSpacing = 1 ;
		eTable.cellPadding = 1 ;
		eTable.border = 1 ;

		if ( node )
		{
			switch ( position )
			{
				case "before": node.parentNode.insertBefore( eTable, node ) ; break ;
				case "child" : node.appendChild( eTable ) ; break ;
			}
		}

		for ( var i = 0 ; i < aParagraphs.length ; i++ )
		{
			var cell = eTable.insertRow(-1).insertCell(-1) ;
			cell.innerHTML = aParagraphs[i].innerHTML ;

			FCKDomTools.RemoveNode( aParagraphs[i] ) ;
		}

		oRange.SetStart( eTable.rows[0].cells[0], 1 ) ;

		var nRows = eTable.rows.length - 1 ;
		oRange.SetEnd( eTable.rows[nRows].cells[eTable.rows[nRows].cells.length - 1], 2 ) ;

		oRange.Select() ;
		oRange.Release() ;

		FCK.Focus() ;
		FCK.Events.FireEvent( 'OnSelectionChange' ) ;
	},

	GetState : function()
	{
		if ( FCK.EditorDocument != null )
		{
			var bIsTable = FCKSelection.HasAncestorNode( 'TABLE' ) ;
			
			switch ( this.Name )
			{
				case 'TableToText' :
					return ( bIsTable ) ? FCK_TRISTATE_OFF : FCK_TRISTATE_DISABLED ;
					break ;
				case 'TextToTable' :
					return ( bIsTable ) ? FCK_TRISTATE_DISABLED : FCK_TRISTATE_OFF ;				
					break ;
				default :
					return FCK_TRISTATE_OFF ;
			}
		}
		else
			return FCK_TRISTATE_DISABLED;
	}
}

FCKCommands.RegisterCommand( 'TableToText' , new FCKDialogCommand( FCKLang['TableToText'], FCKLang['TableToText'], FCKConfig.PluginsPath + 'tableconvert/tabletotext.html', 340, 170 ) ) ;
FCKCommands.RegisterCommand( 'TextToTable', new ConvertTextToTable );

FCK.ContextMenu.RegisterListener( {
	AddItems : function( menu, tag, tagName )
	{
		var bIsTable = ( tagName == 'TABLE' || FCKSelection.HasAncestorNode( 'TABLE' ) ) ;

		if ( bIsTable )
		{
			menu.AddSeparator() ;
			menu.AddItem( 'TableToText', FCKLang.TableToText, null, FCKCommands.GetCommand( 'Table' ).GetState() == FCK_TRISTATE_DISABLED ) ;
		}
		else if ( FCKCommands.GetCommand( 'Table' ).GetState() != FCK_TRISTATE_DISABLED )
		{
			menu.AddSeparator() ;
			menu.AddItem( 'TextToTable', FCKLang.TextToTable, null ) ;
		}
	}}
);
