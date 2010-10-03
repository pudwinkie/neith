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
 * @file DekiScript Bar plugin.
 */

(function()
{
	function calculateCursorPosition( editor )
	{
		var sel = editor.getSelection(),
			firstElement = sel && sel.getStartElement(),
			path = firstElement && new CKEDITOR.dom.elementPath( firstElement );

		if ( !path || !path.block || !path.block.is( 'pre' ) )
		{
			dsbarInst.hide();
			return;
		}

		var range = sel.getRanges()[0],
			node = range && range.startContainer,
			stat = { lines: 1, chars: 0 };

		var processNode = function( node )
		{
			var text;

			if ( node.type == CKEDITOR.NODE_TEXT )
			{
				text = node.getText();

				// Gecko returns text node within of the one line
				if ( !CKEDITOR.env.gecko )
				{
					var splitChar;
					var re;
					
					if ( CKEDITOR.env.ie )
					{
						splitChar = "\r";
						re = /(^\r)|(\r$)/;
					}
					
					if ( CKEDITOR.env.webkit )
					{
						splitChar = "\n";
						re = /(^\n)|(\n$)/;
					}
					
					if ( CKEDITOR.env.opera )
					{
						splitChar = "\r\n";
						re = /(^\r\n)|(\r\n$)/;
					}

					var lines = text.split( splitChar );

					switch ( lines.length )
					{
						// there are no line breaks
						case 1:
							if ( stat.lines < 2 )
							{
								stat.chars += text.length;
							}
							break;
						case 2:
							if ( re.test( text ) )
							{
								if ( stat.lines < 2 )
								{
									stat.chars += lines[ lines.length - 1 ].length;
								}

								stat.lines++;
								break;
							}
							// break;
						default:
							if ( stat.lines < 2 )
							{
								stat.chars += lines[ lines.length - 1 ].length;
							}
							stat.lines += lines.length - 1;
							break;
					}

					return;
				}

				if ( stat.lines < 2 )
				{
					stat.chars += text.length;
				}
			}
			else if ( node.type == CKEDITOR.NODE_ELEMENT )
			{
				// FF
				if ( node.is( 'br' ) )
				{
					stat.lines++;
				}
				else
				{
					if ( stat.lines < 2 )
					{
						stat.chars += node.getText().length;
					}
				}
			}
		};

		if ( node && node.is && node.is( 'pre' ) && range.collapsed )
		{
			var dummySpan = editor.document.createElement( 'span' );
			dummySpan.setHtml( '&#65279;' );

			range.insertNode( dummySpan );
			node = dummySpan.getPreviousSourceNode( true, null, range.startContainer );

			node && processNode( node );

			dummySpan.remove();
		}
		else if ( node && node.type == CKEDITOR.NODE_TEXT )
		{
			text = node.getText().substring( 0, range.startOffset );
			var textNode = new CKEDITOR.dom.text( text );

			processNode( textNode );
		}
		else if ( node )
		{
			processNode( node );
		}

		if ( node )
		{
			while ( node = node.getPreviousSourceNode( true, null, path.block ) )  // only one =
			{
				processNode( node );
			}
		}

		updateLabels( editor, stat.lines, stat.chars );
		dsbarInst.show();
	}

	function calculateCursorPositionTimeout( evt )
	{
		var editor = evt.editor || evt.listenerData;
		CKEDITOR.tools.setTimeout( calculateCursorPosition, 0, this, editor );
	}

	function updateLabels( editor, line, col )
	{
		dsbarInst.updateLabel( 'line', editor.lang.dsbar.line + ':&nbsp;' + line + ',&nbsp;' );
		dsbarInst.updateLabel( 'col', editor.lang.dsbar.col + ':&nbsp;' + col );
	}

	var labelCmd =
	{
		canUndo : false,
		startDisabled : true,
		exec : function( editor, labelText )
		{
			var label = this.uiItems[ 0 ];
			label.setLabel( labelText );
		}
	};

	var dsbarInst;

	var dsbar = function( id )
	{
		this._ =
		{
			id : id,
			bar : CKEDITOR.document.getById( id ),
			labels : []
		};

		this._.bar.setStyles(
			{
				'display' : 'none'
			}
		);
	};

	dsbar.prototype =
	{
		getBar : function()
		{
			return this._.bar;
		},

		addLabel : function( name )
		{
			var span = new CKEDITOR.dom.element( 'span', CKEDITOR.document );

			this._.bar.append( span );

			this._.labels[ name ] = span;
		},

		updateLabel : function( name, label )
		{
			this._.labels[ name ].setHtml( label );
		},

		show : function()
		{
			this._.bar.setStyle( 'display', 'block' );
		},

		hide : function()
		{
			this._.bar.setStyle( 'display', 'none' );
		}
	};

	CKEDITOR.plugins.add( 'dsbar',
	{
		requires : [ 'selection' ],

		lang : [ 'en', 'cs', 'de', 'en-au', 'es', 'et', 'fr-ca', 'fr', 'he', 'hu', 'it', 'ja', 'ko', 'nl', 'pt-br', 'ru', 'sv' ],

		init : function( editor )
		{
			editor.on( 'contentDom', function()
				{
					editor.document.on( 'mouseup', calculateCursorPositionTimeout, this, editor );
					editor.document.on( 'keyup', calculateCursorPositionTimeout, this, editor );
				} );

			editor.on( 'mode', calculateCursorPositionTimeout );
			editor.on( 'selectionChange', calculateCursorPositionTimeout );

			var dsbarId = 'cke_' + CKEDITOR.tools.getNextNumber();

			editor.on( 'themeSpace', function( event )
				{
					if ( event.data.space == 'top' )
					{
						var output = '<span class="dsbar" role="presentation" id="' + dsbarId + '"></span>';
						event.data.html += output;
					}
				} );

			editor.on( 'themeLoaded', function()
				{
					dsbarInst = new dsbar( dsbarId );
					dsbarInst.addLabel( 'line' );
					dsbarInst.addLabel( 'col' );
				} );
		},

		afterInit : function( editor )
		{
			var dataProcessor = editor.dataProcessor,
				dataFilter = dataProcessor && dataProcessor.dataFilter;

			if ( CKEDITOR.env.gecko && dataFilter )
			{
				dataFilter.addRules(
					{
						elements :
						{
							'pre' : function( pre )
							{
								// Gecko prefers <br> as line-break inside <pre>
								var br = new CKEDITOR.dom.element( 'br', editor.document );

								var replaceLineBreaks = function( element )
								{
									if ( !element.children )
									{
										return;
									}

									for ( var i = 0 ; i < element.children.length ; i++ )
									{
										var child = element.children[ i ];

										if ( child.type == CKEDITOR.NODE_TEXT )
										{
											child.value = child.value.replace( /\n/g, br.getOuterHtml() );
										}
										else
										{
											replaceLineBreaks( child );
										}
									}
								};

								replaceLineBreaks( pre );
							}
						}
					} );
			}
		}
	});
})();
