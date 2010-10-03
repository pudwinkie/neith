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
 * @file MindTouch plugin.
 */

(function()
{
	var disable = false;
	
	var onSelectionChange = function( evt )
	{
		var elements = evt.data.path.elements,
			editor = evt.editor;
		
		if ( editor.mode != 'wysiwyg' )
			return false;
		
		disable = false;
		
		for ( var i = 0 ; i < elements.length ; i++ )
		{
			if ( elements[i].is( 'h1' ) && editor.document.getBody().getFirst().equals( elements[i] ) )
			{
				disable = true;
				// we'll disable commands last
				return false;
			}
		}
		
		for ( var name in editor._.commands )
		{
			if ( CKEDITOR.tools.indexOf( editor.config.header_ignoreCommands, name ) == -1 )
			{
				editor._.commands[ name ].enable();
			}
		}
		
		return true;
	};
	
	var disableCommands = function( evt )
	{
		var editor = evt.editor;
		
		if ( !disable )
			return false;
		
		for ( var name in editor._.commands )
		{
			if ( CKEDITOR.tools.indexOf( editor.config.header_ignoreCommands, name ) == -1 )
			{
				editor._.commands[ name ].disable();
			}
		}
		
		return true;
	};

	var addButtonCommand = function( editor, name, buttonDefinition, styleDefiniton )
	{
		var command = buttonDefinition.command;

		if ( command )
		{
			var style = new CKEDITOR.style( styleDefiniton );

			editor.attachStyleStateChange( style, function( state )
				{
					editor.getCommand( command ).setState( state );
				});

			editor.addCommand( command, new CKEDITOR.styleCommand( style ) );
		}

		editor.ui.addButton( name, buttonDefinition );
	};
	
	CKEDITOR.plugins.add( 'mindtouch',
	{
		lang : [ 'en', 'cs', 'de', 'en-au', 'es', 'et', 'fr-ca', 'fr', 'he', 'hu', 'it', 'ja', 'ko', 'nl', 'pt-br', 'ru', 'sv' ],

		requires : [ 'button', 'format', 'selection', 'styles' ],

		beforeInit : function( editor )
		{
			editor.lang.pagebreak = editor.lang.fakeobjects.div;

			var path = this.path;

			addButtonCommand( editor, 'Comment',
				{
					label : editor.lang.mindtouch.comment,
					command : 'comment',
					icon : path + 'images/comment.png'
				}, editor.config.mindtouchStyles_comment );

			addButtonCommand( editor, 'DekiScript',
				{
					label : editor.lang.mindtouch.dekiscript,
					command : 'dekiscript',
					icon : path + 'images/script.png'
				}, editor.config.mindtouchStyles_dekiscript );

			addButtonCommand( editor, 'JEM',
				{
					label : editor.lang.mindtouch.jem,
					command : 'jem',
					icon : path + 'images/script-jem.png'
				}, editor.config.mindtouchStyles_jem );

			addButtonCommand( editor, 'CSS',
				{
					label : editor.lang.mindtouch.css,
					command : 'css',
					icon : path + 'images/script-css.png'
				}, editor.config.mindtouchStyles_css );
		},
		
		init : function( editor )
		{
			// Add custom css to panel
			editor.skin.editor.css.push( CKEDITOR.basePath + '../editor/skin.css' );

			// First of all enable commands or set disabled flag to true
			// to delayed disabling.
			// We should subscribe to this event with -1 priority
			// to let other plug-ins to implement their own logic.
			editor.on( 'selectionChange', onSelectionChange, null, null, -1 );
			
			// delayed disabling - disable command if it is necessary
			// after all listeners will be called
			editor.on( 'selectionChange', disableCommands, null, null, 100 );

			editor.lang.format.tag_h6 = editor.lang.format.tag_h5;
			editor.lang.format.tag_h5 = editor.lang.format.tag_h4;
			editor.lang.format.tag_h4 = editor.lang.format.tag_h3;
			editor.lang.format.tag_h3 = editor.lang.format.tag_h2;
			editor.lang.format.tag_h2 = editor.lang.format.tag_h1;
			CKEDITOR.tools.extend( editor.lang.format, editor.lang.mindtouch.format, true );

			var path = this.path;
			var uiMenuItems = {};

			var menuGroup = 'formatButton';
			editor.addMenuGroup( menuGroup );

			uiMenuItems.h1 =
				{
					label : editor.lang.format.tag_h2,
					command : 'h1',
					icon : path + 'images/text_heading_1.png',
					group : menuGroup
				};

			uiMenuItems.h2 =
				{
					label : editor.lang.format.tag_h3,
					command : 'h2',
					icon : path + 'images/text_heading_2.png',
					group : menuGroup
				};

			uiMenuItems.h3 =
				{
					label : editor.lang.format.tag_h4,
					command : 'h3',
					icon : path + 'images/text_heading_3.png',
					group : menuGroup
				};

			uiMenuItems.h4 =
				{
					label : editor.lang.format.tag_h5,
					command : 'h4',
					icon : path + 'images/text_heading_4.png',
					group : menuGroup
				};

			uiMenuItems.h5 =
				{
					label : editor.lang.format.tag_h6,
					command : 'h5',
					icon : path + 'images/text_heading_5.png',
					group : menuGroup
				};

			editor.addMenuItems( uiMenuItems );

			editor.ui.add( 'Hx', CKEDITOR.UI_MENUBUTTON,
				{
					label : editor.lang.mindtouch.format.tag_hx,
					title : editor.lang.mindtouch.format.tag_hx,
					className : 'cke_button_hx',
					icon : path + 'images/text_heading_x.png',
					onMenu : function()
					{
						var menuItems = {};

						for ( var i = 0 ; i < editor.config.menu_hxItems.length ; i++ )
						{
							var itemName = editor.config.menu_hxItems[ i ].toLowerCase();

							var state = CKEDITOR.TRISTATE_DISABLED,
								commandName = uiMenuItems[ itemName ].command,
								command = commandName && editor.getCommand( commandName );

							if ( command )
							{
								state = command.state;
							}

							menuItems[ itemName ] = state;
						}

						return menuItems;
					},
					onRender : function()
					{
						var me = this;

						editor.on( 'selectionChange', function()
							{
								var state = CKEDITOR.TRISTATE_OFF;
								
								for ( var i = 0 ; i < editor.config.menu_hxItems.length ; i++ )
								{
									var itemName = editor.config.menu_hxItems[ i ].toLowerCase();
									var command = editor.getCommand( uiMenuItems[ itemName ].command );

									if ( command && command.state == CKEDITOR.TRISTATE_ON )
									{
										state = CKEDITOR.TRISTATE_ON;
									}
								}

								me.setState( state );
							} );
					}
				});

			var nTag = editor.config.enterMode == CKEDITOR.ENTER_DIV ? 'div' : 'p';
			addButtonCommand( editor, 'Normal',
				{
					label : editor.lang.format[ 'tag_' + nTag ],
					command : 'normal'
				}, editor.config[ 'format_' + nTag ] );

			addButtonCommand( editor, 'Code',
				{
					label : editor.lang.mindtouch.code,
					command : 'code',
					icon : path + 'images/style_code.png'
				}, editor.config.mindtouchStyles_code );

			addButtonCommand( editor, 'H1', uiMenuItems.h1, editor.config[ 'format_h2' ] );
			addButtonCommand( editor, 'H2', uiMenuItems.h2, editor.config[ 'format_h3' ] );
			addButtonCommand( editor, 'H3', uiMenuItems.h3, editor.config[ 'format_h4' ] );
			addButtonCommand( editor, 'H4', uiMenuItems.h4, editor.config[ 'format_h5' ] );
			addButtonCommand( editor, 'H5', uiMenuItems.h5, editor.config[ 'format_h6' ] );

			/*
			 * Paste processing
			 * @see #0008224
			 * @see #0007885
			 */
			editor.on( 'paste', function( evt )
				{
					if ( evt.data[ 'html' ] )
					{
						var data = evt.data[ 'html' ];

						// remove styles
						if ( editor.config.paste_removeStyles || ( CKEDITOR.env.webkit && editor.config.paste_removeStylesWebkit ) )
						{
							var div = new CKEDITOR.dom.element( 'div', editor.document );
							div.setHtml( data );

							var node = div.getNextSourceNode( false, CKEDITOR.NODE_ELEMENT );
							while ( node )
							{
								node.removeAttribute( 'style' );
								node = node.getNextSourceNode( false, CKEDITOR.NODE_ELEMENT );
							}

							evt.data[ 'html' ] = div.getHtml();
						}
					}
				} );

			editor.on( 'insertHtml', function( evt )
				{
					var editor = evt.editor;

					if ( editor.mode == 'wysiwyg' && CKEDITOR.env.webkit )
					{
						var data = evt.data;

						// if we are pasting block content into the empty block
						// we can get the following structure
						// <p><div><p>pasted text</p></div></p>
						// to aviod this we need paste the content before the block
						if ( /<(p|div|h[1-6]|ul|ol)/.test( data ) )
						{
							editor.focus();

							var selection = editor.getSelection(),
								range = selection && selection.getRanges()[0];

							if ( range.collapsed &&
								 range.startContainer.is &&
								 range.startContainer.is( 'p', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6' ) )
							{
								var first = range.startContainer.getFirst();

								if ( !first || ( !first.getNext() && ( ( first.is && first.is( 'br' ) ) || ( first.type == CKEDITOR.NODE_TEXT && first.getText().length == 0 ) ) ) )
								{
									editor.fire( 'saveSnapshot' );

									if ( editor.dataProcessor )
										data = editor.dataProcessor.toHtml( data );

									var div = new CKEDITOR.dom.element( 'div', editor.document );
									div.setHtml( data );

									var children = div.getChildren();
									while ( children.count() )
									{
										children.getItem( 0 ).insertBefore( range.startContainer );
									}

									CKEDITOR.tools.setTimeout( function()
										{
											editor.fire( 'saveSnapshot' );
										}, 0 );

									evt.cancel();
								}
							}
						}
					}
				} );
		}
	});
})();

/**
 * The list of commands which should not be disabled when cursor is in a header.
 * @type Array
 * @default [
		'mindtouchsave', 'mindtouchcancel', 'source',
		'undo', 'redo', 'find', 'replace', 'removeFormat',
		'maximize', 'about', 'blur', 'blurBack', 'checkspell',
		'copy', 'cut', 'paste', 'newpage', 'pagebreak', 'preview',
		'print', 'scaytcheck', 'selectAll', 'shiftTab', 'showblocks',
		'tab', 'toolbarCollapse', 'toolbarFocus'
	]
 * @example
 */
CKEDITOR.config.header_ignoreCommands =
	[
		'mindtouchsave', 'mindtouchcancel', 'source',
		'undo', 'redo', 'find', 'replace', 'removeFormat',
		'maximize', 'about', 'blur', 'blurBack', 'checkspell',
		'copy', 'cut', 'paste', 'newpage', 'pagebreak', 'preview',
		'print', 'scaytcheck', 'selectAll', 'shiftTab', 'showblocks',
		'tab', 'toolbarCollapse', 'toolbarFocus', 'atd'
	];

/**
 * The style definition to be used to apply the code style in the text.
 * @type Object
 * @default { element : 'code' }
 * @example
 * config.mindtouchStyles_code = { element : 'code', attributes : { 'class': 'Code' } };
 */
CKEDITOR.config.mindtouchStyles_code = { element : 'code' };

CKEDITOR.config.mindtouchStyles_comment = { element : 'p', attributes : {'class' : 'comment'} };
CKEDITOR.config.mindtouchStyles_dekiscript = { element : 'pre', attributes : { 'class' : 'script' } };
CKEDITOR.config.mindtouchStyles_jem = { element : 'pre', attributes : { 'class' : 'script-jem' } };
CKEDITOR.config.mindtouchStyles_css = { element : 'pre', attributes : { 'class' : 'script-css' } };

CKEDITOR.config.menu_hxItems =
	[
		'H4', 'H5'
	];

/**
 * Remove style attribute on pasting.
 * @type Boolean
 * @default false
 * @example
 * config.paste_removeStyles = true;
 */
CKEDITOR.config.paste_removeStyles = false;

/**
 * Remove style attribute on pasting in Webkit browsers.
 * @type Boolean
 * @default true
 * @example
 * config.paste_removeStylesWebkit = false;
 */
CKEDITOR.config.paste_removeStylesWebkit = true;
