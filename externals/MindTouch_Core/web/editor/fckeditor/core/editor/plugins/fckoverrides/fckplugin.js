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

FCKCoreStyleCommand.prototype.GetState = function()
{
	if ( MindTouchDeki.GetState() == FCK_TRISTATE_DISABLED )
		return FCK_TRISTATE_DISABLED ;

	return this.IsActive ? FCK_TRISTATE_ON : FCK_TRISTATE_OFF ;
};

FCKDialogCommand.prototype.GetState = function()
{
	if ( this.GetStateFunction )
		return this.GetStateFunction( this.GetStateParam ) ;
	else if ( this.Name == "Find" || this.Name == "Replace" ) // Dirty hack for find and replace buttons
		return FCK.EditMode == FCK_EDITMODE_WYSIWYG ? FCK_TRISTATE_OFF : FCK_TRISTATE_DISABLED ;
	else
		return MindTouchDeki.GetState() ;
};

// ### FormatBlock
FCKFormatBlockCommand.prototype.GetState = function()
{
	// Disabled if not WYSIWYG.
	if ( FCK.EditMode != FCK_EDITMODE_WYSIWYG || ! FCK.EditorWindow )
		return FCK_TRISTATE_DISABLED ;

	// Disabled if cursor is in the title.
	var path = new FCKElementPath( FCKSelection.GetBoundaryParentElement( true ) ) ;
	var firstBlock = path.Block || path.BlockLimit ;

	if( firstBlock )
	{
		if ( firstBlock.nodeName.toLowerCase() == 'h1' )
		{
			var ePrev = FCKDomTools.GetPreviousSourceElement( firstBlock, true, ['body'] ) ;
			if ( ! ePrev )
			{
				return FCK_TRISTATE_DISABLED ;
			}
		}

		for ( var i = 0 ; i < path.Elements.length ; i++ )
		{
			if ( path.Elements[i].nodeName.IEquals(['table', 'tr', 'td', 'th', 'tbody', 'thead', 'tfoot', 'caption']) )
			{
				return FCK_TRISTATE_DISABLED ;
			}
		}
	}

	return FCK_TRISTATE_OFF ;
}

// ### FontName
FCKFontNameCommand.prototype.GetState = MindTouchDeki.GetState;

// ### FontSize
FCKFontSizeCommand.prototype.GetState = MindTouchDeki.GetState;

FCKStyleCommand.prototype.GetState = function()
{
	if ( FCK.EditMode != FCK_EDITMODE_WYSIWYG || !FCK.EditorDocument || MindTouchDeki.GetState() == FCK_TRISTATE_DISABLED )
		return FCK_TRISTATE_DISABLED ;

	if ( FCKSelection.GetType() == 'Control' )
	{
		var el = FCKSelection.GetSelectedElement() ;
		if ( !el || !FCKStyles.CheckHasObjectStyle( el.nodeName.toLowerCase() ) )
			return FCK_TRISTATE_DISABLED ;
	}

	return FCK_TRISTATE_OFF ;
}

// Incorrect behaviour on disabling color buttons
FCKTextColorCommand.prototype.GetState = MindTouchDeki.GetState;

FCKIndentCommand.prototype.GetState = function()
{
	if ( MindTouchDeki.GetState() == FCK_TRISTATE_DISABLED )
		return FCK_TRISTATE_DISABLED ;

	// Initialize parameters if not already initialzed.
	if ( FCKIndentCommand._UseIndentClasses == undefined )
		FCKIndentCommand._InitIndentModeParameters() ;

	// If we're not in a list, and the starting block's indentation is zero, and the current
	// command is the outdent command, then we should return FCK_TRISTATE_DISABLED.
	var startContainer = FCKSelection.GetBoundaryParentElement( true ) ;
	var endContainer = FCKSelection.GetBoundaryParentElement( false ) ;
	var listNode = FCKDomTools.GetCommonParentNode( startContainer, endContainer, ['ul','ol'] ) ;

	if ( listNode )
	{
		if ( this.Name.IEquals( 'outdent' ) )
			return FCK_TRISTATE_OFF ;
		var firstItem = FCKTools.GetElementAscensor( startContainer, 'li' ) ;
		if ( !firstItem || !firstItem.previousSibling )
			return FCK_TRISTATE_DISABLED ;
		return FCK_TRISTATE_OFF ;
	}
	if ( ! FCKIndentCommand._UseIndentClasses && this.Name.IEquals( 'indent' ) )
		return FCK_TRISTATE_OFF;

	var path = new FCKElementPath( startContainer ) ;
	var firstBlock = path.Block || path.BlockLimit ;
	if ( !firstBlock )
		return FCK_TRISTATE_DISABLED ;

	if ( FCKIndentCommand._UseIndentClasses )
	{
		var indentClass = firstBlock.className.match( FCKIndentCommand._ClassNameRegex ) ;
		var indentStep = 0 ;
		if ( indentClass != null )
		{
			indentClass = indentClass[1] ;
			indentStep = FCKIndentCommand._IndentClassMap[indentClass] ;
		}
		if ( ( this.Name == 'outdent' && indentStep == 0 ) ||
				( this.Name == 'indent' && indentStep == FCKConfig.IndentClasses.length ) )
			return FCK_TRISTATE_DISABLED ;
		return FCK_TRISTATE_OFF ;
	}
	else
	{
		var indent = parseInt( firstBlock.style[this.IndentCSSProperty], 10 ) ;
		if ( isNaN( indent ) )
			indent = 0 ;
		if ( indent <= 0 )
			return FCK_TRISTATE_DISABLED ;
		return FCK_TRISTATE_OFF ;
	}
};

FCKJustifyCommand.prototype.GetState = function()
{
	if ( MindTouchDeki.GetState() == FCK_TRISTATE_DISABLED )
		return FCK_TRISTATE_DISABLED ;

	// Retrieve the first selected block.
	var path = new FCKElementPath( FCKSelection.GetBoundaryParentElement( true ) ) ;
	var firstBlock = path.Block || path.BlockLimit ;

	if ( !firstBlock || firstBlock.nodeName.toLowerCase() == 'body' )
		return FCK_TRISTATE_OFF ;

	// Check if the desired style is already applied to the block.
	var currentAlign ;
	if ( FCKBrowserInfo.IsIE )
		currentAlign = firstBlock.currentStyle.textAlign ;
	else
		currentAlign = FCK.EditorWindow.getComputedStyle( firstBlock, '' ).getPropertyValue( 'text-align' );
	currentAlign = currentAlign.replace( /(-moz-|-webkit-|start|auto)/i, '' );
	if ( ( !currentAlign && this.IsDefaultAlign ) || currentAlign == this.AlignValue )
		return FCK_TRISTATE_ON ;
	return FCK_TRISTATE_OFF ;
};

FCKListCommand.prototype.GetState = function()
{
	if ( MindTouchDeki.GetState() == FCK_TRISTATE_DISABLED )
		return FCK_TRISTATE_DISABLED ;

	// We'll use the style system's convention to determine list state here...
	// If the starting block is a descendant of an <ol> or <ul> node, then we're in a list.
	var startContainer = FCKSelection.GetBoundaryParentElement( true ) ;
	var listNode = startContainer ;
	while ( listNode )
	{
		if ( listNode.nodeName.IEquals( [ 'ul', 'ol' ] ) )
			break ;
		listNode = listNode.parentNode ;
	}
	if ( listNode && listNode.nodeName.IEquals( this.TagName ) )
		return FCK_TRISTATE_ON ;
	else
		return FCK_TRISTATE_OFF ;
};

FCKBlockQuoteCommand.prototype.GetState = function()
{
	if ( MindTouchDeki.GetState() == FCK_TRISTATE_DISABLED )
		return FCK_TRISTATE_DISABLED ;

	// Disabled if not WYSIWYG.
	if ( FCK.EditMode != FCK_EDITMODE_WYSIWYG || ! FCK.EditorWindow )
		return FCK_TRISTATE_DISABLED ;

	var path = new FCKElementPath( FCKSelection.GetBoundaryParentElement( true ) ) ;
	var firstBlock = path.Block || path.BlockLimit ;

	if ( !firstBlock || firstBlock.nodeName.toLowerCase() == 'body' )
		return FCK_TRISTATE_OFF ;

	// See if the first block has a blockquote parent.
	for ( var i = 0 ; i < path.Elements.length ; i++ )
	{
		if ( path.Elements[i].nodeName.IEquals( 'blockquote' ) )
			return FCK_TRISTATE_ON ;
	}
	return FCK_TRISTATE_OFF ;
}


/**
 * Toolbar
 */

// allow to disable panel button into the headers
FCKToolbarSet.prototype.RefreshItemsState = function( editorInstance )
{

	// >MT: use all toolbar items to refresh - not only context sensitive
	var aItems = ( editorInstance ? editorInstance.ToolbarSet : this ).Items ;
	// <MT

	for ( var i = 0 ; i < aItems.length ; i++ )
		aItems[i].RefreshState() ;
}

/**
 * FCKXhtml: added encodeURI for the links and images
 */

//FCKXHtml.TagProcessors['a'] = function( node, htmlNode )
//{
//	// Firefox may create empty tags when deleting the selection in some special cases (SF-BUG 1556878).
//	if ( htmlNode.innerHTML.Trim().length == 0 && !htmlNode.name )
//		return false ;
//
//	var sSavedUrl = htmlNode.getAttribute( '_fcksavedurl' ) ;
//	if ( sSavedUrl != null )
//	{
//		// >MT
//		sSavedUrl = MindTouchDeki.EncodeURI( sSavedUrl ) ;
//		// <MT
//		FCKXHtml._AppendAttribute( node, 'href', sSavedUrl ) ;
//	}
//
//
//	// Anchors with content has been marked with an additional class, now we must remove it.
//	if ( FCKBrowserInfo.IsIE )
//	{
//		// Buggy IE, doesn't copy the name of changed anchors.
//		if ( htmlNode.name )
//			FCKXHtml._AppendAttribute( node, 'name', htmlNode.name ) ;
//	}
//
//	node = FCKXHtml._AppendChildNodes( node, htmlNode, false ) ;
//
//	return node ;
//}
//
//FCKXHtml.TagProcessors['img'] = function( node, htmlNode )
//{
//	// The "ALT" attribute is required in XHTML.
//	if ( ! node.attributes.getNamedItem( 'alt' ) )
//		FCKXHtml._AppendAttribute( node, 'alt', '' ) ;
//
//	var sSavedUrl = htmlNode.getAttribute( '_fcksavedurl' ) ;
//	if ( sSavedUrl != null )
//	{
//		// >MT
//		sSavedUrl = MindTouchDeki.EncodeURI( sSavedUrl ) ;
//		// <MT
//		FCKXHtml._AppendAttribute( node, 'src', sSavedUrl ) ;
//	}
//
//	return node ;
//}

/**
 * Apply backgound color on a cell
 * See #0004187
 */

FCKTextColorCommand.prototype.SetColor = function( color )
{
	FCKUndo.SaveUndoStep() ;

	var style = FCKStyles.GetStyle( '_FCK_' +
		( this.Type == 'ForeColor' ? 'Color' : 'BackColor' ) ) ;

	// >MT
	var selectedCells = [] ;

	if ( this.Type == 'BackColor' )
	{
		var oRange = new FCKDomRange( FCK.EditorWindow ) ;
		oRange.MoveToSelection() ;
		
		selectedCells = FCKTableHandler.GetSelectedCells() ;
		
		if ( selectedCells.length == 1 && !oRange.CheckIsCollapsed() )
			selectedCells = [] ;
	}

	if ( selectedCells.length > 0 )
	{
		for ( var i = 0 ; i < selectedCells.length ; i++ )
		{
			var eTd = selectedCells[ i ] ;

			color = color || '' ;
			eTd.style.backgroundColor = color ;
			
			if (eTd.style.cssText.length > 0)
			{
				eTd.setAttribute('_fckstyle', eTd.style.cssText);
			}
			else
			{
				eTd.removeAttribute( 'style' ) ;
				eTd.removeAttribute( '_fckstyle' ) ;
			}
		}
	} // <MT
	else if ( !color || color.length == 0 )
		FCK.Styles.RemoveStyle( style ) ;
	else
	{
		style.SetVariable( 'Color', color ) ;
		FCKStyles.ApplyStyle( style ) ;
	}

	FCKUndo.SaveUndoStep() ;

	FCK.Focus() ;
	FCK.Events.FireEvent( 'OnSelectionChange' ) ;
}

/**
 * @see #0004188: Select multiple cells and columsn in a table and set "center" align
 */
FCKJustifyCommand.prototype.Execute = function()
{
		// Save an undo snapshot before doing anything.
		FCKUndo.SaveUndoStep() ;

		var range = new FCKDomRange( FCK.EditorWindow ) ;
		range.MoveToSelection() ;

		var currentState = this.GetState() ;
		if ( currentState == FCK_TRISTATE_DISABLED )
			return ;

		// >MT
		if ( FCKBrowserInfo.IsGecko )
		{
			var aCells = FCKTableHandler.GetSelectedCells() ;

			if ( aCells.length )
			{
				for ( var i = 0 ; i < aCells.length ; i++ )
				{
					this._SetAlign( aCells[i], currentState ) ;
				}

				FCK.Focus() ;
				FCK.Events.FireEvent( 'OnSelectionChange' ) ;

				return ;
			}
		}
		// <MT

		// Store a bookmark of the selection since the paragraph iterator might
		// change the DOM tree and break selections.
		var bookmark = range.CreateBookmark() ;

		// Apply alignment setting for each paragraph.
		var iterator = new FCKDomRangeIterator( range ) ;
		var block ;
		while ( ( block = iterator.GetNextParagraph() ) )
		{
			this._SetAlign( block, currentState ) ;
		}

		// Restore previous selection.
		range.MoveToBookmark( bookmark ) ;
		range.Select() ;

		FCK.Focus() ;
		FCK.Events.FireEvent( 'OnSelectionChange' ) ;
}

FCKJustifyCommand.prototype._SetAlign = function( block, currentState  )
{
	var cssClassName = this._CssClassName ;

	block.removeAttribute( 'align' ) ;

	if ( cssClassName )
	{
		// Remove the any of the alignment classes from the className.
		var className = block.className.replace( FCKJustifyCommand._GetClassNameRegex(), '' ) ;

		// Append the desired class name.
		if ( currentState == FCK_TRISTATE_OFF )
		{
			if ( className.length > 0 )
				className += ' ' ;
			block.className = className + cssClassName ;
		}
		else if ( className.length == 0 )
			FCKDomTools.RemoveAttribute( block, 'class' ) ;
	}
	else
	{
		var style = block.style ;
		if ( currentState == FCK_TRISTATE_OFF )
			style.textAlign = this.AlignValue ;
		else
		{
			style.textAlign = '' ;
			if ( style.cssText.length == 0 )
				block.removeAttribute( 'style' ) ;
		}

		// >MT: style attribute protection
		// @see #0006797
		if ( style.cssText.length == 0 )
			block.removeAttribute( '_fckstyle' ) ;
		else
			block.setAttribute( '_fckstyle', style.cssText ) ;
		// <MT
	}
}

FCKXHtml.GetXHTML = function( node, includeNode, format )
{
	FCKDomTools.CheckAndRemovePaddingNode( FCKTools.GetElementDocument( node ), FCKConfig.EnterMode ) ;
	FCKXHtmlEntities.Initialize() ;

	// >MT: we should process nbsp anyway
	if ( !FCKConfig.ProcessHTMLEntities )
	{
		FCKXHtmlEntities.Entities[' '] = 'nbsp' ;
	}
	// <MT

	// Set the correct entity to use for empty blocks.
	// >MT
	// this._NbspEntity = ( FCKConfig.ProcessHTMLEntities? 'nbsp' : '#160' ) ;
	this._NbspEntity = 'nbsp' ;
	// <MT

	// Save the current IsDirty state. The XHTML processor may change the
	// original HTML, dirtying it.
	var bIsDirty = FCK.IsDirty() ;

	// Special blocks are blocks of content that remain untouched during the
	// process. It is used for SCRIPTs and STYLEs.
	FCKXHtml.SpecialBlocks = new Array() ;

	// Create the XML DOMDocument object.
	this.XML = FCKTools.CreateXmlObject( 'DOMDocument' ) ;

	// Add a root element that holds all child nodes.
	this.MainNode = this.XML.appendChild( this.XML.createElement( 'xhtml' ) ) ;

	FCKXHtml.CurrentJobNum++ ;

//  var dTimer = new Date() ;

	if ( includeNode )
		this._AppendNode( this.MainNode, node ) ;
	else
		this._AppendChildNodes( this.MainNode, node, false ) ;
	
	/**
	 * FCKXHtml._AppendNode() marks DOM element objects it has
	 * processed by adding a property called _fckxhtmljob,
	 * setting it equal to the value of FCKXHtml.CurrentJobNum.
	 * On Internet Explorer, if an element object has such a
	 * property,  it will show up in the object's attributes
	 * NamedNodeMap, and the corresponding Attr object in
	 * that collection  will have is specified property set
	 * to true.  This trips up code elsewhere that checks to
	 * see if an element is free of attributes before proceeding
	 * with an edit operation (c.f. FCK.Style.RemoveFromRange())
	 *
	 * refs #2156 and #2834
	 */
	if ( FCKBrowserInfo.IsIE )
		FCKXHtml._RemoveXHtmlJobProperties( node ) ;

	// Get the resulting XHTML as a string.
	var sXHTML = this._GetMainXmlString() ;

//  alert( 'Time: ' + ( ( ( new Date() ) - dTimer ) ) + ' ms' ) ;

	this.XML = null ;

	// Safari adds xmlns="http://www.w3.org/1999/xhtml" to the root node (#963)
	if ( FCKBrowserInfo.IsSafari )
		sXHTML = sXHTML.replace( /^<xhtml.*?>/, '<xhtml>' ) ;

	// Strip the "XHTML" root node.
	sXHTML = sXHTML.substr( 7, sXHTML.length - 15 ).Trim() ;

	// According to the doctype set the proper end for self-closing tags
	// HTML: <br>
	// XHTML: Add a space, like <br/> -> <br />
	if (FCKConfig.DocType.length > 0 && FCKRegexLib.HtmlDocType.test( FCKConfig.DocType ) )
		sXHTML = sXHTML.replace( FCKRegexLib.SpaceNoClose, '>');
	else
		sXHTML = sXHTML.replace( FCKRegexLib.SpaceNoClose, ' />');

	if ( FCKConfig.ForceSimpleAmpersand )
		sXHTML = sXHTML.replace( FCKRegexLib.ForceSimpleAmpersand, '&' ) ;

	// >MT: replace entities before the code formatting
	sXHTML = sXHTML.replace( FCKRegexLib.GeckoEntitiesMarker, '&' ) ;
	// <MT
	
	if ( format )
		sXHTML = FCKCodeFormatter.Format( sXHTML ) ;

	// Now we put back the SpecialBlocks contents.
	for ( var i = 0 ; i < FCKXHtml.SpecialBlocks.length ; i++ )
	{
		var oRegex = new RegExp( '___FCKsi___' + i ) ;
		sXHTML = sXHTML.replace( oRegex, FCKXHtml.SpecialBlocks[i] ) ;
	}

	// Replace entities marker with the ampersand.
	// >MT: replace entities before the code formatting
	// sXHTML = sXHTML.replace( FCKRegexLib.GeckoEntitiesMarker, '&' ) ;
	// <MT

	// Restore the IsDirty state if it was not dirty.
	if ( !bIsDirty )
		FCK.ResetIsDirty() ;

	FCKDomTools.EnforcePaddingNode( FCKTools.GetElementDocument( node ), FCKConfig.EnterMode ) ;
	return sXHTML ;
}

function FCK_OnBlur( editorInstance )
{
	// >MT: see #0004666
	return;
	// <MT

	var eToolbarSet = editorInstance.ToolbarSet ;

	if ( eToolbarSet.CurrentInstance == editorInstance )
		eToolbarSet.Disable() ;
}

/**
 * Visit Link command
 * added support of MindTouch Deki internal links
 */
FCKVisitLinkCommand.prototype.Execute = function()
{
	var el = FCKSelection.MoveToAncestorNode( 'A' ) ;
	var url = el.getAttribute( '_fcksavedurl' ) || el.getAttribute( 'href', 2 ) ;

	url = MindTouchDeki.GetUrlFromName( url, true ) ;

	if ( false == url )
		return ;

	// Check if it's a full URL.
	// If not full URL, we'll need to apply the BaseHref setting.
	if ( ! /:\/\//.test( url ) )
	{
		var baseHref = FCKConfig.BaseHref ;
		var parentWindow = FCK.GetInstanceObject( 'parent' ) ;
		if ( !baseHref )
		{
			baseHref = parentWindow.document.location.href ;
			baseHref = baseHref.substring( 0, baseHref.lastIndexOf( '/' ) + 1 ) ;
		}

		if ( /^\//.test( url ) )
		{
			try
			{
				baseHref = baseHref.match( /^.*:\/\/+[^\/]+/ )[0] ;
			}
			catch ( e )
			{
				baseHref = parentWindow.document.location.protocol + '://' + parentWindow.parent.document.location.host ;
			}
		}

		url = baseHref + url ;
	}

	if ( !window.open( url, '_blank' ) )
		alert( FCKLang.VisitLinkBlocked ) ;
}

FCKStyles.CheckSelectionChanges = function()
{
	// >MT: check if table is selected in FF
	var startElement ;

	if ( MindTouchDeki.CheckTable() )
	{
		var aCells = FCKTableHandler.GetSelectedCells() ;
		startElement = aCells[0].firstChild ;
	}
	else
	{
		startElement = FCKSelection.GetBoundaryParentElement( true ) ;
	}
	// <MT

	if ( !startElement )
		return ;

	// Walks the start node parents path, checking all styles that are being listened.
	var path = new FCKElementPath( startElement ) ;
	var styles = this.GetStyles() ;

	for ( var styleName in styles )
	{
		var callbacks = this._Callbacks[ styleName ] ;

		if ( callbacks )
		{
			var style = styles[ styleName ] ;
			var state = style.CheckActive( path ) ;

			if ( state != ( style._LastState || null ) )
			{
				style._LastState = state ;

				for ( var i = 0 ; i < callbacks.length ; i++ )
				{
					var callback = callbacks[i][0] ;
					var callbackOwner = callbacks[i][1] ;

					callback.call( callbackOwner || window, styleName, state ) ;
				}
			}
		}
	}
}

/*
 * see #5768: Cloned paragraphs copy too much information for Deki
 */
FCKDomTools.CloneElement = function( element, bCloneChildren )
{
	bCloneChildren = bCloneChildren || false ;
	
	element = element.cloneNode( bCloneChildren ) ;

	// The "id" attribute should never be cloned to avoid duplication.
	element.removeAttribute( 'id', false ) ;
	
	// >MT: remove dekiscript attributes
	element.removeAttribute( 'function', false ) ;
	element.removeAttribute( 'block', false ) ;
	element.removeAttribute( 'init', false ) ;
	element.removeAttribute( 'foreach', false ) ;
	element.removeAttribute( 'if', false ) ;
	element.removeAttribute( 'where', false ) ;
	element.removeAttribute( 'ctor', false ) ;
	// <MT

	return element ;
}

/*
 * see #0005759: Pasting into formatted blocks
 */
if ( !FCKBrowserInfo.IsIE )
{
	FCKSelection.GetParentElement = function()
	{
		if ( this.GetType() == 'Control' )
			return FCKSelection.GetSelectedElement().parentNode ;
		else
		{
			var oSel = this.GetSelection() ;
			if ( oSel )
			{
				// if anchorNode == focusNode, see if the selection is text only or including nodes.
				// if text only, return the parent node.
				// if the selection includes DOM nodes, then the anchorNode is the nearest container.
				if ( oSel.anchorNode && oSel.anchorNode == oSel.focusNode )
				{
					var oRange = oSel.getRangeAt( 0 ) ;
					// >MT: check oSel.anchorNode.nodeName
					if ( ( oRange.collapsed || oRange.startContainer.nodeType == 3 ) &&
						 oSel.anchorNode.nodeName.toLowerCase() != 'pre' )
					{
						return oSel.anchorNode.parentNode ;
					}
					else
						return oSel.anchorNode ;
					// <MT
				}
	
				// looks like we're having a large selection here. To make the behavior same as IE's TextRange.parentElement(),
				// we need to find the nearest ancestor node which encapsulates both the beginning and the end of the selection.
				// TODO: A simpler logic can be found.
				var anchorPath = new FCKElementPath( oSel.anchorNode ) ;
				var focusPath = new FCKElementPath( oSel.focusNode ) ;
				var deepPath = null ;
				var shallowPath = null ;
				if ( anchorPath.Elements.length > focusPath.Elements.length )
				{
					deepPath = anchorPath.Elements ;
					shallowPath = focusPath.Elements ;
				}
				else
				{
					deepPath = focusPath.Elements ;
					shallowPath = anchorPath.Elements ;
				}
	
				var deepPathBase = deepPath.length - shallowPath.length ;
				for( var i = 0 ; i < shallowPath.length ; i++)
				{
					if ( deepPath[deepPathBase + i] == shallowPath[i])
						return shallowPath[i];
				}
				return null ;
			}
		}
		return null ;
	}
}

/*
 * see #0005545: No source display if you're scrolled lower than the source output
 */
FCKSourceCommand.prototype.Execute = function()
{
	if ( FCKConfig.SourcePopup )	// Until v2.2, it was mandatory for FCKBrowserInfo.IsGecko.
	{
		var iWidth	= FCKConfig.ScreenWidth * 0.65 ;
		var iHeight	= FCKConfig.ScreenHeight * 0.65 ;
		FCKDialog.OpenDialog( 'FCKDialog_Source', FCKLang.Source, 'dialog/fck_source.html', iWidth, iHeight, null, true ) ;
	}
	else
	{
		FCK.SwitchEditMode() ;
		// >MT
		MindTouchDeki.ScrollToTop() ;
		// <MT
	}
}

/**
 * Protect style attribute
 * 
 * @see #0005598
 * @see http://dev.fckeditor.net/ticket/2810
 * 
 */
if ( !FCKBrowserInfo.IsIE )
{
	FCKXHtml._AppendAttributes = function( xmlNode, htmlNode, node )
	{
		var aAttributes = htmlNode.attributes ;
	
		for ( var n = 0 ; n < aAttributes.length ; n++ )
		{
			var oAttribute = aAttributes[n] ;
	
			if ( oAttribute.specified )
			{
				var sAttName = oAttribute.nodeName.toLowerCase() ;
				var sAttValue ;
	
				// Ignore any attribute starting with "_fck".
				if ( sAttName.StartsWith( '_fck' ) )
					continue ;
				// There is a bug in Mozilla that returns '_moz_xxx' attributes as specified.
				else if ( sAttName.indexOf( '_moz' ) == 0 )
					continue ;
				// There are one cases (on Gecko) when the oAttribute.nodeValue must be used:
				//		- for the "class" attribute
				else if ( sAttName == 'class' )
				{
					sAttValue = oAttribute.nodeValue.replace( FCKRegexLib.FCK_Class, '' ) ;
					if ( sAttValue.length == 0 )
						continue ;
				}
				// XHTML doens't support attribute minimization like "CHECKED". It must be transformed to checked="checked".
				else if ( oAttribute.nodeValue === true )
					sAttValue = sAttName ;
				else
				{
					// >MT: protect style attribute
					if ( sAttName == 'style' && htmlNode.hasAttribute( '_fckstyle' ) )
					{
						sAttValue = htmlNode.getAttribute( '_fckstyle' ) ;
					}
					else
					{
						sAttValue = htmlNode.getAttribute( sAttName, 2 ) ;	// We must use getAttribute to get it exactly as it is defined.
					}
					// <MT
				}
	
				this._AppendAttribute( node, sAttName, sAttValue ) ;
			}
		}
	}
}
else
{
	FCKXHtml._AppendAttributes = function( xmlNode, htmlNode, node, nodeName )
	{
		var aAttributes = htmlNode.attributes,
			bHasStyle ;
	
		for ( var n = 0 ; n < aAttributes.length ; n++ )
		{
			var oAttribute = aAttributes[n] ;
	
			if ( oAttribute.specified )
			{
				var sAttName = oAttribute.nodeName.toLowerCase() ;
				var sAttValue ;
	
				// Ignore any attribute starting with "_fck".
				if ( sAttName.StartsWith( '_fck' ) )
					continue ;
				// The following must be done because of a bug on IE regarding the style
				// attribute. It returns "null" for the nodeValue.
				else if ( sAttName == 'style' )
				{
					// Just mark it to do it later in this function.
					bHasStyle = true ;
					continue ;
				}
				// There are two cases when the oAttribute.nodeValue must be used:
				//		- for the "class" attribute
				//		- for events attributes (on IE only).
				else if ( sAttName == 'class' )
				{
					sAttValue = oAttribute.nodeValue.replace( FCKRegexLib.FCK_Class, '' ) ;
					if ( sAttValue.length == 0 )
						continue ;
				}
				else if ( sAttName.indexOf('on') == 0 )
					sAttValue = oAttribute.nodeValue ;
				else if ( nodeName == 'body' && sAttName == 'contenteditable' )
					continue ;
				// XHTML doens't support attribute minimization like "CHECKED". It must be transformed to checked="checked".
				else if ( oAttribute.nodeValue === true )
					sAttValue = sAttName ;
				else
				{
					// We must use getAttribute to get it exactly as it is defined.
					// There are some rare cases that IE throws an error here, so we must try/catch.
					try
					{
						sAttValue = htmlNode.getAttribute( sAttName, 2 ) ;
					}
					catch (e) {}
				}
				this._AppendAttribute( node, sAttName, sAttValue || oAttribute.nodeValue ) ;
			}
		}
	
		// IE loses the style attribute in JavaScript-created elements tags. (#2390)
		if ( bHasStyle || htmlNode.style.cssText.length > 0 || htmlNode.getAttribute( '_fckstyle' ) )
		{
			var data = FCKTools.ProtectFormStyles( htmlNode ) ;
			
			// >MT: protect style attribute
			var sStyleValue = htmlNode.getAttribute( '_fckstyle' ) || htmlNode.style.cssText ;
			sStyleValue = sStyleValue.replace( FCKRegexLib.StyleProperties, FCKTools.ToLowerCase ) ;
			// <MT

			FCKTools.RestoreFormStyles( htmlNode, data ) ;
			this._AppendAttribute( node, 'style', sStyleValue ) ;
		}
	}
}

FCKRegexLib.ProtectStyles = /<[^>\s]+?(?=\s)[^>]*?\sstyle=((?:(?:\s*)("|').*?\2)|(?:[^"'][^ >]+))/gi ;

FCK.ProtectStyles = function( html )
{
	html = html.replace( FCKRegexLib.ProtectStyles, '$& _fckstyle=$1' ) ;

	return html ;
}

FCK.CustomDataProcessors = [] ;
FCK.AddCustomDataProcessor = function( func, scope )
{
	scope = scope || this ;
	
	var oDataProcessor =
	{
		fProcessor : func,
		oScope : scope
	} ;
	
	FCK.CustomDataProcessors.push( oDataProcessor ) ;
}

FCK.SetData = function( data, resetIsDirty )
{
	this.EditingArea.Mode = FCK.EditMode ;

	// If there was an onSelectionChange listener in IE we must remove it to avoid crashes #1498
	if ( FCKBrowserInfo.IsIE && FCK.EditorDocument )
	{
		FCK.EditorDocument.detachEvent("onselectionchange", Doc_OnSelectionChange ) ;
	}

	FCKTempBin.Reset() ;

	// Bug #2469: SelectionData.createRange becomes undefined after the editor
	// iframe is changed by FCK.SetData().
	FCK.Selection.Release() ;

	if ( FCK.EditMode == FCK_EDITMODE_WYSIWYG )
	{
		// Save the resetIsDirty for later use (async)
		this._ForceResetIsDirty = ( resetIsDirty === true ) ;

		// Protect parts of the code that must remain untouched (and invisible)
		// during editing.
		data = FCKConfig.ProtectedSource.Protect( data ) ;

		// Call the Data Processor to transform the data.
		data = FCK.DataProcessor.ConvertToHtml( data ) ;

		// Fix for invalid self-closing tags (see #152).
		data = data.replace( FCKRegexLib.InvalidSelfCloseTags, '$1></$2>' ) ;

		// Protect event attributes (they could get fired in the editing area).
		data = FCK.ProtectEvents( data ) ;

		// Protect some things from the browser itself.
		data = FCK.ProtectUrls( data ) ;
		data = FCK.ProtectTags( data ) ;
		// >MT: custom data processors
		for ( var i = 0 ; i < FCK.CustomDataProcessors.length ; i++ )
		{
			oProcessor = FCK.CustomDataProcessors[i] ;
			data = oProcessor.fProcessor.call( oProcessor.oScope, data ) ;
		}
		// <MT

		// Insert the base tag (FCKConfig.BaseHref), if not exists in the source.
		// The base must be the first tag in the HEAD, to get relative
		// links on styles, for example.
		if ( FCK.TempBaseTag.length > 0 && !FCKRegexLib.HasBaseTag.test( data ) )
			data = data.replace( FCKRegexLib.HeadOpener, '$&' + FCK.TempBaseTag ) ;

		// Build the HTML for the additional things we need on <head>.
		var sHeadExtra = '' ;

		if ( !FCKConfig.FullPage )
			sHeadExtra += _FCK_GetEditorAreaStyleTags() ;

		if ( FCKBrowserInfo.IsIE )
			sHeadExtra += FCK._GetBehaviorsStyle() ;
		else if ( FCKConfig.ShowBorders )
			sHeadExtra += FCKTools.GetStyleHtml( FCK_ShowTableBordersCSS, true ) ;

		sHeadExtra += FCKTools.GetStyleHtml( FCK_InternalCSS, true ) ;

		// Attention: do not change it before testing it well (sample07)!
		// This is tricky... if the head ends with <meta ... content type>,
		// Firefox will break. But, it works if we place our extra stuff as
		// the last elements in the HEAD.
		data = data.replace( FCKRegexLib.HeadCloser, sHeadExtra + '$&' ) ;

		// Load the HTML in the editing area.
		this.EditingArea.OnLoad = _FCK_EditingArea_OnLoad ;
		this.EditingArea.Start( data ) ;
	}
	else
	{
		// Remove the references to the following elements, as the editing area
		// IFRAME will be removed.
		FCK.EditorWindow	= null ;
		FCK.EditorDocument	= null ;
		FCKDomTools.PaddingNode = null ;

		this.EditingArea.OnLoad = null ;
		this.EditingArea.Start( data ) ;

		// Enables the context menu in the textarea.
		this.EditingArea.Textarea._FCKShowContextMenu = true ;

		// Removes the enter key handler.
		FCK.EnterKeyHandler = null ;

		if ( resetIsDirty )
			this.ResetIsDirty() ;

		// Listen for keystroke events.
		FCK.KeystrokeHandler.AttachToElement( this.EditingArea.Textarea ) ;

		this.EditingArea.Textarea.focus() ;

		FCK.Events.FireEvent( 'OnAfterSetHTML' ) ;
	}

	if ( window.onresize )
		window.onresize() ;
}

// protect style attribute
FCK.AddCustomDataProcessor( FCK.ProtectStyles ) ;

FCKDomRange.prototype.MoveToElementEditEnd = function( targetElement )
{
	var editableElement ;

	while ( targetElement && targetElement.nodeType == 1 )
	{
		if ( FCKDomTools.CheckIsEditable( targetElement ) )
			editableElement = targetElement ;
		else if ( editableElement )
			break ;		// If we already found an editable element, stop the loop.

		targetElement = targetElement.firstChild ;
	}

	if ( editableElement )
	{
		this.SetStart( editableElement, 2 ) ;
		this.SetEnd( editableElement, 2 ) ;
	}
}

if ( ! FCKBrowserInfo.IsIE )
{
	FCK.InitializeBehaviors = function()
	{
		// When calling "SetData", the editing area IFRAME gets a fixed height. So we must recalculate it.
		if ( window.onresize )		// Not for Safari/Opera.
			window.onresize() ;
	
		FCKFocusManager.AddWindow( this.EditorWindow ) ;
	
		this.ExecOnSelectionChange = function()
		{
			FCK.Events.FireEvent( "OnSelectionChange" ) ;
		}
	
		this._ExecDrop = function( evt )
		{
			if ( FCK.MouseDownFlag )
			{
				FCK.MouseDownFlag = false ;
				return ;
			}
	
			if ( FCKConfig.ForcePasteAsPlainText )
			{
				if ( evt.dataTransfer )
				{
					var text = evt.dataTransfer.getData( 'Text' ) ;
					text = FCKTools.HTMLEncode( text ) ;
					text = FCKTools.ProcessLineBreaks( window, FCKConfig, text ) ;
					FCK.InsertHtml( text ) ;
				}
				else if ( FCKConfig.ShowDropDialog )
					FCK.PasteAsPlainText() ;
	
				evt.preventDefault() ;
				evt.stopPropagation() ;
			}
		}
	
		this._ExecCheckCaret = function( evt )
		{
			if ( FCK.EditMode != FCK_EDITMODE_WYSIWYG )
				return ;
	
			if ( evt.type == 'keypress' )
			{
				var keyCode = evt.keyCode ;
				// ignore if positioning key is not pressed.
				// left or up arrow keys need to be processed as well, since <a> links can be expanded in Gecko's editor
				// when the caret moved left or up from another block element below.
				if ( keyCode < 33 || keyCode > 40 )
					return ;
			}
	
			var blockEmptyStop = function( node )
			{
				if ( node.nodeType != 1 )
					return false ;
				var tag = node.tagName.toLowerCase() ;
				return ( FCKListsLib.BlockElements[tag] || FCKListsLib.EmptyElements[tag] ) ;
			}
	
			var moveCursor = function()
			{
				var selection = FCKSelection.GetSelection() ;
				var range = selection.getRangeAt(0) ;
				if ( ! range || ! range.collapsed )
					return ;
	
				var node = range.endContainer ;
	
				// only perform the patched behavior if we're at the end of a text node.
				if ( node.nodeType != 3 )
					return ;
	
				if ( node.nodeValue.length != range.endOffset )
					return ;
					
				// >MT: fix for bug #0006241
				var lineBreakPos = node.nodeValue.lastIndexOf( '\n' ) ;				
				if ( node.nodeValue.length > 0 && lineBreakPos == (node.nodeValue.length - 1) )
				{
					range = FCK.EditorDocument.createRange() ;
					range.setStart( node, lineBreakPos ) ;
					range.setEnd( node, lineBreakPos ) ;
					
					selection.removeAllRanges() ;
					selection.addRange( range ) ;					
					FCK.Events.FireEvent( "OnSelectionChange" ) ;
					return ;
				}
				// <MT
	
				// only perform the patched behavior if we're in an <a> tag, or the End key is pressed.
				var parentTag = node.parentNode.tagName.toLowerCase() ;
				if ( ! (  parentTag == 'a' || ( !FCKBrowserInfo.IsOpera && String(node.parentNode.contentEditable) == 'false' ) ||
						( ! ( FCKListsLib.BlockElements[parentTag] || FCKListsLib.NonEmptyBlockElements[parentTag] )
						  && keyCode == 35 ) ) )
					return ;
	
				// our caret has moved to just after the last character of a text node under an unknown tag, how to proceed?
				// first, see if there are other text nodes by DFS walking from this text node.
				// 	- if the DFS has scanned all nodes under my parent, then go the next step.
				//	- if there is a text node after me but still under my parent, then do nothing and return.
				var nextTextNode = FCKTools.GetNextTextNode( node, node.parentNode, blockEmptyStop ) ;
				if ( nextTextNode )
					return ;
	
				// we're pretty sure we need to move the caret forcefully from here.
				range = FCK.EditorDocument.createRange() ;
	
				nextTextNode = FCKTools.GetNextTextNode( node, node.parentNode.parentNode, blockEmptyStop ) ;
				if ( nextTextNode )
				{
					// Opera thinks the dummy empty text node we append beyond the end of <a> nodes occupies a caret
					// position. So if the user presses the left key and we reset the caret position here, the user
					// wouldn't be able to go back.
					if ( FCKBrowserInfo.IsOpera && keyCode == 37 )
						return ;
	
					// now we want to get out of our current parent node, adopt the next parent, and move the caret to
					// the appropriate text node under our new parent.
					// our new parent might be our current parent's siblings if we are lucky.
					range.setStart( nextTextNode, 0 ) ;
					range.setEnd( nextTextNode, 0 ) ;
				}
				else
				{
					// no suitable next siblings under our grandparent! what to do next?
					while ( node.parentNode
						&& node.parentNode != FCK.EditorDocument.body
						&& node.parentNode != FCK.EditorDocument.documentElement
						&& node == node.parentNode.lastChild
						&& ( ! FCKListsLib.BlockElements[node.parentNode.tagName.toLowerCase()]
						  && ! FCKListsLib.NonEmptyBlockElements[node.parentNode.tagName.toLowerCase()] ) )
						node = node.parentNode ;
	
	
					if ( FCKListsLib.BlockElements[ parentTag ]
							|| FCKListsLib.EmptyElements[ parentTag ]
							|| node == FCK.EditorDocument.body )
					{
						// if our parent is a block node, move to the end of our parent.
						range.setStart( node, node.childNodes.length ) ;
						range.setEnd( node, node.childNodes.length ) ;
					}
					else
					{
						// things are a little bit more interesting if our parent is not a block node
						// due to the weired ways how Gecko's caret acts...
						var stopNode = node.nextSibling ;
	
						// find out the next block/empty element at our grandparent, we'll
						// move the caret just before it.
						while ( stopNode )
						{
							if ( stopNode.nodeType != 1 )
							{
								stopNode = stopNode.nextSibling ;
								continue ;
							}
	
							var stopTag = stopNode.tagName.toLowerCase() ;
							if ( FCKListsLib.BlockElements[stopTag] || FCKListsLib.EmptyElements[stopTag]
								|| FCKListsLib.NonEmptyBlockElements[stopTag] )
								break ;
							stopNode = stopNode.nextSibling ;
						}
	
						// note that the dummy marker below is NEEDED, otherwise the caret's behavior will
						// be broken in Gecko.
						var marker = FCK.EditorDocument.createTextNode( '' ) ;
						if ( stopNode )
							node.parentNode.insertBefore( marker, stopNode ) ;
						else
							node.parentNode.appendChild( marker ) ;
						range.setStart( marker, 0 ) ;
						range.setEnd( marker, 0 ) ;
					}
				}
	
				selection.removeAllRanges() ;
				selection.addRange( range ) ;
				FCK.Events.FireEvent( "OnSelectionChange" ) ;
			}
	
			setTimeout( moveCursor, 1 ) ;
		}
	
		this.ExecOnSelectionChangeTimer = function()
		{
			if ( FCK.LastOnChangeTimer )
				window.clearTimeout( FCK.LastOnChangeTimer ) ;
	
			FCK.LastOnChangeTimer = window.setTimeout( FCK.ExecOnSelectionChange, 100 ) ;
		}
	
		this.EditorDocument.addEventListener( 'mouseup', this.ExecOnSelectionChange, false ) ;
	
		// On Gecko, firing the "OnSelectionChange" event on every key press started to be too much
		// slow. So, a timer has been implemented to solve performance issues when typing to quickly.
		this.EditorDocument.addEventListener( 'keyup', this.ExecOnSelectionChangeTimer, false ) ;
	
		this._DblClickListener = function( e )
		{
			FCK.OnDoubleClick( e.target ) ;
			e.stopPropagation() ;
		}
		this.EditorDocument.addEventListener( 'dblclick', this._DblClickListener, true ) ;
	
		// Record changes for the undo system when there are key down events.
		this.EditorDocument.addEventListener( 'keydown', this._KeyDownListener, false ) ;
	
		// Hooks for data object drops
		if ( FCKBrowserInfo.IsGecko )
		{
			this.EditorWindow.addEventListener( 'dragdrop', this._ExecDrop, true ) ;
		}
		else if ( FCKBrowserInfo.IsSafari )
		{
			this.EditorDocument.addEventListener( 'dragover', function ( evt )
					{if ( !FCK.MouseDownFlag && FCK.Config.ForcePasteAsPlainText ) evt.returnValue = false ;}, true ) ;
			this.EditorDocument.addEventListener( 'drop', this._ExecDrop, true ) ;
			this.EditorDocument.addEventListener( 'mousedown',
				function( ev )
				{
					var element = ev.srcElement ;
	
					if ( element.nodeName.IEquals( 'IMG', 'HR', 'INPUT', 'TEXTAREA', 'SELECT' ) )
					{
						FCKSelection.SelectNode( element ) ;
					}
				}, true ) ;
	
			this.EditorDocument.addEventListener( 'mouseup',
				function( ev )
				{
					if ( ev.srcElement.nodeName.IEquals( 'INPUT', 'TEXTAREA', 'SELECT' ) )
						ev.preventDefault()
				}, true ) ;
	
			this.EditorDocument.addEventListener( 'click',
				function( ev )
				{
					if ( ev.srcElement.nodeName.IEquals( 'INPUT', 'TEXTAREA', 'SELECT' ) )
						ev.preventDefault()
				}, true ) ;
		}
	
		// Kludge for buggy Gecko caret positioning logic (Bug #393 and #1056)
		if ( FCKBrowserInfo.IsGecko || FCKBrowserInfo.IsOpera )
		{
			this.EditorDocument.addEventListener( 'keypress', this._ExecCheckCaret, false ) ;
			this.EditorDocument.addEventListener( 'click', this._ExecCheckCaret, false ) ;
		}
	
		// Reset the context menu.
		FCK.ContextMenu._InnerContextMenu.SetMouseClickWindow( FCK.EditorWindow ) ;
		FCK.ContextMenu._InnerContextMenu.AttachToElement( FCK.EditorDocument ) ;
	}
}

/**
 * @see #0006389: changing bullets to numbers in list doesn't work as expected
 */
FCKListCommand.prototype._ChangeListType = function( groupObj, markerObj, listsCreated )
{
	// >MT
	if ( groupObj.contents.length == 1 )
	{
		var itemNode = groupObj.contents[0] ;
		if ( itemNode.parentNode.nodeName.IEquals( ['ul', 'ol'] ) )
		{
			var newList = MindTouchDeki.RenameNode( itemNode.parentNode, this.TagName ) ;
			newList._FCK_ListGroupObject = null ;
			return ;
		}
	}
	// <MT
	
	// This case is easy...
	// 1. Convert the whole list into a one-dimensional array.
	// 2. Change the list type by modifying the array.
	// 3. Recreate the whole list by converting the array to a list.
	// 4. Replace the original list with the recreated list.
	var listArray = FCKDomTools.ListToArray( groupObj.root, markerObj ) ;
	var selectedListItems = [] ;
	for ( var i = 0 ; i < groupObj.contents.length ; i++ )
	{
		var itemNode = groupObj.contents[i] ;
		itemNode = FCKTools.GetElementAscensor( itemNode, 'li' ) ;
		if ( ! itemNode || itemNode._FCK_ListItem_Processed )
			continue ;
		selectedListItems.push( itemNode ) ;
		FCKDomTools.SetElementMarker( markerObj, itemNode, '_FCK_ListItem_Processed', true ) ;
	}
	var fakeParent = FCKTools.GetElementDocument( groupObj.root ).createElement( this.TagName ) ;
	for ( var i = 0 ; i < selectedListItems.length ; i++ )
	{
		var listIndex = selectedListItems[i]._FCK_ListArray_Index ;
		listArray[listIndex].parent = fakeParent ;
	}
	var newList = FCKDomTools.ArrayToList( listArray, markerObj ) ;
	for ( var i = 0 ; i < newList.listNode.childNodes.length ; i++ )
	{
		if ( newList.listNode.childNodes[i].nodeName.IEquals( this.TagName ) )
			listsCreated.push( newList.listNode.childNodes[i] ) ;
	}
	groupObj.root.parentNode.replaceChild( newList.listNode, groupObj.root ) ;
}

/**
 * @see #0006647: Attempting to format certain text blocks causes FireFox to crash
 */
FCKRegexLib.HtmlTag = /<\/?\w+(?:(?:\s+(?:\w|\w[\w-]*\w)(?:\s*=\s*(?:\".*?\"|'.*?'|[^'\">\s]+))?)+\s*|\s*)\/?>/ ;


/**
 * @see #0006613
 */
FCK_POSITION_IDENTICAL = 0 ;
FCK_POSITION_DISCONNECTED = 1 ;
FCK_POSITION_PRECEDING = 2 ;
FCK_POSITION_FOLLOWING = 4 ;
FCK_POSITION_CONTAINS = 8 ;
FCK_POSITION_CONTAINED_BY = 16 ;

//Compare Position - MIT Licensed, John Resig
FCKDomTools.ComparePosition = function ( a, b )
{
	return a.compareDocumentPosition ?
		a.compareDocumentPosition( b ) :
		a.contains ?
			( a != b && a.contains( b ) && FCK_POSITION_CONTAINED_BY ) +
			( a != b && b.contains(a) && FCK_POSITION_CONTAINS ) +
			( a.sourceIndex >= 0 && b.sourceIndex >= 0 ?
				( a.sourceIndex < b.sourceIndex && FCK_POSITION_FOLLOWING ) +
				( a.sourceIndex > b.sourceIndex && FCK_POSITION_PRECEDING ) :
					FCK_POSITION_DISCONNECTED ) +
			FCK_POSITION_IDENTICAL :
			FCK_POSITION_IDENTICAL ;
}

FCKListsLib.RemoveEmpty = {abbr:1,acronym:1,address:1,b:1,bdo:1,big:1,cite:1,code:1,del:1,dfn:1,em:1,font:1,i:1,ins:1,label:1,kbd:1,q:1,s:1,samp:1,small:1,span:1,strike:1,strong:1,sub:1,sup:1,tt:1,u:1,'var':1} ;

FCK.DTD = (function()
{
    var X = FCKTools.Merge ;

    var A,L,J,M,N,O,D,H,P,K,Q,F,G,C,B,E,I ;
    A = {isindex:1, fieldset:1} ;
    B = {input:1, button:1, select:1, textarea:1, label:1} ;
    C = X({a:1}, B) ;
    D = X({iframe:1}, C) ;
    E = {hr:1, ul:1, menu:1, div:1, blockquote:1, noscript:1, table:1, center:1, address:1, dir:1, pre:1, h5:1, dl:1, h4:1, noframes:1, h6:1, ol:1, h1:1, h3:1, h2:1} ;
    F = {ins:1, del:1, script:1} ;
    G = X({b:1, acronym:1, bdo:1, 'var':1, '#':1, abbr:1, code:1, br:1, i:1, cite:1, kbd:1, u:1, strike:1, s:1, tt:1, strong:1, q:1, samp:1, em:1, dfn:1, span:1}, F) ;
    H = X({sub:1, img:1, object:1, sup:1, basefont:1, map:1, applet:1, font:1, big:1, small:1}, G) ;
    I = X({p:1}, H) ;
    J = X({iframe:1}, H, B) ;
    K = {img:1, noscript:1, br:1, kbd:1, center:1, button:1, basefont:1, h5:1, h4:1, samp:1, h6:1, ol:1, h1:1, h3:1, h2:1, form:1, font:1, '#':1, select:1, menu:1, ins:1, abbr:1, label:1, code:1, table:1, script:1, cite:1, input:1, iframe:1, strong:1, textarea:1, noframes:1, big:1, small:1, span:1, hr:1, sub:1, bdo:1, 'var':1, div:1, object:1, sup:1, strike:1, dir:1, map:1, dl:1, applet:1, del:1, isindex:1, fieldset:1, ul:1, b:1, acronym:1, a:1, blockquote:1, i:1, u:1, s:1, tt:1, address:1, q:1, pre:1, p:1, em:1, dfn:1} ;

    L = X({a:1}, J) ;
    M = {tr:1} ;
    N = {'#':1} ;
    O = X({param:1}, K) ;
    P = X({form:1}, A, D, E, I) ;
    Q = {li:1} ;

    return {
        col: {},
        tr: {td:1, th:1},
        img: {},
        colgroup: {col:1},
        noscript: P,
        td: P,
        br: {},
        th: P,
        center: P,
        kbd: L,
        button: X(I, E),
        basefont: {},
        h5: L,
        h4: L,
        samp: L,
        h6: L,
        ol: Q,
        h1: L,
        h3: L,
        option: N,
        h2: L,
        form: X(A, D, E, I),
        select: {optgroup:1, option:1},
        font: L,		// Changed from L to J (see (1))
        ins: P,
        menu: Q,
        abbr: L,
        label: L,
        table: {thead:1, col:1, tbody:1, tr:1, colgroup:1, caption:1, tfoot:1},
        code: L,
        script: N,
        tfoot: M,
        cite: L,
        li: P,
        input: {},
        iframe: P,
        strong: L,		// Changed from L to J (see (1))
        textarea: N,
        noframes: P,
        big: L,			// Changed from L to J (see (1))
        small: L,		// Changed from L to J (see (1))
        span: L,		// Changed from L to J (see (1))
        hr: {},
        dt: L,
        sub: L,			// Changed from L to J (see (1))
        optgroup: {option:1},
        param: {},
        bdo: L,
        'var': L,		// Changed from L to J (see (1))
        div: P,
        object: O,
        sup: L,			// Changed from L to J (see (1))
        dd: P,
        strike: L,		// Changed from L to J (see (1))
        area: {},
        dir: Q,
        map: X({area:1, form:1, p:1}, A, F, E),
        applet: O,
        dl: {dt:1, dd:1},
        del: P,
        isindex: {},
        fieldset: X({legend:1}, K),
        thead: M,
        ul: Q,
        acronym: L,
        b: L,			// Changed from L to J (see (1))
        a: J,
        blockquote: P,
        caption: L,
        i: L,			// Changed from L to J (see (1))
        u: L,			// Changed from L to J (see (1))
        tbody: M,
        s: L,
        address: X(D, I),
        tt: L,			// Changed from L to J (see (1))
        legend: L,
        q: L,
        pre: X(G, C),
        p: L,
        em: L,			// Changed from L to J (see (1))
        dfn: L
    } ;
})() ;

//from quirksmode.org
FCKTools.GetEventTarget = function( e )
{
	var target = null ;

	if ( !e )
		var e = window.event ;

	if ( e.target )
		target = e.target ;
	else if ( e.srcElement )
		target = e.srcElement ;

	if ( target && target.nodeType == 3 ) // defeat Safari bug
		target = target.parentNode ;

	return target ;
}

/**
 * @see 0004513: Undo operation work incorrectly in Safari
 * @see 0007557: Webkit: paste from keyboard broken
 */
function _FCK_EditingArea_OnLoad()
{
	// Get the editor's window and document (DOM)
	FCK.EditorWindow	= FCK.EditingArea.Window ;
	FCK.EditorDocument	= FCK.EditingArea.Document ;

	if ( FCKBrowserInfo.IsIE )
		FCKTempBin.ToElements() ;

	FCK.InitializeBehaviors() ;

	// Listen for mousedown and mouseup events for tracking drag and drops.
	FCK.MouseDownFlag = false ;
	FCKTools.AddEventListener( FCK.EditorDocument, 'mousemove', _FCK_MouseEventsListener ) ;
	FCKTools.AddEventListener( FCK.EditorDocument, 'mousedown', _FCK_MouseEventsListener ) ;
	FCKTools.AddEventListener( FCK.EditorDocument, 'mouseup', _FCK_MouseEventsListener ) ;
	if ( FCKBrowserInfo.IsSafari )
	{
		// #3481: WebKit has a bug with paste where the paste contents may leak
		// outside table cells. So add padding nodes before and after the paste.
		FCKTools.AddEventListener( FCK.EditorDocument, 'paste', function( evt )
		{
			// >MT
			var range = new FCKDomRange( FCK.EditorWindow );
			range.MoveToSelection();
			range.DeleteContents() ;

			var selectedElement = FCKSelection.GetSelectedElement() || FCKSelection.GetParentElement() ;
			var bookmark = range.CreateBookmark( true ) ;

			var node = FCK.EditorDocument.createElement( 'div' ) ;
			node.id = '_fck_pastebin' ;
			node.className = 'fck_pastebin' ;
			node.innerHTML = '\xa0' ;

			FCKDomTools.SetElementStyles( node,
				{
					position : 'absolute',
					left : '-1000px',
					width : '1px',
					height : '1px',
					overflow : 'hidden'
				} ) ;

			if ( selectedElement )
			{
				FCKDomTools.SetElementStyles( node,
					{
						// Position the bin exactly at the position of the selected element
						// to avoid any subsequent document scroll.
						top : FCKTools.GetDocumentPosition( FCK.EditorWindow, selectedElement ).y + 'px'
					} );
			}

			FCK.EditorDocument.body.appendChild( node ) ;

			range.SetStart( node, 1 ) ;
			range.SetEnd( node, 2 ) ;
			range.Select() ;

			// Remove the padding nodes after the paste is done.
			setTimeout( function()
				{
					var binContents = '',
						$bins = MindTouchDeki.$( FCK.EditorDocument ).find( 'div.fck_pastebin' ),
						br = FCK.EditorDocument.createElement( 'br' ) ;
					
					$bins.each( function( index )
						{
							var $bin = MindTouchDeki.$( this ) ;

							if ( $bins.length > 1 && index == 0 )
							{
								return ;
							}

							// Grab the HTML contents.
							// We need to look for a apple style wrapper on webkit it also adds
							// a div wrapper if you copy/paste the body of the editor.
							// Remove hidden div and restore selection.
							$bin.find( 'span.Apple-style-span' ).each( function()
								{
									FCKDomTools.RemoveNode( this, true ) ;
								} );

							$bin.children( 'meta' ).remove() ;

							binContents += $bin.html() ;

							if ( index < $bins.length - 1 )
							{
								binContents += br.outerHTML ;
							}
						} ) ;

					FCKDomTools.RemoveNode( node ) ;
					range.SelectBookmark( bookmark ) ;
					FCK.InsertHtml( binContents ) ;
				}, 0 ) ;
			// <MT
		} );
	}

	// Most of the CTRL key combos do not work under Safari for onkeydown and onkeypress (See #1119)
	// But we can use the keyup event to override some of these...
	// >MT: #0004513
//	if ( FCKBrowserInfo.IsSafari )
//	{
//		var undoFunc = function( evt )
//		{
//			if ( ! ( evt.ctrlKey || evt.metaKey ) )
//				return ;
//			if ( FCK.EditMode != FCK_EDITMODE_WYSIWYG )
//				return ;
//			switch ( evt.keyCode )
//			{
//				case 89:
//					FCKUndo.Redo() ;
//					break ;
//				case 90:
//					FCKUndo.Undo() ;
//					break ;
//			}
//		}
//
//		FCKTools.AddEventListener( FCK.EditorDocument, 'keyup', undoFunc ) ;
//	}
	// <MT

	// Create the enter key handler
	FCK.EnterKeyHandler = new FCKEnterKey( FCK.EditorWindow, FCKConfig.EnterMode, FCKConfig.ShiftEnterMode, FCKConfig.TabSpaces ) ;

	// Listen for keystroke events.
	FCK.KeystrokeHandler.AttachToElement( FCK.EditorDocument ) ;

	if ( FCK._ForceResetIsDirty )
		FCK.ResetIsDirty() ;

	// This is a tricky thing for IE. In some cases, even if the cursor is
	// blinking in the editing, the keystroke handler doesn't catch keyboard
	// events. We must activate the editing area to make it work. (#142).
	if ( FCKBrowserInfo.IsIE && FCK.HasFocus )
		FCK.EditorDocument.body.setActive() ;

	FCK.OnAfterSetHTML() ;

	// Restore show blocks status.
	FCKCommands.GetCommand( 'ShowBlocks' ).RestoreState() ;

	// Check if it is not a startup call, otherwise complete the startup.
	if ( FCK.Status != FCK_STATUS_NOTLOADED )
		return ;

	FCK.SetStatus( FCK_STATUS_ACTIVE ) ;
}

/**
 * @see #0007557
 */
if ( !FCKBrowserInfo.IsIE )
{
	FCK.InsertHtml = function( html )
	{
		var doc = FCK.EditorDocument,
			range;

		html = FCKConfig.ProtectedSource.Protect( html ) ;
		html = FCK.ProtectEvents( html ) ;
		html = FCK.ProtectUrls( html ) ;
		html = FCK.ProtectTags( html ) ;

		// Save an undo snapshot first.
		FCKUndo.SaveUndoStep() ;

		// >MT
//		if ( FCKBrowserInfo.IsGecko )
		if ( FCKBrowserInfo.IsGecko || FCKBrowserInfo.IsSafari )
		// <MT
		{
			html = html.replace( /&nbsp;$/, '$&<span _fcktemp="1"/>' ) ;

			var docFrag = new FCKDocumentFragment( this.EditorDocument ) ;
			docFrag.AppendHtml( html ) ;

			var lastNode = docFrag.RootNode.lastChild ;

			range = new FCKDomRange( this.EditorWindow ) ;
			range.MoveToSelection() ;

			// If the first element (if exists) of the document fragment is a block
			// element, then split the current block. (#1537)
			var currentNode = docFrag.RootNode.firstChild ;
			while ( currentNode && currentNode.nodeType != 1 )
				currentNode = currentNode.nextSibling ;

			if ( currentNode && FCKListsLib.BlockElements[ currentNode.nodeName.toLowerCase() ] )
				range.SplitBlock() ;

			range.DeleteContents() ;
			range.InsertNode( docFrag.RootNode ) ;

			range.MoveToPosition( lastNode, 4 ) ;
		}
		else
			doc.execCommand( 'inserthtml', false, html ) ;

		this.Focus() ;

		// Save the caret position before calling document processor.
		if ( !range )
		{
			range = new FCKDomRange( this.EditorWindow ) ;
			range.MoveToSelection() ;
		}
		var bookmark = range.CreateBookmark() ;

		FCKDocumentProcessor.Process( doc ) ;

		// Restore caret position, ignore any errors in case the document
		// processor removed the bookmark <span>s for some reason.
		try
		{
			range.MoveToBookmark( bookmark ) ;
			range.Select() ;
		}
		catch ( e ) {}

		// For some strange reason the SaveUndoStep() call doesn't activate the undo button at the first InsertHtml() call.
		this.Events.FireEvent( "OnSelectionChange" ) ;
	}
}

/**
 * Opera calculates context menu position incorrectly
 * @see #0005911
 */
function FCKContextMenu_AttachedElement_OnContextMenu( ev, fckContextMenu, el )
{
	if ( ( fckContextMenu.CtrlDisable && ( ev.ctrlKey || ev.metaKey ) ) || FCKConfig.BrowserContextMenu )
		return true ;

	var eTarget = el || this ;

	if ( fckContextMenu.OnBeforeOpen )
		fckContextMenu.OnBeforeOpen.call( fckContextMenu, eTarget ) ;

	if ( fckContextMenu._MenuBlock.Count() == 0 )
		return false ;

	if ( fckContextMenu._Redraw )
	{
		fckContextMenu._MenuBlock.Create( fckContextMenu._Panel.MainNode ) ;
		fckContextMenu._Redraw = false ;
	}

	// This will avoid that the content of the context menu can be dragged in IE
	// as the content of the panel is recreated we need to do it every time
	FCKTools.DisableSelection( fckContextMenu._Panel.Document.body ) ;

	var x = 0 ;
	var y = 0 ;
	if ( FCKBrowserInfo.IsIE )
	{
		x = ev.screenX ;
		y = ev.screenY ;
	}
	else if ( FCKBrowserInfo.IsSafari )
	{
		x = ev.clientX ;
		y = ev.clientY ;
	}
	else
	{
		x = ev.pageX ;
		y = ev.pageY ;

		if ( FCKBrowserInfo.IsOpera && ev.target )
		{
			var doc = FCKTools.GetElementDocument( ev.target ) ;

			if ( doc )
				y += doc.body.scrollTop ;
		}
	}
	fckContextMenu._Panel.Show( x, y, ev.currentTarget || null ) ;

	return false ;
}
