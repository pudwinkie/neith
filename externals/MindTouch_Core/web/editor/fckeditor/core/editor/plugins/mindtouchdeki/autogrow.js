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
 * Plugin: automatically resizes the editor until a configurable maximun
 * height (FCKConfig.AutoGrowMax), based on its contents.
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
 
/*
 * Auto Expanding Text Area (1.2.2)
 * by Chrys Bader (www.chrysbader.com)
 * chrysb@gmail.com
 *
 * Special thanks to:
 * Jake Chapa - jake@hybridstudio.com
 * John Resig - jeresig@gmail.com
 *
 * Copyright (c) 2008 Chrys Bader (www.chrysbader.com)
 * Dual licensed under the MIT (MIT-LICENSE.txt)
 * and GPL (GPL-LICENSE.txt) licenses.
 *
 */

var FCKAutoGrow =
{
	Interval : null,
	Dummy : null,
	Textarea : null,
	LineHeight : 0,
	MinHeight : FCKConfig.AutoGrowMin || 0,

	CheckWysiwyg : function()
	{
		if ( !FCK.EditorWindow || !window.frameElement )
			return ;
		
		var oInnerDoc = FCK.EditorDocument ;

		var iFrameHeight, iInnerHeight ;

		if ( FCKBrowserInfo.IsIE )
		{
			iFrameHeight = FCK.EditorWindow.frameElement.offsetHeight ;
			iInnerHeight = oInnerDoc.body.scrollHeight ;
		}
		else if ( FCKBrowserInfo.IsOpera )
		{
			iFrameHeight = FCK.EditorWindow.innerHeight ;
			iInnerHeight = oInnerDoc.body.scrollHeight ;
		}
		else
		{
			iFrameHeight = FCK.EditorWindow.innerHeight ;
			iInnerHeight = oInnerDoc.body.offsetHeight + 60 ;
		}

		if ( iInnerHeight < FCKAutoGrow.MinHeight )
		{
			iInnerHeight = FCKAutoGrow.MinHeight ;
		}

		var iDiff = iInnerHeight - iFrameHeight ;

		if ( iDiff != 0 )
		{
			var iMainFrameSize = window.frameElement.offsetHeight ;

			if ( iDiff > 0 )
			{
				iMainFrameSize += iDiff ;
			}
			else if ( iDiff < 0 && iMainFrameSize > iInnerHeight )
			{
				iMainFrameSize += iDiff ;
				if ( iMainFrameSize < iInnerHeight )
					iMainFrameSize = iInnerHeight ;
			}
			else
				return ;

			// see #0003766
			if ( FCKBrowserInfo.IsIE )
			{
				iMainFrameSize += 5;
			}

			window.frameElement.style.height = iMainFrameSize + "px" ;

			// see #0004494
			if ( FCKBrowserInfo.IsSafari )
			{
				FCK.EditorWindow.frameElement.style.height = iMainFrameSize - 5 + "px" ;
			}

			// Gecko browsers use an onresize handler to update the innermost
			// IFRAME's height. If the document is modified before the onresize
			// is triggered, the plugin will miscalculate the new height. Thus,
			// forcibly trigger onresize. #1336
			if ( typeof window.onresize == 'function' )
				window.onresize() ;
				
			// FF on Windows platform has iframe height limitation
			// Maximum iframe height is 32768
			// Setting any larger value will result in incrementing of parent window height only
			// @see #0007138
			if ( FCKBrowserInfo.IsGecko && iDiff > 0 && FCK.EditorWindow.innerHeight < oInnerDoc.body.offsetHeight && FCK.EditorWindow.innerHeight == 32763 )
			{
				window.frameElement.style.height = "32763px" ;
				if ( typeof window.onresize == 'function' )
					window.onresize() ;
			}
		}
	},

	CheckSource : function()
	{
		if ( !window.frameElement )
			return ;
			
		var paddingLeft = MindTouchDeki.GetCurrentElementStyle( FCKAutoGrow.Textarea, 'padding-left' );
		var paddingRight = MindTouchDeki.GetCurrentElementStyle( FCKAutoGrow.Textarea, 'padding-right' );

		if ( null === FCKAutoGrow.Dummy )
		{
			FCKAutoGrow.Dummy = document.createElement( 'div' ) ;			
			FCKDomTools.SetElementStyles( FCKAutoGrow.Dummy,
				{
					'fontSize'		: MindTouchDeki.GetCurrentElementStyle( FCKAutoGrow.Textarea, 'font-size' ),
					'fontFamily'	: MindTouchDeki.GetCurrentElementStyle( FCKAutoGrow.Textarea, 'font-family' ),
					'paddingTop'	: MindTouchDeki.GetCurrentElementStyle( FCKAutoGrow.Textarea, 'padding-top' ),
					'paddingRight'	: paddingRight,
					'paddingBottom'	: MindTouchDeki.GetCurrentElementStyle( FCKAutoGrow.Textarea, 'padding-bottom' ),
					'paddingLeft'	: paddingLeft,
					'lineHeight'	: MindTouchDeki.GetCurrentElementStyle( FCKAutoGrow.Textarea, 'line-height' ),
					'overflowX'		: 'hidden',
					'position'		: 'absolute',
					'top'			: 0,
					'left'			: -9999,
					'white-space'	: 'pre-wrap'
				} ) ;
				
			if ( FCKBrowserInfo.IsIE )
			{
				// IE < 8 doesn't support white-space: pre-wrap
				FCKAutoGrow.Dummy.style.wordWrap = 'break-word' ;
			}
			
			document.body.appendChild( FCKAutoGrow.Dummy ) ;
				
			if ( !FCKBrowserInfo.IsIE || FCKConfig.IESourceAutoGrow )
				FCKAutoGrow.Textarea.style.overflowY = 'hidden' ;
		}
		
		// Change width after resize
		var paddingWidth = parseInt(paddingLeft) + parseInt(paddingRight);
		FCKAutoGrow.Dummy.style.width = (FCKAutoGrow.Textarea.offsetWidth - paddingWidth) + 'px' ;
		
		var Html = FCKAutoGrow.Textarea.value.replace( /(<|>)/g, '_' ) ;
		Html = Html.replace( /&/g, '&amp;' ) ;
		
		if ( FCKBrowserInfo.IsIE )
		{
			Html = Html.replace( /\r\n|\n/g, '<BR>' ) ;
		}
		else
		{
			Html = Html.replace( /\r\n|\n/g, '<br>' ) ;
		}
		
		Html = Html.replace( /(\s{2,})/g, function( str, spaces ) {
			var s = '' ;
			for ( var i = 0 ; i < spaces.length - 1 ; i++ )
			{
				s += '&nbsp;';
			}
			return s + ' ' ;
		} ) ;
		
		if ( FCKAutoGrow.Dummy.innerHTML != Html )
		{
			FCKAutoGrow.Dummy.innerHTML = Html ;
		
			var iTextareaHeight = MindTouchDeki.$( FCKAutoGrow.Textarea ).height() ;
			var iDummyHeight = MindTouchDeki.$( FCKAutoGrow.Dummy ).height() ;

			if ( iTextareaHeight < iDummyHeight + FCKAutoGrow.LineHeight
				|| iDummyHeight < iTextareaHeight )
			{	
				var height = iDummyHeight + FCKAutoGrow.LineHeight + 50 ;
				
				if ( height < FCKAutoGrow.MinHeight )
					height = FCKAutoGrow.MinHeight ;
				
				FCKAutoGrow.Textarea.style.height = height + 'px' ;
				window.frameElement.style.height = (height + 5) + 'px' ;
			}
		}
		
		return ;
	},
	
	SetListeners : function()
	{
		if ( FCK.EditMode != FCK_EDITMODE_WYSIWYG )
		{
			if ( !FCKConfig.SourceAutoGrow )
				return ;
			
			FCKAutoGrow.Textarea = FCK.EditingArea.Textarea ;
			FCKAutoGrow.LineHeight = parseInt( MindTouchDeki.GetCurrentElementStyle( FCKAutoGrow.Textarea, 'line-height' ) ) ;
			
			if ( isNaN( FCKAutoGrow.LineHeight ) )
				FCKAutoGrow.LineHeight = 0 ;
			
			FCKAutoGrow.CheckSource() ;
			
			// see #0005686
			if ( !FCKBrowserInfo.IsIE || FCKConfig.IESourceAutoGrow )
				FCKAutoGrow.Interval = window.setInterval( FCKAutoGrow.CheckSource, 400 ) ;
		}
		else
		{
			if ( FCKAutoGrow.Interval )
			{
				clearInterval( FCKAutoGrow.Interval ) ;
			}

			if ( FCKAutoGrow.Dummy )
			{
				FCKDomTools.RemoveNode( FCKAutoGrow.Dummy ) ;
				FCKAutoGrow.Dummy = null ;
			}

			if ( FCKBrowserInfo.IsIE && FCKConfig.AutoGrow )
			{
				FCK.EditorWindow.attachEvent( 'onscroll', FCKAutoGrow.CheckWysiwyg ) ;
				FCK.EditorDocument.attachEvent( 'onkeyup', FCKAutoGrow.CheckWysiwyg ) ;
			}
		}
	},
	
	CheckEditorStatus : function( sender, status )
	{
		if ( status == FCK_STATUS_COMPLETE )
		{
			setTimeout( FCKAutoGrow.CheckWysiwyg, 0 ) ;
		}
	}
}

if ( FCKConfig.AutoGrow )
{
	FCK.AttachToOnSelectionChange( FCKAutoGrow.CheckWysiwyg ) ;
	FCK.Events.AttachEvent( 'OnStatusChange', FCKAutoGrow.CheckEditorStatus ) ;
}

FCK.Events.AttachEvent( 'OnAfterSetHTML', FCKAutoGrow.SetListeners ) ;
