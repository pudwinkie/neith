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

var FCKInsertLinkCommand = function() {}

FCKInsertLinkCommand.prototype =
{
	GetState : MindTouchDeki.GetState,

	Execute : function()
	{
		var oParams = this._GetParams() ;

		var aHref = [ FCKConfig.DekiCommonPath + '/popups/link_dialog.php',
					 '?href=' + oParams.f_href,
//					 '?href=' + oParams.f_href.utf8URL(),
					 '&contextID=' + FCKConfig.PageId,
					 '&cntxt=' + FCKConfig.PageTitle.utf8URL(),
					 '&userName=' + FCKConfig.UserName
					 ];

		MindTouchDeki.OpenDialog( 'InsertLink', aHref.join(''), "600px", "285px", oParams, this._InsertLink, this );
	},

	_GetParams : function()
	{
		var oParams = {} ;

		// oLink: The actual selected link in the editor.
		this._Link = FCK.Selection.MoveToAncestorNode( 'A' ) ;

		if ( this._Link )
			FCK.Selection.SelectNode( this._Link ) ;

		if ( ! this._Link )
		{
			var selectedText = MindTouchDeki.GetSelectedText() ;
			selectedText = selectedText ? selectedText : '' ;
			
			var sHref = ( MindTouchDeki.IsLinkInternal( selectedText ) ) ?
					'./' + encodeURIComponent(selectedText) : encodeURI(selectedText) ;

			oParams = {
				'f_href'		: sHref,
				'f_text'		: selectedText,
				'contextTopic'	: FCKConfig.PageTitle,
				'contextTopicID': FCKConfig.PageId,
				'userName'		: FCKConfig.UserName,
				'newlink'		: true
			} ;

		}
		else
		{
			if ( this._Link.className == 'site' )
			{
				return ;
			}

			var sHRef = this._Link.getAttribute( '_fcksavedurl' ) ;

			if ( sHRef == null )
				sHRef = this._Link.getAttribute( 'href' , 2 ) || '' ;

//			sHRef = sHRef.replace( /([^!-~]+)/g, function( match ) { return encodeURI( match ); } );

			oParams = {
				'f_href'		: sHRef,
				'f_text'		: MindTouchDeki.GetInnerText(this._Link),
				'contextTopic'	: FCKConfig.PageTitle,
				'contextTopicID': FCKConfig.PageId,
				'userName'		: FCKConfig.UserName
			};
		}

//		oParams.f_href = MindTouchDeki.HrefDecode( oParams.f_href ) ;

		if ( oParams.f_href.indexOf(MindTouchDeki.InternalPrefix) == 0 )
		{
			oParams.f_href = oParams.f_href.substr( MindTouchDeki.InternalPrefix.length ) ;
		}

		return oParams ;
	},

	_InsertLink : function( oParams )
	{
		FCKUndo.SaveUndoStep() ;

		var sInnerHtml ;

		var oLink = this._Link ;

		if ( ! oParams )
		{
			return false ;
		}

		var sText = oParams.f_text ;
		var sUri = oParams.f_href ;

		if ( 0 == sText.length )
		{
			sText = sUri ;
		}

		sUri = MindTouchDeki.NormalizeUri( sUri ) ;

		// If no link is selected, create a new one (it may result in more than one link creation - #220).
		var aLinks = oLink ? [ oLink ] : FCK.CreateLink( sUri, true ) ;

		// If no selection, no links are created, so use the uri as the link text (by dom, 2006-05-26)
		var aHasSelection = ( aLinks.length > 0 ) ;

		if ( !aHasSelection )
		{
			sInnerHtml = sText ;

			// Create a new (empty) anchor.
			aLinks = [ FCK.InsertElement( 'a' ) ] ;
		}

		for ( var i = 0 ; i < aLinks.length ; i++ )
		{
			oLink = aLinks[i] ;

			if ( aHasSelection )
				sInnerHtml = oLink.innerHTML ;	// Save the innerHTML (IE changes it if it is like an URL).

//			sUri = MindTouchDeki.EncodeURI( sUri ) ;

			oLink.href = sUri ;
			oLink.innerHTML = sInnerHtml ;		// Set (or restore) the innerHTML

			oLink.setAttribute( '_fcksavedurl', sUri ) ;

			MindTouchDeki.NormalizeLink( oLink ) ;
		}

		// Select the (first) link.
		FCKSelection.SelectNode( aLinks[0] );
	}
}

var FCKInsertQuickLinkCommand = function() {}

FCKInsertQuickLinkCommand.prototype = new FCKInsertLinkCommand ;

FCKInsertQuickLinkCommand.prototype.Execute = function()
{
	var oParams = this._GetParams() ;
	this._InsertLink( oParams ) ;
}


FCKCommands.RegisterCommand( 'MindTouchDeki_InsertLink', new FCKInsertLinkCommand );
FCKCommands.RegisterCommand( 'MindTouchDeki_InsertQuickLink', new FCKInsertQuickLinkCommand );

var oInsertLinkItem = new FCKToolbarButton( 'MindTouchDeki_InsertLink', FCKLang.InsertLinkLbl, FCKLang.InsertLink, null, false, true, 34 ) ;
FCKToolbarItems.RegisterItem( 'MindTouchDeki_InsertLink', oInsertLinkItem ) ;
