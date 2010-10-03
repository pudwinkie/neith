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

var MindTouchDekiKeysStrokes = function( targetWindow )
{
	this.Window = targetWindow ;

	// Setup the Keystroke Handler.
	var oKeystrokeHandler = new FCKKeystrokeHandler( false ) ;
	oKeystrokeHandler._MindTouchDekiKeysStrokes = this ;
	oKeystrokeHandler.OnKeystroke = MindTouchDekiKeysStrokes_OnKeystroke ;

	oKeystrokeHandler.SetKeystrokes( [
		[ CTRL + 49 /*1*/, 'Header' ],
		[ CTRL + 50 /*2*/, 'Header' ],
		[ CTRL + 51 /*3*/, 'Header' ],
		[ CTRL + 52 /*4*/, 'Header' ],
		[ CTRL + 53 /*5*/, 'Header' ],
		[ CTRL + 48 /*0*/, 'Paragraph' ],
		[ CTRL + 78 /*N*/, 'Paragraph' ],
		[ CTRL + SHIFT + 76 /*L*/, 'InsertList' ],
		[ 9 /*TAB*/, 'Tab' ],
		[ CTRL + 9 /*TAB*/, 'Tab' ],
		[ SHIFT + 9 /*TAB*/, 'Tab' ]
	] ) ;

	oKeystrokeHandler.AttachToElement( targetWindow.document ) ;
}


function MindTouchDekiKeysStrokes_OnKeystroke( keyCombination, keystrokeValue )
{
	var oMindTouchDekiKeysStrokes = this._MindTouchDekiKeysStrokes ;
	try
	{
		switch ( keystrokeValue )
		{
			case 'Header' :
				var sHeader = 'h' + ( keyCombination - 1047 ) ; // h2 - h6
				return oMindTouchDekiKeysStrokes.ApplyStyle( FCKStyles.GetStyle( '_FCK_' + sHeader ) ) ;
				break ;
			case 'Paragraph' :
				return oMindTouchDekiKeysStrokes.ApplyStyle( FCKStyles.GetStyle( '_FCK_p' ) ) ;
				break ;
			case 'InsertList' :
				return oMindTouchDekiKeysStrokes.DoList() ;
				break ;
			case 'Tab' :
				return oMindTouchDekiKeysStrokes.DoTab( keyCombination ) ;
				break ;
		}
	}
	catch (e)
	{
		// If for any reason we are not able to handle it, go
		// ahead with the browser default behavior.
	}

	return false ;
}

MindTouchDekiKeysStrokes.prototype.ApplyStyle = function( style )
{
	FCKUndo.SaveUndoStep() ;

	FCK.Styles.ApplyStyle( style ) ;

	FCK.Focus() ;
	FCK.Events.FireEvent( 'OnSelectionChange' ) ;

	return true ;
}

MindTouchDekiKeysStrokes.prototype.DoList = function()
{
	var commandName ;

	if ( FCK_TRISTATE_ON == FCKCommands.GetCommand( 'InsertUnorderedList' ).GetState() )
	{
		commandName = 'InsertOrderedList' ;
	}
	else
	{
		if ( FCK_TRISTATE_ON == FCKCommands.GetCommand( 'InsertOrderedList' ).GetState() )
		{
			var parentElement = FCKSelection.GetParentElement() ;
			var parentPath = new FCKElementPath( parentElement ) ;

			var countList = 0 ;

			commandName = 'InsertOrderedList' ;

			for ( var i = 0 ; i < parentPath.Elements.length ; i++ )
			{
				var pathElement = parentPath.Elements[i] ;

				if ( pathElement.nodeName.IEquals( ['ul', 'ol'] ) )
				{
					countList++ ;
				}

				if ( countList > 1 )
				{
					commandName = 'InsertUnorderedList' ;
					break ;
				}
			}
		}
		else
		{
			commandName = 'InsertUnorderedList' ;
		}
	}

	if ( ! ( FCK_TRISTATE_DISABLED == FCKCommands.GetCommand( commandName ).GetState() ) )
	{
		FCKCommands.GetCommand( commandName ).Execute() ;
	}

	return true ;
}

MindTouchDekiKeysStrokes.prototype.DoTab = function( keyCombination )
{
	var hasShift = ( keyCombination == SHIFT + 9 ) ;
	var forceTab = ( keyCombination == CTRL + 9 ) ;

	var commandName = hasShift ? 'Outdent' : 'Indent' ;

	var oRange = new FCKDomRange( this.Window ) ;
	oRange.MoveToSelection() ;

	// we need to indent/outdent list items inside table cells
	// instead of jump to the next cell
	// @see #0007861
	var isListItem = false;

	var node = oRange._Range.startContainer ;
	while ( node )
	{
		if ( node.nodeType == 1 )
		{
			var tagName = node.tagName.toLowerCase() ;
			if ( tagName == "td" || tagName == "th" )
			{
				if ( isListItem )
				{
					var isStartOfBlock = oRange.CheckStartOfBlock();
					if ( isStartOfBlock || ( !isStartOfBlock && forceTab ) )
					{
						break; // while
					}
				}

				var eTd = node ;
				var nextTd = hasShift ? MindTouchDeki.GetPreviousSibling( eTd, ['TD', 'TH'] ) : MindTouchDeki.GetNextSibling( eTd, ['TD', 'TH'] ) ;

				if ( nextTd == null )
				{
					var eTr = hasShift ? MindTouchDeki.GetPreviousSibling( eTd.parentNode, ['TR'] ) : MindTouchDeki.GetNextSibling( eTd.parentNode, ['TR'] ) ;

					if ( eTr )
					{
						nextTd = hasShift ? FCKDomTools.GetLastChild( eTr, ['TD', 'TH'] ) : FCKDomTools.GetFirstChild( eTr, ['TD', 'TH'] ) ;
					}
				}

				if ( nextTd == null )
				{
					if ( hasShift )
					{
						break ; // while
					}

					FCKUndo.SaveUndoStep() ;
					
					FCKTableHandler.InsertRow( false ) ;
					var oRow = FCKSelection.MoveToAncestorNode( 'TR' ) ;

					if ( oRow && oRow.cells.length > 0 )
					{
						var nextTd = oRow.cells.item( 0 ) ;
					}
				}

				if ( nextTd != null )
				{
					oRange.MoveToNodeContents( nextTd ) ;
					oRange.Collapse() ;
					oRange.Select() ;
					return true ;
				}
				else
				{
					return false ;
				}
			}
			else if ( tagName == "tr" || tagName == "tbody" || tagName == "table" )
			{
				return false ;
			}
			else if ( tagName == "dt" || (tagName == "dd" && hasShift) )
			{
				FCKUndo.SaveUndoStep() ;
				
				var bookmark = oRange.CreateBookmark() ;
				var newNode = ( tagName == 'dt' ) ? 'dd' : 'dt' ;
				
				MindTouchDeki.RenameNode( node, newNode ) ;
				
				oRange.SelectBookmark( bookmark ) ;
				return true ;
			}
			else if ( tagName == 'li' )
			{
				isListItem = true ;
			}
		}

		node = node.parentNode ;
	}

	if ( oRange.StartBlock && /^h[1-6]$/i.test( oRange.StartBlock.tagName ) )
	{
		var eNext = FCKDomTools.GetNextSourceElement( oRange.StartBlock, false, null, [ 'hr','br' ], false ) ;

		if ( eNext )
		{
			oRange.MoveToNodeContents( eNext ) ;
			oRange.Collapse( true ) ;
			oRange.Select() ;
			oRange.Release() ;
		}

		return true ;
	}

	var tabSpaces = FCKConfig.DekiTabSpaces ;

	if ( ! oRange.CheckStartOfBlock() && tabSpaces > 0 )
	{
		if ( FCKConfig.TabSpaces > 0 || hasShift )
		{
			// Prevent adding of additional spaces
			return true ;
		}
		
		FCKUndo.SaveUndoStep() ;

		var sTabText = '' ;

		while ( tabSpaces-- > 0 )
			sTabText += '\xa0' ;

		oRange.DeleteContents() ;
		oRange.InsertNode( this.Window.document.createTextNode( sTabText ) ) ;
		oRange.Collapse( false ) ;
		oRange.Select() ;

		return true ;
	}

	if ( ! ( FCK_TRISTATE_DISABLED == FCKCommands.GetCommand( commandName ).GetState() ) )
	{
		FCKCommands.GetCommand( commandName ).Execute() ;
	}

	return true ;
}

function MindTouchDekiKeysHandler_SetHandler()
{
	if ( FCK.EditMode != FCK_EDITMODE_WYSIWYG )
		return ;

	FCK.MindTouchDekiKeysHandler = new MindTouchDekiKeysStrokes( FCK.EditorWindow ) ;
}

FCK.Events.AttachEvent( 'OnAfterSetHTML', MindTouchDekiKeysHandler_SetHandler ) ;

