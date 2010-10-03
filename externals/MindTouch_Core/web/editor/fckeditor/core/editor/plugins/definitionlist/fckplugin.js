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

var FCKDefinitionListCommand = function( name, tagName )
{
	this.Name = name ;
	this.TagName = tagName ;
}

FCKDefinitionListCommand.prototype =
{
	GetState : function()
	{
		// Disabled if not WYSIWYG or if the cursor is into header.
		if ( MindTouchDeki.GetState() == FCK_TRISTATE_DISABLED )
			return FCK_TRISTATE_DISABLED ;

		// We'll use the style system's convention to determine list state here...
		var startContainer = FCKSelection.GetBoundaryParentElement( true ) ;
		var listNode = startContainer ;
		
		while ( listNode )
		{
			if ( listNode.nodeName.IEquals( [ this.TagName ] ) )
				break ;
			listNode = listNode.parentNode ;
		}
		
		if ( listNode && listNode.nodeName.IEquals( this.TagName ) )
			return FCK_TRISTATE_ON ;
		else
			return FCK_TRISTATE_OFF ;
	},

	Execute : function()
	{
		FCKUndo.SaveUndoStep() ;

		var doc = FCK.EditorDocument ;
		var range = new FCKDomRange( FCK.EditorWindow ) ;
		range.MoveToSelection() ;
		var state = this.GetState() ;

		// Midas lists rule #1 says we can create a list even in an empty document.
		// But FCKDomRangeIterator wouldn't run if the document is really empty.
		// So create a paragraph if the document is empty and we're going to create a list.
		if ( state == FCK_TRISTATE_OFF )
		{
			FCKDomTools.TrimNode( doc.body ) ;
			if ( ! doc.body.firstChild )
			{
				var paragraph = doc.createElement( 'p' ) ;
				doc.body.appendChild( paragraph ) ;
				range.MoveToNodeContents( paragraph ) ;
			}
		}
		
		var bookmark = range.CreateBookmark() ;

		// Group the blocks up because there are many cases where multiple lists have to be created,
		// or multiple lists have to be cancelled.
		var listGroups = [] ;
		var markerObj = {} ;
		var iterator = new FCKDomRangeIterator( range ) ;
		var block ;

		iterator.ForceBrBreak = ( state == FCK_TRISTATE_OFF ) ;
		var nextRangeExists = true ;
		var rangeQueue = null ;
		while ( nextRangeExists )
		{
			while ( ( block = iterator.GetNextParagraph() ) )
			{
				var path = new FCKElementPath( block ) ;
				var listNode = null ;
				var processedFlag = false ;
				var blockLimit = path.BlockLimit ;

				// First, try to group by a list ancestor.
				for ( var i = path.Elements.length - 1 ; i >= 0 ; i-- )
				{
					var el = path.Elements[i] ;
					if ( el.nodeName.IEquals( ['dl'] ) )
					{
						// If we've encountered a list inside a block limit
						// The last group object of the block limit element should
						// no longer be valid. Since paragraphs after the list
						// should belong to a different group of paragraphs before
						// the list. (Bug #1309)
						if ( blockLimit._FCK_ListGroupObject )
							blockLimit._FCK_ListGroupObject = null ;

						var groupObj = el._FCK_ListGroupObject ;
						if ( groupObj )
							groupObj.contents.push( block ) ;
						else
						{
							groupObj = { 'root' : el, 'contents' : [ block ] } ;
							listGroups.push( groupObj ) ;
							FCKDomTools.SetElementMarker( markerObj, el, '_FCK_ListGroupObject', groupObj ) ;
						}
						processedFlag = true ;
						break ;
					}
				}

				if ( processedFlag )
					continue ;

				// No list ancestor? Group by block limit.
				var root = blockLimit ;
				if ( root._FCK_ListGroupObject )
					root._FCK_ListGroupObject.contents.push( block ) ;
				else
				{
					var groupObj = { 'root' : root, 'contents' : [ block ] } ;
					FCKDomTools.SetElementMarker( markerObj, root, '_FCK_ListGroupObject', groupObj ) ;
					listGroups.push( groupObj ) ;
				}
			}

			if ( FCKBrowserInfo.IsIE )
				nextRangeExists = false ;
			else
			{
				if ( rangeQueue == null )
				{
					rangeQueue = [] ;
					var selectionObject = FCKSelection.GetSelection() ;
					if ( selectionObject && listGroups.length == 0 )
						rangeQueue.push( selectionObject.getRangeAt( 0 ) ) ;
					for ( var i = 1 ; selectionObject && i < selectionObject.rangeCount ; i++ )
						rangeQueue.push( selectionObject.getRangeAt( i ) ) ;
				}
				if ( rangeQueue.length < 1 )
					nextRangeExists = false ;
				else
				{
					var internalRange = FCKW3CRange.CreateFromRange( doc, rangeQueue.shift() ) ;
					range._Range = internalRange ;
					range._UpdateElementInfo() ;
					if ( range.StartNode.nodeName.IEquals( 'td' ) )
						range.SetStart( range.StartNode, 1 ) ;
					if ( range.EndNode.nodeName.IEquals( 'td' ) )
						range.SetEnd( range.EndNode, 2 ) ;
					iterator = new FCKDomRangeIterator( range ) ;
					iterator.ForceBrBreak = ( state == FCK_TRISTATE_OFF ) ;
				}
			}
		}

		// Now we have two kinds of list groups, groups rooted at a list, and groups rooted at a block limit element.
		// We either have to build lists or remove lists, for removing a list does not makes sense when we are looking
		// at the group that's not rooted at lists. So we have three cases to handle.
		var listsCreated = [] ;
		while ( listGroups.length > 0 )
		{
			var groupObj = listGroups.shift() ;
			if ( state == FCK_TRISTATE_OFF )
			{
				if ( groupObj.root.nodeName.IEquals( ['dl'] ) )
					this._ChangeListType( groupObj, markerObj, listsCreated ) ;
				else
					this._CreateList( groupObj, listsCreated ) ;
			}
			else if ( state == FCK_TRISTATE_ON && groupObj.root.nodeName.IEquals( ['dl'] ) )
				this._RemoveList( groupObj, markerObj ) ;
		}

		// For all new lists created, merge adjacent, same type lists.
		for ( var i = 0 ; i < listsCreated.length ; i++ )
		{
			var listNode = listsCreated[i] ;
			var stopFlag = false ;
			var currentNode = listNode ;
			while ( ! stopFlag )
			{
				currentNode = currentNode.nextSibling ;
				if ( currentNode && currentNode.nodeType == 3 && currentNode.nodeValue.search( /^[\n\r\t ]*$/ ) == 0 )
					continue ;
				stopFlag = true ;
			}

			if ( currentNode && currentNode.nodeName.IEquals( this.TagName ) )
			{
				currentNode.parentNode.removeChild( currentNode ) ;
				while ( currentNode.firstChild )
					listNode.appendChild( currentNode.removeChild( currentNode.firstChild ) ) ;
			}

			stopFlag = false ;
			currentNode = listNode ;
			while ( ! stopFlag )
			{
				currentNode = currentNode.previousSibling ;
				if ( currentNode && currentNode.nodeType == 3 && currentNode.nodeValue.search( /^[\n\r\t ]*$/ ) == 0 )
					continue ;
				stopFlag = true ;
			}
			if ( currentNode && currentNode.nodeName.IEquals( this.TagName ) )
			{
				currentNode.parentNode.removeChild( currentNode ) ;
				while ( currentNode.lastChild )
					listNode.insertBefore( currentNode.removeChild( currentNode.lastChild ),
						       listNode.firstChild ) ;
			}
		}

		// Clean up, restore selection and update toolbar button states.
		FCKDomTools.ClearAllMarkers( markerObj ) ;
		range.MoveToBookmark( bookmark ) ;
		range.Select() ;

		FCK.Focus() ;
		FCK.Events.FireEvent( 'OnSelectionChange' ) ;
	},
	
	_ChangeListType : function( groupObj, markerObj, listsCreated )
	{
		var listArray = this.ListToArray( groupObj.root, markerObj ) ;
		var selectedListItems = [] ;
		for ( var i = 0 ; i < groupObj.contents.length ; i++ )
		{
			var itemNode = groupObj.contents[i] ;
			var newNode = MindTouchDeki.RenameNode( itemNode, this.TagName ) ;
		}
	},

	_CreateList : function( groupObj, listsCreated )
	{
		var contents = groupObj.contents ;
		var doc = FCKTools.GetElementDocument( groupObj.root ) ;
		var listContents = [] ;

		// It is possible to have the contents returned by DomRangeIterator to be the same as the root.
		// e.g. when we're running into table cells.
		// In such a case, enclose the childNodes of contents[0] into a <div>.
		if ( contents.length == 1 && contents[0] == groupObj.root )
		{
			var divBlock = doc.createElement( 'div' );
			while ( contents[0].firstChild )
				divBlock.appendChild( contents[0].removeChild( contents[0].firstChild ) ) ;
			contents[0].appendChild( divBlock ) ;
			contents[0] = divBlock ;
		}

		// Calculate the common parent node of all content blocks.
		var commonParent = groupObj.contents[0].parentNode ;
		for ( var i = 0 ; i < contents.length ; i++ )
			commonParent = FCKDomTools.GetCommonParents( commonParent, contents[i].parentNode ).pop() ;

		// We want to insert things that are in the same tree level only, so calculate the contents again
		// by expanding the selected blocks to the same tree level.
		for ( var i = 0 ; i < contents.length ; i++ )
		{
			var contentNode = contents[i] ;
			while ( contentNode.parentNode )
			{
				if ( contentNode.parentNode == commonParent )
				{
					listContents.push( contentNode ) ;
					break ;
				}
				contentNode = contentNode.parentNode ;
			}
		}

		if ( listContents.length < 1 )
			return ;

		// Insert the list to the DOM tree.
		var insertAnchor = listContents[listContents.length - 1].nextSibling ;
		var listNode = doc.createElement( 'dl' ) ;
		listsCreated.push( listNode ) ;
		while ( listContents.length )
		{
			var contentBlock = listContents.shift() ;
			var docFrag = doc.createDocumentFragment() ;
			while ( contentBlock.firstChild )
				docFrag.appendChild( contentBlock.removeChild( contentBlock.firstChild ) ) ;
			contentBlock.parentNode.removeChild( contentBlock ) ;
			var listItem = doc.createElement( this.TagName == 'dl' ? 'dt' : this.TagName ) ;
			listItem.appendChild( docFrag ) ;
			listNode.appendChild( listItem ) ;
		}
		commonParent.insertBefore( listNode, insertAnchor ) ;
	},

	_RemoveList : function( groupObj, markerObj )
	{
		// This is very much like the change list type operation.
		// Except that we're changing the selected items' indent to -1 in the list array.
		var listArray = this.ListToArray( groupObj.root, markerObj ) ;
		var selectedListItems = [] ;
		
		if ( this.TagName == 'dl' )
		{
			for ( var i = 0 ; i < listArray.length ; i++ )
			{
				listArray[i].indent = -1 ;
			}
		}
		else
		{
			for ( var i = 0 ; i < groupObj.contents.length ; i++ )
			{
				var itemNode = groupObj.contents[i] ;
				itemNode = FCKTools.GetElementAscensor( itemNode, this.TagName ) ;
				if ( ! itemNode || itemNode._FCK_ListItem_Processed )
					continue ;
				selectedListItems.push( itemNode ) ;
				FCKDomTools.SetElementMarker( markerObj, itemNode, '_FCK_ListItem_Processed', true ) ;
			}

			var lastListIndex = null ;
			for ( var i = 0 ; i < selectedListItems.length ; i++ )
			{
				var listIndex = selectedListItems[i]._FCK_ListArray_Index ;
				listArray[listIndex].indent = -1 ;
				lastListIndex = listIndex ;
			}
	
			// After cutting parts of the list out with indent=-1, we still have to maintain the array list
			// model's nextItem.indent <= currentItem.indent + 1 invariant. Otherwise the array model of the
			// list cannot be converted back to a real DOM list.
			for ( var i = lastListIndex + 1; i < listArray.length ; i++ )
			{
				if ( listArray[i].indent > listArray[i-1].indent + 1 )
				{
					var indentOffset = listArray[i-1].indent + 1 - listArray[i].indent ;
					var oldIndent = listArray[i].indent ;
					while ( listArray[i] && listArray[i].indent >= oldIndent)
					{
						listArray[i].indent += indentOffset ;
						i++ ;
					}
					i-- ;
				}
			}
		}

		var newList = this.ArrayToList( listArray, markerObj ) ;
		// If groupObj.root is the last element in its parent, or its nextSibling is a <br>, then we should
		// not add a <br> after the final item. So, check for the cases and trim the <br>.
		if ( groupObj.root.nextSibling == null || groupObj.root.nextSibling.nodeName.IEquals( 'br' ) )
		{
			if ( newList.listNode.lastChild.nodeName.IEquals( 'br' ) )
				newList.listNode.removeChild( newList.listNode.lastChild ) ;
		}
		groupObj.root.parentNode.replaceChild( newList.listNode, groupObj.root ) ;
	},
	
	/**
	 * Convert a DOM list tree into a data structure that is easier to
	 * manipulate. This operation should be non-intrusive in the sense that it
	 * does not change the DOM tree, with the exception that it may add some
	 * markers to the list item nodes when markerObj is specified.
	 */
	ListToArray : function( listNode, markerObj, baseArray, baseIndentLevel, grandparentNode )
	{
		if ( ! listNode.nodeName.IEquals( ['dl'] ) )
			return [] ;

		if ( ! baseIndentLevel )
			baseIndentLevel = 0 ;
		if ( ! baseArray )
			baseArray = [] ;
		// Iterate over all list items to get their contents and look for inner lists.
		for ( var i = 0 ; i < listNode.childNodes.length ; i++ )
		{
			var listItem = listNode.childNodes[i] ;
			if ( ! listItem.nodeName.IEquals( ['dt', 'dd'] ) )
				continue ;
			var itemObj = { 'parent' : listNode, 'indent' : baseIndentLevel, 'contents' : [], 'dx' : listItem.nodeName.toLowerCase() } ;
			if ( ! grandparentNode )
			{
				itemObj.grandparent = listNode.parentNode ;
				if ( itemObj.grandparent && itemObj.grandparent.nodeName.IEquals( ['dt', 'dd'] ) )
					itemObj.grandparent = itemObj.grandparent.parentNode ;
			}
			else
				itemObj.grandparent = grandparentNode ;
			if ( markerObj )
				FCKDomTools.SetElementMarker( markerObj, listItem, '_FCK_ListArray_Index', baseArray.length ) ;
			baseArray.push( itemObj ) ;
			for ( var j = 0 ; j < listItem.childNodes.length ; j++ )
			{
				var child = listItem.childNodes[j] ;
				if ( child.nodeName.IEquals( 'dl' ) )
					// Note the recursion here, it pushes inner list items with
					// +1 indentation in the correct order.
					this.ListToArray( child, markerObj, baseArray, baseIndentLevel + 1, itemObj.grandparent ) ;
				else
					itemObj.contents.push( child ) ;
			}
		}
		return baseArray ;
	},

	// Convert our internal representation of a list back to a DOM forest.
	ArrayToList : function( listArray, markerObj, baseIndex, itemType )
	{
		if ( baseIndex == undefined )
			baseIndex = 0 ;
		if ( ! listArray || listArray.length < baseIndex + 1 )
			return null ;
		var doc = FCKTools.GetElementDocument( listArray[baseIndex].parent ) ;
		var retval = doc.createDocumentFragment() ;
		var rootNode = null ;
		var currentIndex = baseIndex ;
		var indentLevel = Math.max( listArray[baseIndex].indent, 0 ) ;
		var currentListItem = null ;
		while ( true )
		{
			var item = listArray[currentIndex] ;
			if ( item.indent == indentLevel )
			{
				if ( ! rootNode || listArray[currentIndex].parent.nodeName != rootNode.nodeName )
				{
					rootNode = listArray[currentIndex].parent.cloneNode( false ) ;
					retval.appendChild( rootNode ) ;
				}
				currentListItem = doc.createElement( item.dx ) ;
				rootNode.appendChild( currentListItem ) ;
				for ( var i = 0 ; i < item.contents.length ; i++ )
					currentListItem.appendChild( item.contents[i].cloneNode( true ) ) ;
				currentIndex++ ;
			}
			else if ( item.indent == Math.max( indentLevel, 0 ) + 1 )
			{
				var listData = this.ArrayToList( listArray, null, currentIndex ) ;
				currentListItem.appendChild( listData.listNode ) ;
				currentIndex = listData.nextIndex ;
			}
			else if ( item.indent == -1 && baseIndex == 0 && item.grandparent )
			{
				var currentListItem ;
				if ( item.grandparent.nodeName.IEquals( 'dl' ) )
					currentListItem = doc.createElement( item.dx ) ;
				else
				{
					if ( FCKConfig.EnterMode.IEquals( ['div', 'p'] ) && ! item.grandparent.nodeName.IEquals( 'td' ) )
						currentListItem = doc.createElement( FCKConfig.EnterMode ) ;
					else
						currentListItem = doc.createDocumentFragment() ;
				}
				for ( var i = 0 ; i < item.contents.length ; i++ )
					currentListItem.appendChild( item.contents[i].cloneNode( true ) ) ;
				if ( currentListItem.nodeType == 11 )
				{
					if ( currentListItem.lastChild &&
							currentListItem.lastChild.getAttribute &&
							currentListItem.lastChild.getAttribute( 'type' ) == '_moz' )
						currentListItem.removeChild( currentListItem.lastChild );
					currentListItem.appendChild( doc.createElement( 'br' ) ) ;
				}
				if ( currentListItem.nodeName.IEquals( FCKConfig.EnterMode ) && currentListItem.firstChild )
				{
					FCKDomTools.TrimNode( currentListItem ) ;
					if ( FCKListsLib.BlockBoundaries[currentListItem.firstChild.nodeName.toLowerCase()] )
					{
						var tmp = doc.createDocumentFragment() ;
						while ( currentListItem.firstChild )
							tmp.appendChild( currentListItem.removeChild( currentListItem.firstChild ) ) ;
						currentListItem = tmp ;
					}
				}
				if ( FCKBrowserInfo.IsGeckoLike && currentListItem.nodeName.IEquals( ['div', 'p'] ) )
					FCKTools.AppendBogusBr( currentListItem ) ;
				retval.appendChild( currentListItem ) ;
				rootNode = null ;
				currentIndex++ ;
			}
			else
				return null ;

			if ( listArray.length <= currentIndex || Math.max( listArray[currentIndex].indent, 0 ) < indentLevel )
			{
				break ;
			}
		}

		// Clear marker attributes for the new list tree made of cloned nodes, if any.
		if ( markerObj )
		{
			var currentNode = retval.firstChild ;
			while ( currentNode )
			{
				if ( currentNode.nodeType == 1 )
					FCKDomTools.ClearElementMarkers( markerObj, currentNode ) ;
				currentNode = FCKDomTools.GetNextSourceNode( currentNode ) ;
			}
		}

		return { 'listNode' : retval, 'nextIndex' : currentIndex } ;
	}
}

// Add dl/dt/dd elements to FCKListLib to correct processing of the enter key
FCKListsLib.StyleObjectElements.dl = 1 ;

FCKListsLib.NonEmptyBlockElements.dl = 1 ;
FCKListsLib.NonEmptyBlockElements.dt = 1 ;
FCKListsLib.NonEmptyBlockElements.dd = 1 ;

FCKListsLib.PathBlockElements.dd = 1 ;

FCKListsLib.BlockBoundaries.dl = 1 ;
FCKListsLib.BlockBoundaries.dt = 1 ;
FCKListsLib.BlockBoundaries.dd = 1 ;

FCKListsLib.ListBoundaries.dl = 1 ;
FCKListsLib.ListBoundaries.dt = 1 ;
FCKListsLib.ListBoundaries.dd = 1 ;

// Register the related commands.
FCKCommands.RegisterCommand( 'DefinitionList', new FCKDefinitionListCommand( 'definitionlist', 'dl' ) );
FCKCommands.RegisterCommand( 'DefinitionTerm', new FCKDefinitionListCommand( 'definitionterm', 'dt' ) );
FCKCommands.RegisterCommand( 'DefinitionDescription', new FCKDefinitionListCommand( 'definitiondescription', 'dd' ) );

// Create toolbar buttons.
var oDLItem = new FCKToolbarButton( 'DefinitionList', FCKLang.DefinitionList, null, null, false, true, FCKConfig.PluginsPath + 'definitionlist/images/dl.gif' ) ;
var oDTItem = new FCKToolbarButton( 'DefinitionTerm', FCKLang.DefinitionTerm, null, null, false, true, FCKConfig.PluginsPath + 'definitionlist/images/dt.gif' ) ;
var oDDItem = new FCKToolbarButton( 'DefinitionDescription', FCKLang.DefinitionDescription, null, null, false, true, FCKConfig.PluginsPath + 'definitionlist/images/dd.gif' ) ;

// Register the buttons.
FCKToolbarItems.RegisterItem( 'DefinitionList', oDLItem ) ;
FCKToolbarItems.RegisterItem( 'DefinitionTerm', oDTItem ) ;
FCKToolbarItems.RegisterItem( 'DefinitionDescription', oDDItem ) ;
