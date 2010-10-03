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

/**
 * @file Autogrow plugin.
 */

(function()
{
	var interval = null,
		dummy = null,
		onResize,
		prevHeight = 0;
	
	function autoGrowWysiwyg( ev )
	{
		var editor = ev.editor;
		
		if ( editor.document && editor.getCommand( 'maximize' ).state != CKEDITOR.TRISTATE_ON )
		{
			var contents = editor.getThemeSpace( 'contents' );
			
			var containerHeight = contents.getStyle( 'height' );
			var contentsHeight;

			if ( CKEDITOR.env.ie )
			{
				// innerHeight returns incorrect value with HTML 4.0 Transitional doctype
				// @see #0007806
				contentsHeight = editor.document.getBody().$.scrollHeight;
			}
			else
			{
				contentsHeight = Deki.$( editor.document.getBody().$ ).innerHeight();
			}
			
			containerHeight = parseInt( containerHeight ) || Deki.$( contents.$ ).height();
			contentsHeight = contentsHeight || editor.config.height;
			
			if ( contentsHeight < editor.config.resize_minHeight )
				contentsHeight = editor.config.resize_minHeight;
			
			var delta = contentsHeight - containerHeight;
			
			if ( delta != 0 )
			{
				editor.resize( null, contentsHeight + 70, true );
			}
		}
	}
	
	function autoGrowSource( editor )
	{
		var ta = editor.textarea;
		
		if ( !ta || editor.getCommand( 'maximize' ).state == CKEDITOR.TRISTATE_ON )
			return;
		
		var height;
		
		if ( CKEDITOR.env.ie )
		{
			height = ta.$.scrollHeight;
		}
		else
		{
			var	paddingLeft = ta.getComputedStyle( 'padding-left' ),
				paddingRight = ta.getComputedStyle( 'padding-right' );
			
			if ( dummy === null )
			{
				dummy = CKEDITOR.document.createElement( 'pre' );
				dummy.setStyles(
					{
						'font-size'		: ta.getComputedStyle( 'font-size' ),
						'font-family'	: ta.getComputedStyle( 'font-family' ),
						'padding-top'	: ta.getComputedStyle( 'padding-top' ),
						'padding-right'	: paddingRight,
						'padding-bottom': ta.getComputedStyle( 'padding-bottom' ),
						'padding-left'	: paddingLeft,
						'line-height'	: ta.getComputedStyle( 'line-height' ),
						'letter-spacing': ta.getComputedStyle( 'letter-spacing' ),
						'overflow-x'	: 'hidden',
						'position'		: 'absolute',
						'top'			: 0,
						'left'			: '-9999px',
						'white-space'	: 'pre-wrap'
					});
				
				dummy.appendTo( ta.getDocument().getBody() );

				var paddingWidth = parseInt( paddingLeft ) + parseInt( paddingRight );
				paddingWidth = paddingWidth || 0;

				dummy.setStyle( 'width', ta.$.clientWidth - paddingWidth + 'px' );
			}
			
			var html = ta.getValue().replace( /(<|>)/g, '_' );
			html = html.replace( /&/g, '&amp;' );

			// let browser processes the html before comparison
			var fakeDiv = new CKEDITOR.dom.element( 'div', editor.document );
			fakeDiv.setHtml( html );
			
			if ( dummy.getHtml() != fakeDiv.getHtml() )
			{
				dummy.setHtml( html );
			
				var textareaHeight = Deki.$( ta.$ ).height();
				var dummyHeight = Deki.$( dummy.$ ).height();
				
				var lineHeight = ta.getComputedStyle( 'line-height' );
				lineHeight = parseInt( lineHeight ) || 0;
	
				if ( textareaHeight < dummyHeight + lineHeight || dummyHeight < textareaHeight )
				{	
					height = dummyHeight + lineHeight;
				}
			}
		}
		
		if ( !isNaN( height ) )
		{
			height += 50;
			
			if ( height < editor.config.resize_minHeight )
			{
				height = editor.config.resize_minHeight;
			}

			if ( height != prevHeight )
			{
				if ( CKEDITOR.env.ie )
				{
					ta.setStyle( 'height', height + 'px' );
				}

				editor.resize( null, height, true );
				
				prevHeight = height;
			}
		}
		
		return;
	}
	
	function autoGrow( evt )
	{
		var editor = evt.editor,
			win = CKEDITOR.document.getWindow();
		
		switch ( editor.mode )
		{
			case 'wysiwyg' :
				
				if ( interval )
				{
					window.clearInterval( interval );
					interval = null;
				}
				
				if ( dummy )
				{
					dummy.remove();
					dummy = null;
				}

				if ( typeof onResize == 'function' )
				{
					editor.removeListener( 'resize', onResize );
					win.removeListener( 'resize', onResize );
				}

				prevHeight = 0;
				
				editor.on( 'insertHtml', autoGrowWysiwyg, null, null, 200 );
				editor.on( 'insertElement', autoGrowWysiwyg, null, null, 200 );
				editor.on( 'paste', autoGrowWysiwyg, null, null, 2000 );
				editor.on( 'selectionChange', autoGrowWysiwyg );
				editor.on( 'contentDom', autoGrowWysiwyg );
				editor.on( 'key', autoGrowWysiwyg );
				break;
				
			case 'source' :
				
				if ( editor.config.autogrow_source )
				{
					if ( CKEDITOR.env.ie )
					{
						var textarea = editor.textarea;
						onResize = function( evt )
							{
								var holderElement = textarea && textarea.getParent();
								holderElement && textarea.setStyle( 'height', holderElement.$.clientHeight + 'px' );
								evt.cancel();
							};
						editor.on( 'resize', onResize, null, null, 1 );
						win.on( 'resize', onResize, null, null, 1 );
					}
					
					autoGrowSource( editor );
					
					interval = window.setInterval( function()
						{
							autoGrowSource( editor );
							
						}, 400 );
				}
				break;
		}
	}
	
	CKEDITOR.plugins.add( 'autogrow',
	{
		requires : [ 'floatingtoolbar' ],
		
		init : function( editor )
		{
			if ( editor.config.autogrow )
			{
				editor.on( 'mode', autoGrow );
				editor.on( 'contentDom', autoGrow );
			}
		}
	});
})();

/**
 * Whether to enable the autogrowing feature.
 * @type Boolean
 * @default true
 * @see config.floating_toolbar
 * @example
 * config.autogrow = false;
 */
CKEDITOR.config.autogrow = true;

/**
 * Whether to enable the autogrowing feature in source mode.
 * @type Boolean
 * @default true
 * @example
 * config.autogrow_source = false;
 */
CKEDITOR.config.autogrow_source = true;
