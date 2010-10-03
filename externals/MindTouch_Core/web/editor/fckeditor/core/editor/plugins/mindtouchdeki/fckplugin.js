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

String.prototype.utf8URL = parent.String.prototype.utf8URL ;
String.prototype.utf8ToString = parent.String.prototype.utf8ToString ;

var MindTouchDeki = new Object() ;

MindTouchDeki.$ = parent.Deki.$ ;

MindTouchDeki.InternalPrefix = "mks://localhost/" ;
MindTouchDeki.ExternalRegex = /^([a-z]+:)[\/]{2,5}/i ;
MindTouchDeki.EmailRegex = /^(mailto:)?[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$/i ;

MindTouchDeki.IsLinkExternal = function( href )
{
	return ( href.match(MindTouchDeki.ExternalRegex) != null ) && href.indexOf(MindTouchDeki.InternalPrefix) != 0 ;
}

MindTouchDeki.IsLinkInternal = function( href )
{
	return ( ! ( MindTouchDeki.IsLinkExternal( href ) ||
		MindTouchDeki.IsLinkNetwork( href ) ||
		MindTouchDeki.IsEmail( href ) ) );
}

MindTouchDeki.IsLinkNetwork = function( href )
{
	// don't put this condition in one line since fckpackager processes it incorrectly
	return ( href.indexOf( "\\" ) == 0 ) ||
			( href.indexOf( "//" ) == 0 ) ;
}

MindTouchDeki.IsEmail = function( email )
{
	return MindTouchDeki.EmailRegex.test( email ) ;
}

MindTouchDeki.IsUrl = function( href )
{
	var regexp = /(^(news|telnet|nttp|http|ftp|https):\/\/(\w+:{0,1}\w*@)?(\S+)(:[0-9]+)?(\/|\/([\w#!:.?+=&%@!\-\/]))?)|^(file:\/{2,5}([^\s\/\\:\*\?"<>#$&',:;\[`\{\|\}])+)/i ;
	return regexp.test( href ) ;
}

MindTouchDeki.EncodeURI = function( uri )
{
	// normalize uri - decode and unescape
	// see #0006537
	try
	{
		uri = decodeURI( uri ) ;
	}
	catch(ex) {}
	
	uri = unescape( uri ) ;
	
	uri = uri.replace( /([^0-9A-Za-z=:\/\\]+)/g, function( match, p1, offset, source )
	{
		var result = match ;
		
		var s1 = source.substr( 0, offset ) ;
		var s2 = source.substr( offset + match.length ) ;
		
		var matches1 = s1.match( /\{\{/g ) ;
		var matches2 = s2.match( /\}\}/g ) ;
		
		if ( !matches1 || !matches2 || matches1.length == matches2.length )
		{
			result = encodeURI( match ) ;
		}
		
		return result ;
	} );
	
	return uri ;
}

MindTouchDeki.HrefEncode = function( str )
{
	function hex( d )
	{
		return ( d < 16 ) ? ( "0" + d.toString(16) ) : d.toString( 16 ) ;
	}
	
	if ( typeof str.replace == 'undefined' )
	{
		str = str.toString() ;
	}
	
	var isInternal = ( str.indexOf( MindTouchDeki.InternalPrefix ) == 0 ) ;
	if ( isInternal )
	{
		str = str.substr( MindTouchDeki.InternalPrefix.length ) ;
		str = str.utf8URL() ;
		str = str.replace( /\x2F\x2F/g, "%2f%2f" ) ;
		str = str.replace( /\x2E/g, "%2e" ) ;
		str = MindTouchDeki.InternalPrefix + str ;
	}
	
	return str ;
}

MindTouchDeki.HrefDecode = function( str )
{
	if( typeof str.replace == 'undefined' )
	{
		str = str.toString() ;
	}
	
	str = str.replace( /&nbsp;/ig, " " ) ;
	str = unescape( str ) ;
	str = str.utf8ToString() ;
	
	if ( str.indexOf( MindTouchDeki.InternalPrefix ) == 0 )
	{
		str = str.replace( /_/ig, " " ) ;
	}
	
	return str ;
}

MindTouchDeki.NormalizeUri = function( sUri )
{
	if ( MindTouchDeki.IsEmail( sUri ) )
	{
		if ( sUri.toLowerCase().indexOf( 'mailto:' ) != 0 )
		{
			sUri = 'mailto:' + sUri;
		}

		return sUri ;
	}

	if ( MindTouchDeki.IsLinkNetwork( sUri ) )
	{
		return 'file:///' + sUri.replace( /\\/g, '/' ) ;
	}

	if ( MindTouchDeki.IsLinkExternal( sUri ) )
	{
		return sUri ;
	}

	// internal links

	if ( sUri != "" && sUri.indexOf( MindTouchDeki.InternalPrefix ) != 0 && sUri.indexOf( '/' ) != 0 )
	{
		sUri = MindTouchDeki.InternalPrefix + sUri ;
	}

	sUri = sUri.replace( / /g, '_' ) ;

	return sUri ;
}

MindTouchDeki.NormalizeLink = function( node )
{
	node.title = MindTouchDeki.HrefDecode( node.href ) ;
	
	// bugfix #2643, let parser choose classes
	if ( node.title.indexOf( MindTouchDeki.InternalPrefix ) == 0 )
	{
		node.title = node.title.substr( MindTouchDeki.InternalPrefix.length ) ;
	
		MindTouchDeki.$( node ).removeClass( 'external' ) ;
	}
}

MindTouchDeki.StripHost = function( href )
{
	var host = document.location.protocol + '//' + document.location.host ;

	if ( href.indexOf( host ) == 0 )
	{
		href = href.substring( host.length ) ;
	}

	return href ;
}

MindTouchDeki.GetUrlFromName = function( href, addContext )
{
	var isInternal = MindTouchDeki.IsLinkInternal( href ) ;
	
	if ( isInternal )
	{
		if ( href.indexOf( MindTouchDeki.InternalPrefix ) == 0 )
		{
			href = href.substr( MindTouchDeki.InternalPrefix.length ) ;
			href = '/index.php?title=' + href ;
			href = href.replace( /&[a-z]+=.*$/i, '' ) ;
			href = href.replace( / /g, '_' ) ;
			
			if ( addContext )
			{
				href += "&contextid=" + FCKConfig.PageId ;
			}
		}
	}
	else
	{
		if ( href.toLowerCase().indexOf( 'mailto:' ) == 0 )
		{
			if ( MindTouchDeki.IsEmail( href ) )
			{
				alert( FCKLang.ValidEmail ) ;
			}
			else
			{
				alert( FCKLang.InvalidEmail ) ;
			}
			return false ;
		}
	
		if ( !MindTouchDeki.IsUrl( href ) )
		{
			alert( FCKLang.InvalidUrl ) ;
			return false ;
		}
	}
	return href ;
}

MindTouchDeki.GetInnerText = function(el)
{
	var txt = '', i;
	for ( i = el.firstChild ; i ; i = i.nextSibling )
	{
		if ( i.nodeType == 3 )
		{
			txt += i.data ;
		}
		else if ( i.nodeType == 1 )
		{
			txt += MindTouchDeki.GetInnerText( i ) ;
		}
	}
	return txt;
}

MindTouchDeki.GetSelectedText = function()
{
	if ( FCKBrowserInfo.IsIE )
	{
		var range = MindTouchDeki.CreateRange() ;
		return range.text ;
	}
	else
	{
		var selection = FCKSelection.GetSelection() ;
		return selection.toString() ;
	}
}

MindTouchDeki.CreateRange = function( selection )
{
	selection = selection || FCKSelection.GetSelection() ;
	var range = null ;

	if ( FCKBrowserInfo.IsIE )
	{
		try
		{
			range = selection.createRange() ;
		}
		catch( ex ) {}
	}
	else
	{
		try
		{
			range = selection.getRangeAt(0) ;
		}
		catch( ex )
		{
			range = FCK.EditorDocument.createRange() ;
		}
	}

	return range ;
}

MindTouchDeki.GetNextSibling = function( node, siblingNames )
{
	if ( typeof ( siblingNames ) == 'string' )
		siblingNames = [ siblingNames ] ;

	var eSibling = node.nextSibling ;
	while( eSibling )
	{
		if ( eSibling.nodeType == 1 )
		{
			if ( eSibling.tagName.Equals.apply( eSibling.tagName, siblingNames ) )
				return eSibling ;
		}

		eSibling = eSibling.nextSibling ;
	}

	return null ;
}

MindTouchDeki.GetPreviousSibling = function( node, siblingNames )
{
	if ( typeof ( siblingNames ) == 'string' )
		siblingNames = [ siblingNames ] ;

	var eSibling = node.previousSibling ;
	while( eSibling )
	{
		if ( eSibling.nodeType == 1 )
		{
			if ( eSibling.tagName.Equals.apply( eSibling.tagName, siblingNames ) )
				return eSibling ;
		}

		eSibling = eSibling.previousSibling ;
	}

	return null ;
}

// returns true if some cells are selected in FF
// in all other cases it returns false
MindTouchDeki.CheckTable = function()
{
	if ( FCKBrowserInfo.IsGecko )
	{
		var oSelection = FCKSelection.GetSelection() ;

		if ( oSelection.rangeCount > 1 )
		{
			var eParent = FCKSelection.GetSelectedElement() ;

			if ( eParent && eParent.nodeName.toLowerCase() == 'td' )
			{
				return true ;
			}
		}
	}

	return false ;
}

/**
 * Returns computed style for element
 * @param Element element
 * @param String propertyName - css property name, e.g. width, font-size etc.
 */
MindTouchDeki.GetCurrentElementStyle = function( element, propertyName )
{
	if ( FCKBrowserInfo.IsIE )
	{
		var property = propertyName.split( '-' ) ;
		for ( var i = 1 ; i < property.length ; i++ )
		{
			var p = property[i] ;
			property[i] = p.charAt(0).toUpperCase() + p.substr(1) ;
		}

		propertyName = property.join('') ;

		return element.currentStyle[ propertyName ] ;
	}
	else
		return element.ownerDocument.defaultView.getComputedStyle( element, '' ).getPropertyValue( propertyName ) ;
}

MindTouchDeki.ScrollToTop = function()
{
	if ( FCKConfig.ToolbarLocation != "In" )
	{
		var win = FCKTools.GetElementWindow( FCK.EditingArea.TargetElement ) ;
		var mainFrame = win.frameElement ;
		
		var mainDoc = FCKTools.GetElementDocument( mainFrame ) ;
		
		var pos = MindTouchDeki.$( mainFrame ).position() ;
		var toolbarHeight = FCK.ToolbarSet._IFrame.offsetHeight ;
		
		var newPos = pos.top - toolbarHeight ;
		
		if ( MindTouchDeki.$( mainDoc ).scrollTop() > newPos )
		{
			parent.scrollTo( 0, newPos ) ;
		}
	}
}

// Copy all the attributes from one node to the other, kinda like a clone
// But oSkipAttributes is an object with the attributes that must NOT be copied
MindTouchDeki.CopyAttributes = function( oSource, oDest, oSkipAttributes )
{
	var aAttributes = oSource.attributes ;

	for ( var n = 0 ; n < aAttributes.length ; n++ )
	{
		var oAttribute = aAttributes[n] ;

		if ( oAttribute.specified )
		{
			var sAttName = oAttribute.nodeName ;
			// We can set the type only once, so do it with the proper value, not copying it.
			if ( sAttName in oSkipAttributes )
				continue ;

			var sAttValue = oSource.getAttribute( sAttName, 2 ) ;
			if ( sAttValue == null )
				sAttValue = oAttribute.nodeValue ;

			oDest.setAttribute( sAttName, sAttValue, 0 ) ;	// 0 : Case Insensitive
		}
	}
	// The style:
	if ( oSource.style.cssText !== '' || oSource.getAttribute('_fckstyle') )
		oDest.style.cssText = oSource.getAttribute('_fckstyle') || oSource.style.cssText ;
}

/**
* Replaces a tag with another one, keeping its contents:
* for example TD --> TH, and TH --> TD.
* input: the original node, and the new tag name
* http://www.w3.org/TR/DOM-Level-3-Core/core.html#Document3-renameNode
*/
MindTouchDeki.RenameNode = function( oNode , newTag )
{
	// TODO: if the browser natively supports document.renameNode call it.
	// does any browser currently support it in order to test?

	// Only rename element nodes.
	if ( oNode.nodeType != 1 )
		return null ;

	// If it's already correct exit here.
	if ( oNode.nodeName == newTag )
		return oNode ;

	var oDoc = oNode.ownerDocument ;
	// Create the new node
	var newNode = oDoc.createElement( newTag ) ;

	// Copy all attributes
	MindTouchDeki.CopyAttributes( oNode, newNode, {} ) ;

	// Move children to the new node
	FCKDomTools.MoveChildren( oNode, newNode ) ;

	// Finally replace the node and return the new one
	oNode.parentNode.replaceChild( newNode, oNode ) ;

	return newNode ;
}


MindTouchDeki.IsArray = function( o )
{
	return Object.prototype.toString.call(o) === '[object Array]' ; 
}


MindTouchDeki.OpenDialog = function( name, url, width, height, params, callback, scope )
{
	var Deki = parent.Deki ;

	var dialog = new Deki.Dialog({
		'src' : url,
		'width' : width,
		'height' : height,
		'buttons' : [
			Deki.Dialog.BTN_OK,
			Deki.Dialog.BTN_CANCEL
		],
		'args' : params,
		'callback' : callback,
		'scope' : scope
	}) ;

	dialog.render();
	dialog.show();
};

(function() {
	if ( FCKConfig.ToolbarLocation != "In" )
	{
		LoadScript( FCKConfig.PluginsPath + 'mindtouchdeki/autogrow.js' ) ;
		
		var i = 0, j = 0, toolbarSet = FCKURLParams['Toolbar'] || 'Default' ;
		
		if ( !FCKConfig.ToolbarSets[toolbarSet] )
			return ;
		
		while ( i < FCKConfig.ToolbarSets[toolbarSet].length )
		{
			if ( MindTouchDeki.IsArray(FCKConfig.ToolbarSets[toolbarSet][i]) )
			{
				j = 0;
				while ( j < FCKConfig.ToolbarSets[toolbarSet][i].length )
				{
					if ( FCKConfig.ToolbarSets[toolbarSet][i][j] == 'FitWindow' )
					{
						if ( FCKConfig.ToolbarSets[toolbarSet][i].length == 1 )
						{
							// ['FitWindow']
							FCKConfig.ToolbarSets[toolbarSet].splice( i, 1 ) ;
							break;
						}
						else
						{
							var index, howMany ;
							
							if ( j == 0 && FCKConfig.ToolbarSets[toolbarSet][i][j+1] == '-' )
							{
								// ['FitWindow','-',...]
								index = j ;
								howMany = 2 ;
							}
							else if ( j == FCKConfig.ToolbarSets[toolbarSet][i].length - 1 && FCKConfig.ToolbarSets[toolbarSet][i][j-1] == '-' )
							{
								// [...,'-','FitWindow']
								index = j - 1 ;
								howMany = 2 ;
							}
							else if ( FCKConfig.ToolbarSets[toolbarSet][i][j-1] == '-' && FCKConfig.ToolbarSets[toolbarSet][i][j+1] == '-' )
							{
								// [...,'-','FitWindow','-',...]
								index = j - 1;
								howMany = 3 ;
							}
							else
							{
								// common case
								index = j ;
								howMany = 1 ;
							}
							
							j = FCKConfig.ToolbarSets[toolbarSet][i].length - 1 - howMany ;
							FCKConfig.ToolbarSets[toolbarSet][i].splice( index, howMany ) ;
						}
					}
					else
					{
						j++ ;
					}
				}
			}
			
			i++ ;
		}
	}
})();

/**
 * Context menu
 */

FCK.ContextMenu.RegisterListener( {
	AddItems : function( menu, tag, tagName )
	{
		var bInsideLink = ( tagName == 'A' || FCKSelection.HasAncestorNode( 'A' ) ) ;

		if ( FCKCommands.GetCommand( 'MindTouchDeki_InsertLink' ).GetState() != FCK_TRISTATE_DISABLED )
		{
			// Go up to the anchor to test its properties
			var oLink = FCKSelection.MoveToAncestorNode( 'A' ) ;
			var bIsAnchor = ( oLink && oLink.name.length > 0 && oLink.href.length == 0 ) ;
			// If it isn't a link then don't add the Link context menu
			if ( bIsAnchor )
				return ;

			menu.AddSeparator() ;

			if ( bInsideLink )
			{
				menu.AddItem( 'VisitLink', FCKLang.VisitLink ) ;
				menu.AddSeparator() ;
				menu.AddItem( 'MindTouchDeki_InsertLink', FCKLang.EditLink, 34 ) ;
				menu.AddItem( 'Unlink', FCKLang.RemoveLink, 35 ) ;
			}
			else
			{
				menu.AddItem( 'MindTouchDeki_InsertLink', FCKLang.InsertLinkCM, 34 ) ;
			}
		}
	}}
);

FCK.ContextMenu.RegisterListener( {
	AddItems : function( menu, tag, tagName )
	{
		if ( tagName == 'IMG' && !tag.getAttribute( '_fckfakelement' ) )
		{
			menu.AddSeparator() ;
			menu.AddItem( 'MindTouchDeki_InsertImage', FCKLang.ImageProperties, 37 ) ;
		}
	}}
);

MindTouchDeki.GetChildNodes = function( element, nodeNames )
{
	var i, node, nodes = [] ;
	
	for ( i = 0 ; i < element.childNodes.length ; i++ )
	{
		node = element.childNodes[i] ;
		if ( node.nodeName.IEquals( nodeNames ) )
		{
			nodes.push( node ) ;
		}
		
		if ( node.childNodes.length )
		{
			nodes = nodes.concat( MindTouchDeki.GetChildNodes( node, nodeNames ) ) ;
		}
	}
	
	return nodes ;
};

MindTouchDeki_MouseClickTarget = null ;

/**
 * Updates _fckstyle attribute on images resizing
 * see #7319
 */
MindTouchDeki_OnMouseUp = function( evt )
{
	if ( MindTouchDeki_MouseClickTarget )
	{
		var images = [], img, i ;
		
		if ( MindTouchDeki_MouseClickTarget.nodeName.toLowerCase() == 'img' )
		{
			images.push( MindTouchDeki_MouseClickTarget ) ;
		}
		else
		{
			images = MindTouchDeki.GetChildNodes( MindTouchDeki_MouseClickTarget, 'img' ) ;
		}
		
		for ( i = 0 ; i < images.length ; i++ )
		{
			img = images[i] ;
			
			if ( img.style.cssText.length > 0 )
			{
				img.setAttribute( '_fckstyle', img.style.cssText ) ;
			}
		}
		
		MindTouchDeki_MouseClickTarget = null ;
	}
}

MindTouchDeki_OnMouseDown = function( evt )
{
	MindTouchDeki_MouseClickTarget = FCKTools.GetEventTarget( evt ) ;
}

FCK.Events.AttachEvent( "OnAfterSetHTML", function()
	{
		if ( FCK.EditMode != FCK_EDITMODE_WYSIWYG || ! FCK.EditorDocument )
			return;

		FCKTools.AddEventListener( FCK.EditorDocument, 'mouseup', MindTouchDeki_OnMouseUp ) ;
		FCKTools.AddEventListener( FCK.EditorDocument, 'mousedown', MindTouchDeki_OnMouseDown ) ;
	} ) ;

MindTouchDeki_OnStatusChange = function( editorInstance, newStatus )
{
	if ( newStatus == FCK_STATUS_COMPLETE )
	{
		for ( var i = 0 ; i < FCK.ToolbarSet.Items.length ; i++ )
		{
			var oItem = FCK.ToolbarSet.Items[i] ;

			if ( oItem._UIButton && oItem._UIButton.Style == FCK_TOOLBARITEM_ONLYICON )
			{
				var oButtonContainer = oItem._UIButton.MainElement ;

				if ( oButtonContainer.firstChild && oButtonContainer.firstChild.nodeName.toLowerCase() == 'img' )
				{
					oButtonContainer.firstChild.setAttribute( 'alt', oItem._UIButton.Tooltip ) ;
				}
			}
		}
	}
}

FCK.Events.AttachEvent( "OnStatusChange", MindTouchDeki_OnStatusChange ) ;

/**
 * Commands' State
 */

MindTouchDeki.GetState = function()
{
	// Disabled if not WYSIWYG.
	if ( FCK.EditMode != FCK_EDITMODE_WYSIWYG || ! FCK.EditorWindow )
		return FCK_TRISTATE_DISABLED ;

	// Disabled if cursor is in the header.
	var path = new FCKElementPath( FCKSelection.GetBoundaryParentElement( true ) ) ;
	var firstBlock = path.Block || path.BlockLimit ;

	if ( !firstBlock || firstBlock.nodeName.toLowerCase() == 'body' )
		return FCK_TRISTATE_OFF ;

	// See if the first block has a h1 parent.
	for ( var i = 0 ; i < path.Elements.length ; i++ )
	{
		if ( path.Elements[i].nodeName.IEquals( ['h1'] ) &&
			FCK.EditorDocument.body.firstChild === path.Elements[i] )
		{
			return FCK_TRISTATE_DISABLED ;
		}
	}
	return FCK_TRISTATE_OFF ;
};
