/*
 * FCKeditor - The text editor for Internet - http://www.fckeditor.net
 * Copyright (C) 2003-2009 Frederico Caldeira Knabben
 *
 * == BEGIN LICENSE ==
 *
 * Licensed under the terms of any of the following licenses at your
 * choice:
 *
 *  - GNU General Public License Version 2 or later (the "GPL")
 *    http://www.gnu.org/licenses/gpl.html
 *
 *  - GNU Lesser General Public License Version 2.1 or later (the "LGPL")
 *    http://www.gnu.org/licenses/lgpl.html
 *
 *  - Mozilla Public License Version 1.1 or later (the "MPL")
 *    http://www.mozilla.org/MPL/MPL-1.1.html
 *
 * == END LICENSE ==
 *
 * This plugin contains overrode fck's core function.
 */
 
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

var FindReplace = function()
{
	this.FindRange = null ;
}

FindReplace.prototype =
{
	_Find : function( blockElement, searchString, isCaseSensitive, isMatchWord )
	{
		isCaseSensitive = ( isCaseSensitive ) ? isCaseSensitive : false ;
		isMatchWord = ( isMatchWord ) ? isMatchWord : false ;

		if ( !this.FindRange )
			this.FindRange = new FindReplace.CharacterRange( new FindReplace.CharacterCursor( blockElement ), searchString.length ) ;
		else
		{
			this.FindRange = this.FindRange.GetNextRange( searchString.length ) ;
		}
		var matcher = new FindReplace.KmpMatch( searchString, ! isCaseSensitive ) ;
		var matchState = FindReplace.KMP_NOMATCH ;
		var character = '%' ;

		while ( character != null )
		{
			while ( ( character = this.FindRange.GetEndCharacter() ) )
			{
				matchState = matcher.FeedCharacter( character ) ;
				if ( matchState == FindReplace.KMP_MATCHED )
					break ;
				if ( this.FindRange.MoveNext() )
					matcher.Reset() ;
			}

			if ( matchState == FindReplace.KMP_MATCHED )
			{
				if ( isMatchWord )
				{
					var cursors = this.FindRange.GetCursors() ;
					var head = cursors[ cursors.length - 1 ].Clone() ;
					var tail = cursors[0].Clone() ;
					if ( !head.MoveNext() && !FindReplace.CheckIsWordSeparator( head.GetCharacter() ) )
						continue ;
					if ( !tail.MoveBack() && !FindReplace.CheckIsWordSeparator( tail.GetCharacter() ) )
						continue ;
				}

				return true ;
			}
		}

		this.FindRange = null ;
		return false ;
	},

	Find : function( blockElement, searchString, isCaseSensitive, isMatchWord )
	{
		return this._Find( blockElement, searchString, isCaseSensitive, isMatchWord )
	},

	Replace : function( blockElement, searchString, replaceNodes, isCaseSensitive, isMatchWord )
	{
		var saveUndoStep = function( selectRange )
		{
			var ieRange ;
			if ( FCKBrowserInfo.IsIE )
				ieRange = document.selection.createRange() ;

			selectRange.Select() ;
			FCKUndo.SaveUndoStep() ;
			var cloneRange = selectRange.Clone() ;
			cloneRange.Collapse( false ) ;
			cloneRange.Select() ;

			if ( ieRange )
				setTimeout( function(){ ieRange.select() ; }, 1 ) ;
		}
		
		if ( this._Find( blockElement, searchString, isCaseSensitive, isMatchWord ) )
		{
			var range = this.FindRange.ToDomRange() ;
			var bookmark = range.CreateBookmark() ;

			range.MoveToBookmark( bookmark ) ;
		
			saveUndoStep( range ) ;
			range.DeleteContents() ;

			for ( var i = 0 ; i < replaceNodes.length ; i++ )
			{
				range.InsertNode( replaceNodes[i] ) ;
				range.MoveToPosition( replaceNodes[i], 4 ) ;
			}

			range._UpdateElementInfo() ;
		
			this.FindRange = CharacterRange.CreateFromDomRange( range ) ;
		}

		return ;
	},

	ReplaceAll : function( blockElement, searchString, replaceNodes, isCaseSensitive, isMatchWord, isSkipNowiki )
	{
		var replaceCount = 0 ;

		while ( this._Find( blockElement, searchString, isCaseSensitive, isMatchWord ) )
		{
			var range = this.FindRange.ToDomRange() ;

			// see #0005531: Editor expands ~~~ even when inside a <span class="plain"> element.
			var path = new FCKElementPath( range.StartNode ) ;
			var isPlain = false;
			for ( var i = 0 ; i < path.Elements.length ; i++ )
			{
				var pathElement = path.Elements[i] ;
				if ( /plain/.test(FCKDomTools.GetAttributeValue( pathElement, 'class' )) )
				{
					isPlain = true ;
					break ;
				}
			}

			var bookmark = range.CreateBookmark() ;
			range.MoveToBookmark( bookmark ) ;
			range.Select() ;

			if ( ! isPlain || isSkipNowiki )
			{
				FCKUndo.SaveUndoStep() ;

				range.DeleteContents() ;
				for ( var i = 0 ; i < replaceNodes.length ; i++ )
				{
					range.InsertNode( replaceNodes[i] ) ;
					range.MoveToPosition( replaceNodes[i], 4 ) ;
				}
				range._UpdateElementInfo() ;
			}
			else
			{
				range.Collapse( false ) ;
			}

			this.FindRange = FindReplace.CharacterRange.CreateFromDomRange( range ) ;
			replaceCount++ ;
		}

		return replaceCount ;
	}
}

FindReplace.GetNextNonEmptyTextNode = function( node, stopNode )
{
	while ( ( node = FCKDomTools.GetNextSourceNode( node, false, 3, stopNode ) ) && node && node.length < 1 )
		1 ;
	return node ;
}

FindReplace.CharacterCursor = function( arg )
{
	if ( arg.nodeType && arg.nodeType != 3 )
	{
		this._textNode = FindReplace.GetNextNonEmptyTextNode( arg, arg ) ;
		this._offset = 0 ;
	}
	else
	{
		this._textNode = arguments[0] ;
		this._offset = arguments[1] ;
	}
}
FindReplace.CharacterCursor.prototype =
{
	GetCharacter : function()
	{
		return ( this._textNode && this._textNode.nodeValue.charAt( this._offset ) ) || null ;
	},

	// Non-normalized.
	GetTextNode : function()
	{
		return this._textNode ;
	},

	// Non-normalized.
	GetIndex : function()
	{
		return this._offset ;
	},

	// Return value means whehther we've crossed a line break or a paragraph boundary.
	MoveNext : function()
	{
		if ( this._offset < this._textNode.length - 1 )
		{
			this._offset++ ;
			return false ;
		}

		var crossed = false ;
		var curNode = this._textNode ;
		while ( ( curNode = FCKDomTools.GetNextSourceNode( curNode ) )
				&& curNode && ( curNode.nodeType != 3 || curNode.length < 1 ) )
		{
			var tag = curNode.nodeName.toLowerCase() ;
			if ( FCKListsLib.BlockElements[tag] || tag == 'br' )
				crossed = true ;
		}

		this._textNode = curNode ;
		this._offset = 0 ;
		return crossed ;
	},

	// Return value means whehther we've crossed a line break or a paragraph boundary.
	MoveBack : function()
	{
		if ( this._offset > 0 && this._textNode.length > 0 )
		{
			this._offset = Math.min( this._offset - 1, this._textNode.length - 1 ) ;
			return false ;
		}

		var crossed = false ;
		var curNode = this._textNode ;
		while ( ( curNode = FCKDomTools.GetPreviousSourceNode( curNode ) )
				&& curNode && ( curNode.nodeType != 3 || curNode.length < 1 ) )
		{
			var tag = curNode.nodeName.toLowerCase() ;
			if ( FCKListsLib.BlockElements[tag] || tag == 'br' )
				crossed = true ;
		}

		this._textNode = curNode ;
		this._offset = curNode && curNode.length - 1 ;
		return crossed ;
	},

	Clone : function()
	{
		return new FindReplace.CharacterCursor( this._textNode, this._offset ) ;
	}
} ;

FindReplace.CharacterRange = function( initCursor, maxLength )
{
	this._cursors = initCursor.push ? initCursor : [initCursor] ;
	this._maxLength = maxLength ;
}
FindReplace.CharacterRange.prototype =
{
	ToDomRange : function()
	{
		var firstCursor = this._cursors[0] ;
		var lastCursor = this._cursors[ this._cursors.length - 1 ] ;
		var domRange = new FCKDomRange( FCKTools.GetElementWindow( firstCursor.GetTextNode() ) ) ;
		var w3cRange = domRange._Range = domRange.CreateRange() ;
		w3cRange.setStart( firstCursor.GetTextNode(), firstCursor.GetIndex() ) ;
		w3cRange.setEnd( lastCursor.GetTextNode(), lastCursor.GetIndex() + 1 ) ;
		domRange._UpdateElementInfo() ;
		return domRange ;
	},

	MoveNext : function()
	{
		var next = this._cursors[ this._cursors.length - 1 ].Clone() ;
		var retval = next.MoveNext() ;
		if ( retval )
			this._cursors = [] ;
		this._cursors.push( next ) ;
		if ( this._cursors.length > this._maxLength )
			this._cursors.shift() ;
		return retval ;
	},

	MoveBack : function()
	{
		var prev = this._cursors[0].Clone() ;
		var retval = prev.MoveBack() ;
		if ( retval )
			this._cursors = [] ;
		this._cursors.unshift( prev ) ;
		if ( this._cursors.length > this._maxLength )
			this._cursors.pop() ;
		return retval ;
	},

	GetEndCharacter : function()
	{
		if ( this._cursors.length < 1 )
			return null ;
		var retval = this._cursors[ this._cursors.length - 1 ].GetCharacter() ;
		return retval ;
	},

	GetNextRange : function( len )
	{
		if ( this._cursors.length == 0 )
			return null ;
		var cur = this._cursors[ this._cursors.length - 1 ].Clone() ;
		cur.MoveNext() ;
		return new FindReplace.CharacterRange( cur, len ) ;
	},

	GetCursors : function()
	{
		return this._cursors ;
	}
} ;

FindReplace.CharacterRange.CreateFromDomRange = function( domRange )
{
	var w3cRange = domRange._Range ;
	var startContainer = w3cRange.startContainer ;
	var endContainer = w3cRange.endContainer ;
	var startTextNode, startIndex, endTextNode, endIndex ;

	if ( startContainer.nodeType == 3 )
	{
		startTextNode = startContainer ;
		startIndex = w3cRange.startOffset ;
	}
	else if ( domRange.StartNode.nodeType == 3 )
	{
		startTextNode = domRange.StartNode ;
		startIndex = 0 ;
	}
	else
	{
		startTextNode = FindReplace.GetNextNonEmptyTextNode( domRange.StartNode, domRange.StartNode.parentNode ) ;
		if ( !startTextNode )
			return null ;
		startIndex = 0 ;
	}

	if ( endContainer.nodeType == 3 && w3cRange.endOffset > 0 )
	{
		endTextNode = endContainer ;
		endIndex = w3cRange.endOffset - 1 ;
	}
	else
	{
		endTextNode = domRange.EndNode ;
		while ( endTextNode.nodeType != 3 )
			endTextNode = endTextNode.lastChild ;
		endIndex = endTextNode.length - 1 ;
	}

	var cursors = [] ;
	var current = new FindReplace.CharacterCursor( startTextNode, startIndex ) ;
	cursors.push( current ) ;
	if ( !( current.GetTextNode() == endTextNode && current.GetIndex() == endIndex ) && !domRange.CheckIsEmpty() )
	{
		do
		{
			current = current.Clone() ;
			current.MoveNext() ;
			cursors.push( current ) ;
		}
		while ( !( current.GetTextNode() == endTextNode && current.GetIndex() == endIndex ) ) ;
	}

	return new FindReplace.CharacterRange( cursors, cursors.length ) ;
}

// Knuth-Morris-Pratt Algorithm for stream input
FindReplace.KMP_NOMATCH = 0 ;
FindReplace.KMP_ADVANCED = 1 ;
FindReplace.KMP_MATCHED = 2 ;
FindReplace.KmpMatch = function( pattern, ignoreCase )
{
	var overlap = [ -1 ] ;
	for ( var i = 0 ; i < pattern.length ; i++ )
	{
		overlap.push( overlap[i] + 1 ) ;
		while ( overlap[ i + 1 ] > 0 && pattern.charAt( i ) != pattern.charAt( overlap[ i + 1 ] - 1 ) )
			overlap[ i + 1 ] = overlap[ overlap[ i + 1 ] - 1 ] + 1 ;
	}
	this._Overlap = overlap ;
	this._State = 0 ;
	this._IgnoreCase = ( ignoreCase === true ) ;
	if ( ignoreCase )
		this.Pattern = pattern.toLowerCase();
	else
		this.Pattern = pattern ;
}
FindReplace.KmpMatch.prototype = {
	FeedCharacter : function( c )
	{
		if ( this._IgnoreCase )
			c = c.toLowerCase();

		while ( true )
		{
			if ( c == this.Pattern.charAt( this._State ) )
			{
				this._State++ ;
				if ( this._State == this.Pattern.length )
				{
					// found a match, start over, don't care about partial matches involving the current match
					this._State = 0;
					return FindReplace.KMP_MATCHED;
				}
				return FindReplace.KMP_ADVANCED ;
			}
			else if ( this._State == 0 )
				return FindReplace.KMP_NOMATCH;
			else
				this._State = this._Overlap[ this._State ];
		}

		return null ;
	},

	Reset : function()
	{
		this._State = 0 ;
	}
};

/* Is this character a unicode whitespace or a punctuation mark?
 * References:
 * http://unicode.org/Public/UNIDATA/PropList.txt (whitespaces)
 * http://php.chinaunix.net/manual/tw/ref.regex.php (punctuation marks)
 */
FindReplace.CheckIsWordSeparator = function( c )
{
	if ( !c )
		return true;
	var code = c.charCodeAt( 0 );
	if ( code >= 9 && code <= 0xd )
		return true;
	if ( code >= 0x2000 && code <= 0x200a )
		return true;
	switch ( code )
	{
		case 0x20:
		case 0x85:
		case 0xa0:
		case 0x1680:
		case 0x180e:
		case 0x2028:
		case 0x2029:
		case 0x202f:
		case 0x205f:
		case 0x3000:
			return true;
		default:
	}
	return /[.,"'?!;:]/.test( c ) ;
}
