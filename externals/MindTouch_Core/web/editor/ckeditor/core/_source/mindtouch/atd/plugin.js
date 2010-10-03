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
 * @file After the Deadline plug-in.
 */

(function()
{
	var pluginPath,
		foundErrors = {},
		ignoreStrings = {},
		ignoreTypes = {};

	function guardDomWalkerNonEmptyTextNode( node )
	{
		return ( node.type == CKEDITOR.NODE_TEXT && node.getLength() > 0 );
	}
	
	function isMarkedNode( node )
	{
		return node &&
			node.getName &&
			node.getName() == 'span' &&
			node.hasAttribute( 'atd_error_id' );
	}

	var ErrorsIterator = function()
	{
		this.positions = [];
		this.errors = [];
	};
	
	ErrorsIterator.prototype =
	{
		add : function( pos, $error )
		{
			if ( !this.errors[ pos ] )
			{
				this.positions.push( pos );
				this.errors[ pos ] = $error;
			}
		},

		hasPos : function( pos )
		{
			return ( typeof this.errors[ pos ] == 'object' );
		},

		count : function()
		{
			return this.positions.length; // don't use errors array
		},

		each : function( callback, scope )
		{
			if ( typeof callback != 'function' )
				return;

			scope = scope || this;

			// why the sort function is necessary?
			var positions = this.positions.sort( function( a, b ) {return a - b} );

			for ( var i = 0 ; i < positions.length ; i++ )
			{
				var $error = this.errors[ positions[i] ];
				callback.apply( scope, [ positions[i], $error ] );
			}
		}
	};

	var Cookie =
	{
		getHash : function( name )
		{
			var val = this.get( name ),
				hash = {};

			if ( val )
			{
				jQuery.each( val.split( '&' ), function( index, val )
					{
						val = val.split( '=' );
						hash[ unescape( val[0] ) ] = unescape( val[1] );
					} );
			}

			return hash;
		},

		setHash : function( name, value, days )
		{
			var serialized = '';

			jQuery.each( value, function( key, val )
				{
					serialized += ( !serialized ? '' : '&' ) + escape( key ) + '=' + escape( val );
				} );

			this.set( name, serialized, days );
		},

		set : function( name, value, days )
		{
			var expires = '';

			if ( days )
			{
				var date = new Date();
				date.setTime( date.getTime() + ( days * 24 * 60 * 60 * 1000 ) );
				expires = '; expires=' + date.toGMTString();
			}
			document.cookie = name + '=' + escape( value ) + expires + '; path=/';
		},

		get : function( name )
		{
			var nameEQ = name + '=';
			var ca = document.cookie.split( ';' );

			for( var i = 0 ; i < ca.length ; i++)
			{
				var c = ca[ i ];
				while ( c.charAt( 0 ) == ' ' )
				{
					c = c.substring( 1, c.length );
				}
				if ( c.indexOf( nameEQ ) == 0 )
				{
					return unescape( c.substring( nameEQ.length, c.length ) );
				}
			}
			return null;
		},

		remove : function( name )
		{
			Cookie.set( name, '', -1 );
		}
	};

	/**
	 * Returns regex to match word separators
	 *
	 * from AtD Core
	 *
	 * @param exclude boolean If true regexp will match all characters exclude separators
	 * @return string Regex
	 */
	var getSeparators = function( exclude )
	{
		var re = '', i;
		var str = '"\'s!#$%&()*+,./:;<=>?@[\]^_{|}';

		// Build word separator regexp
		for ( i = 0 ; i < str.length ; i++ )
		{
			re += '\\' + str.charAt( i );
		}

		if ( exclude )
		{
			return "(?:[^\xa0" + re  + "])+";
		}
		else
		{
			return "(?:(?:[\xa0" + re  + "])|(?:\\-\\-))+";
		}
	};

	var htmlFilterRule =
		{
			elements :
				{
					span : function( element )
					{
						if ( element.attributes['atd_error_id'] )
						{
							delete element.name;	// Write children, but don't write this node.
							return element;
						}
					}
				}
		};

	var atdCmd =
	{
		canUndo: false,
		editorFocus : true,

		exec : function( editor )
		{
			this.editor = editor;

			var me = this;

			var proxyUrl = editor.config.atd_rpc_url || pluginPath + 'proxy.php';
			var apiKey = editor.config.atd_api_key || '';

			if ( editor.lock )
			{
				editor.lock();
			}

			/* post the editor contents to the AtD service */
			jQuery.ajax(
				{
					url : proxyUrl + '?url=/checkDocument',
					data :
						{
							data : editor.getData(),
							key : apiKey
						},
					type : 'POST',
					dataType : 'xml',
					success : function( data, status )
					{
						if ( status == 'success' )
						{
							me._processErrors.call( me, data );
						}
						else
						{
							alert( editor.lang.atd.serverError );
						}
					},
					error : function( xhr, textStatus, errorThrown )
					{
						alert( editor.lang.atd.serverError );
					},
					complete : function()
					{
						if ( editor.unlock )
						{
							editor.unlock();
						}
					}
				} );
		},

		_processErrors : function( resultDoc )
		{
			var me = this,
				editor = this.editor,
				$result = jQuery( resultDoc );

			var $message = $result.children( 'message' );
			if ( $message.length )
			{
				alert( $message.text() );
				return;
			}

			// clear previous errors
			this._clearErrors();
			foundErrors = {};

			var $errors = $result.find( 'results > error' );

			if ( $errors.length == 0 )
			{
				alert( editor.lang.atd.noErrors );
				return;
			}

			// attach event for spell checker menu
			var menuHandler = function( ev )
				{
					var target = ev.data.getTarget();

					if ( isMarkedNode( target ) )
					{
						me._showMenu.call( me, target );
					}
				};

			editor.document.removeListener( 'click', menuHandler );
			editor.document.on( 'click', menuHandler );

			var range, walker;

			range = new CKEDITOR.dom.range( editor.document );
			range.setStartBefore( editor.document.getBody() );
			range.setEndAfter( editor.document.getBody() );

			walker = new CKEDITOR.dom.walker( range.clone() );
			walker.evaluator = guardDomWalkerNonEmptyTextNode;

			var textNode, prevTextNode;

			// union text nodes with the same parent
			// some elements contain a few text nodes produced after dom manipulation
			// but in fact this is one text node
			while ( ( textNode = walker.next() ) )
			{
				if ( prevTextNode && prevTextNode.getParent().equals( textNode.getParent() ) &&
					textNode.getPrevious().equals( prevTextNode ) )
				{
					var text = prevTextNode.getText() + textNode.getText(),
						newTextNode = new CKEDITOR.dom.text( text, editor.document );

					newTextNode.insertAfter( textNode );

					prevTextNode.remove();
					textNode.remove();

					textNode = newTextNode;

					var r = new CKEDITOR.dom.range( editor.document );
					r.moveToPosition( textNode, CKEDITOR.POSITION_AFTER_END );
					r.setEndAfter( editor.document.getBody() );

					walker.range = r;
					walker.reset();
				}

				prevTextNode = textNode;
			}

			walker = new CKEDITOR.dom.walker( range.clone() );
			walker.evaluator = guardDomWalkerNonEmptyTextNode;

			// walk on all text nodes
			while ( ( textNode = walker.next() ) )
			{
				var errors = new ErrorsIterator(),
					text = textNode.getText();

				// collect errors for text node
				$errors.each( function()
				{
					var $error = jQuery( this ),
						errorText = $error.children( 'string' ).text(),
						errorDescription = $error.children( 'description' ).text();

					if ( ignoreStrings[ errorText ] || ignoreTypes[ errorDescription ] )
					{
						return;
					}

					var pos = text.indexOf( errorText );

					// if text node contains a few equal errors
					// or precontext does not match
					while ( pos > -1 && ( errors.hasPos( pos ) || !me._checkErrorPrecontext( $error, text, pos ) ) )
					{
						pos = text.indexOf( errorText, pos + 1 );
					}

					// if error text was found and context matchs
					// save the error
					if ( pos > -1 )
					{
						errors.add( pos, $error );
					}
				} );

				// process next text node if this one does not contain errors
				if ( errors.count() == 0 )
					continue;

				var newNodes = [],
					postText = text,
					posOffset = 0;

				errors.each( function( pos, $error )
					{
						pos = pos - posOffset;

						var preText = postText.substring( 0, pos ),
							errorText = $error.children( 'string' ).text();

						var errorNode = me._createErrorNode( $error );

						newNodes.push( new CKEDITOR.dom.text( preText, editor.document ) );
						newNodes.push( errorNode );

						postText = postText.substring( pos + errorText.length );
						posOffset = pos + posOffset + errorText.length;
					} );

				if ( postText.length )
					newNodes.push( new CKEDITOR.dom.text( postText, editor.document ) );

				var r = new CKEDITOR.dom.range( editor.document );
				r.selectNodeContents( textNode );
				r.deleteContents();

				for ( var i = 0 ; i < newNodes.length ; i++ )
				{
					r.insertNode( newNodes[i] );
					r.moveToPosition( newNodes[i], CKEDITOR.POSITION_AFTER_END );
				}
				
				r.setEndAfter( editor.document.getBody() );

				walker.range = r;
				walker.reset();
			}

			editor.fire( 'scrollToTop' );
		},

		_createErrorNode : function( $error )
		{
			var editor = this.editor;

			var errorText = $error.children( 'string' ).text(),
				errorType = $error.children( 'type' ).text().toLowerCase();

			var errorElement = new CKEDITOR.dom.element( 'span', editor.document );
			errorElement.setText( errorText );

			var errorStyle = new CKEDITOR.style( editor.config.atd_styles[ errorType ] );
			errorStyle.applyToObject( errorElement );

			var id = CKEDITOR.tools.getNextNumber();
			errorElement.setAttribute( 'atd_error_id', id );
			foundErrors[ id ] = $error;

			return errorElement;
		},

		/**
		 * Checks the precontext of found error
		 * @param $error jQuery object - error node
		 * @param text String - text contains error
		 * @param errorPos Number - position of the error
		 * @return Boolean - true if precontext matchs or is empty, false otherwise
		 */
		_checkErrorPrecontext : function( $error, text, errorPos )
		{
			var preContext = $error.children( 'precontext' ).text();
			var preText = text.substring( 0, errorPos );
			var re, match;
			var result = false;

			if ( preContext.length )
			{
				re = new RegExp( '(?:^|' + getSeparators() + ')(' + getSeparators( true ) + ')' + getSeparators() + '$' );
				match = re.exec( preText );

				if ( match && match[1] == preContext )
				{
					result = true;
				}
			}
			else
			{
				re = new RegExp( '(?:^|.*?' + getSeparators() + ')' + $error.children( 'string' ).text() + '(?:' + getSeparators() + '|$)' );
				match = re.exec( text );

				if ( match )
				{
					result = true;
				}
			}

			return result;
		},

		_showMenu : function( errorNode )
		{
			var editor = this.editor,
				me = this,
				menu = new CKEDITOR.menu( editor ),
				menuItem;

			editor.addMenuGroup( 'atdTitle', 100 );
			editor.addMenuGroup( 'atdSuggestion', 200 );
			editor.addMenuGroup( 'atdExplain', 300 );
			editor.addMenuGroup( 'atdIgnore', 400 );

			var $error = foundErrors[ errorNode.getAttribute( 'atd_error_id' ) ];

			if ( !$error )
			{
				return;
			}
			
			var range = new CKEDITOR.dom.range( editor.document );
			range.selectNodeContents( errorNode );
			range.select();

			menuItem = new CKEDITOR.menuTitle( editor, 'atd_title',
				{
					title : $error.children( 'description' ).text(),
					group : 'atdTitle'
				} );
			menu.add( menuItem );

			$error.children( 'suggestions' ).children( 'option' ).each( function()
				{
					var $this = jQuery( this );

					menuItem = new CKEDITOR.menuItem( editor, 'atd_suggestion',
						{
							label : $this.text(),
							group : 'atdSuggestion',
							suggestion : $this.text()
						});

					menu.add( menuItem );
				} );

			var $url = $error.children( 'url' );
			if ( $url.length )
			{
				menuItem = new CKEDITOR.menuItem( editor, 'atd_explain',
					{
						label : editor.lang.atd.explain,
						group : 'atdExplain'
					});
				menu.add( menuItem );
			}

			menuItem = new CKEDITOR.menuItem( editor, 'atd_ignore',
				{
					label : editor.lang.atd.ignoreSuggestion,
					group : 'atdIgnore'
				});
			menu.add( menuItem );

			if ( editor.config.atd_ignore_enable )
			{
				menuItem = new CKEDITOR.menuItem( editor, 'atd_ignoreAlways',
					{
						label : editor.lang.atd.ignoreAlways,
						group : 'atdIgnore'
					});
			}
			else
			{
				menuItem = new CKEDITOR.menuItem( editor, 'atd_ignoreAll',
					{
						label : editor.lang.atd.ignoreAll,
						group : 'atdIgnore'
					});
			}

			menu.add( menuItem );

			menu.onClick = function( item )
			{
				menu.hide();
				editor.focus();

				var range = new CKEDITOR.dom.range( editor.document );

				var moveCursorToNodeEnd = function( node )
				{
					if ( node )
					{
						range.selectNodeContents( node );
						range.collapse();
						range.select();
					}
				};

				switch ( item.name )
				{
					case 'atd_explain':
						window.open( $url.text() + '&theme=' + editor.config.atd_theme, 'atdExplain', 'location=1,toolbar=0,menubar=0,directories=0,resizable=1,status=0,scrollbars=1,width=480,height=380' );
						break;
					case 'atd_ignore':
						var first = errorNode.getFirst();
						errorNode.remove( true );
						moveCursorToNodeEnd( first );
						break;
					case 'atd_ignoreAll':
					case 'atd_ignoreAlways':
						var first = errorNode.getFirst(),
							error = errorNode.getText();
						
						me._clearErrors( error );
						moveCursorToNodeEnd( first );

						// store the error
						if ( item.name == 'atd_ignoreAlways' )
						{
							var url = editor.config.atd_ignore_rpc_url || '';

							if ( url.length )
							{
								var apiKey = editor.config.atd_api_key || '';

								jQuery.ajax(
									{
										url : url + encodeURI( error ).replace( /&/g, '%26' ) + '&key=' + apiKey,
										type : 'GET',
										dataType : 'xml',
										success : function( data, status )
										{
											if ( status != 'success' )
											{
												alert( "Ignore preference save failed\n" + status + "\nAt: " + url );
											}
										},
										error : function( xhr, textStatus, errorThrown )
										{
											alert( "Ignore preference save failed\n" + textStatus + "\nAt: " + url );
										}
									} );
							}
							else
							{
								var ignore = Cookie.getHash( 'atd_ignore' ) || {};
								ignore[ error ] = 1;

								Cookie.setHash( 'atd_ignore', ignore, 365 );
							}

							ignoreStrings[ error ] = 1;
						}

						break;
					case 'atd_suggestion':
						editor.fire( 'saveSnapshot' );

						var errorTextNode = new CKEDITOR.dom.text( item.suggestion, editor.document );
						errorTextNode.replace( errorNode );

						moveCursorToNodeEnd( errorTextNode );
						
						editor.fire( 'saveSnapshot' );
						break;
				}
			};

			menu.show( errorNode, 4 );
		},

		_clearErrors : function( error )
		{
			var nodeList = this.editor.document.getElementsByTag( 'span' ),
				count = nodeList.count(),
				i, node;

			for ( i = count - 1 ; i >= 0 ; i-- )
			{
				node = nodeList.getItem( i );
				if ( isMarkedNode( node ) )
				{
					if ( !error || error == node.getText() )
					{
						node.remove( true );
					}
				}
			}
		}
	};

	CKEDITOR.menuTitle = CKEDITOR.tools.createClass(
	{
		$ : function( editor, name, definition )
		{
			CKEDITOR.tools.extend( this, definition );

			// Transform the group name into its order number.
			this.group = editor._.menuGroups[ this.group ];

			this.editor = editor;
			this.name = name;
		},

		proto :
		{
			render : function( menu, index, output )
			{
				var id = menu.id + String( index ),
					state = ( typeof this.state == 'undefined' ) ? CKEDITOR.TRISTATE_OFF : this.state;;

				output.push(
					'<span class="cke_menuitem">' +
					'<span id="', id, '"' +
						' class=cke_menu_title"', '" href="javascript:void(\'', ( this.title || '' ).replace( "'", '' ), '\')"' +
						' title="', this.title, '"' +
						' tabindex="-1"' +
						'_cke_focus=1' +
						' hidefocus="true"' +
						' role="menuitem"' +
						( state == CKEDITOR.TRISTATE_DISABLED ? 'aria-disabled="true"' : '' ) +
						( state == CKEDITOR.TRISTATE_ON ? 'aria-pressed="true"' : '' ) );

				// Some browsers don't cancel key events in the keydown but in the
				// keypress.
				// TODO: Check if really needed for Gecko+Mac.
				if ( CKEDITOR.env.opera || ( CKEDITOR.env.gecko && CKEDITOR.env.mac ) )
				{
					output.push(
						' onkeypress="return false;"' );
				}

				// With Firefox, we need to force the button to redraw, otherwise it
				// will remain in the focus state.
				if ( CKEDITOR.env.gecko )
				{
					output.push(
						' onblur="this.style.cssText = this.style.cssText;"' );
				}

				output.push(
						'>' +
						'<span class="cke_label" style="margin-left:0; text-align:center; font-weight:bold;">' );

				if ( this.getItems )
				{
					output.push(
								'<span class="cke_menuarrow"></span>' );
				}

				output.push(
							this.title,
							'</span>' +
					'</span>' +
					'</span>' );
			}
		}
	});

	CKEDITOR.plugins.add( 'atd',
	{
		lang : [ 'en', 'cs', 'de', 'en-au', 'es', 'et', 'fr-ca', 'fr', 'he', 'hu', 'it', 'ja', 'ko', 'nl', 'pt-br', 'ru', 'sv' ],

		beforeInit : function()
		{
			if ( typeof jQuery == undefined )
			{
				CKEDITOR.scriptLoader.load( 'http://code.jquery.com/jquery-latest.js' );
			}
		},

		init : function( editor )
		{
			pluginPath = this.path;

			// Register the command.
			editor.addCommand( 'atd', atdCmd );

			// Register the toolbar button.
			editor.ui.addButton( 'AtD',
				{
					label : editor.lang.atd.toolbar,
					command : 'atd',
					icon : this.path + 'images/atdbuttontr.gif'
				});

			var dataProcessor = editor.dataProcessor,
				htmlFilter = dataProcessor && dataProcessor.htmlFilter;
			
			htmlFilter && htmlFilter.addRules( htmlFilterRule );

			// ignore strings
			var ignoreStringsCfg = editor.config.atd_ignore_strings || '';
			ignoreStringsCfg = ignoreStringsCfg.split( /,\s*/g );

			for ( var i = 0 ; i < ignoreStringsCfg.length ; i++ )
			{
				ignoreStrings[ ignoreStringsCfg[ i ] ] = 1;
			}

			if ( editor.config.atd_ignore_enable )
			{
				var ignoreStringsCookie = Cookie.getHash( 'atd_ignore' ) || {};
				CKEDITOR.tools.extend( ignoreStrings, ignoreStringsCookie );
			}

			// ignore types
			var ignoreTypesCfg = editor.config.atd_ignore_types || '';
			ignoreTypesCfg = ignoreTypesCfg.split( /,\s*/g );

			for ( var i = 0 ; i < ignoreTypesCfg.length ; i++ )
			{
				ignoreTypes[ ignoreTypesCfg[ i ] ] = 1;
			}
		},

		afterInit : function( editor )
		{
			// Prevent word marker line from displaying in elements path. (#3570)
			var elementsPathFilters;
			if ( editor._.elementsPath && ( elementsPathFilters = editor._.elementsPath.filters ) )
			{
				elementsPathFilters.push( function( element )
					{
						if ( isMarkedNode( element ) )
							return false;
					} );
			}
		}
	});
})();

/**
 * Styles for error nodes
 * @name CKEDITOR.config.atd_styles
 * @type Object
 */
CKEDITOR.config.atd_styles =
	{
		spelling : {element : 'span', styles : {'border-bottom' : '2px solid red'}, attributes : {'class' : 'atdError'}},
		grammar : {element : 'span', styles : {'border-bottom' : '2px solid green'}, attributes : {'class' : 'atdError'}},
		suggestion : {element : 'span', styles : {'border-bottom' : '2px solid blue'}, attributes : {'class' : 'atdError'}}
	};

/**
 * URL to AtD proxy script
 * @name CKEDITOR.config.atd_rpc_url
 * @type String
 * @default pluginPath + 'server/proxy.php'
 * @example CKEDITOR.config.atd_rpc_url = 'proxy.php';
 */

/**
 * AtD API Key
 * @name CKEDITOR.config.atd_api_key
 * @type String
 * @default ''
 * @example CKEDITOR.config.atd_api_key = '';
 */

/**
 * AtD theme
 * @name CKEDITOR.config.atd_theme
 * @type String
 * @default 'tinymce'
 * @example CKEDITOR.config.atd_theme = 'wordpress';
 */
CKEDITOR.config.atd_theme = 'tinymce';

/**
 * This list contains the categories of errors we want to ignore
 * @name CKEDITOR.config.atd_ignore_types
 * @type String
 * @default 'Bias Language,Cliches,Complex Expression,Diacritical Marks,Double Negatives,Hidden Verbs,Jargon Language,Passive voice,Phrases to Avoid,Redundant Expression'
 * @example CKEDITOR.config.atd_ignore_types = '';
 */
CKEDITOR.config.atd_ignore_types = 'Bias Language,Cliches,Complex Expression,Diacritical Marks,Double Negatives,Hidden Verbs,Jargon Language,Passive voice,Phrases to Avoid,Redundant Expression';

/**
 * Strings which this plugin should ignore
 * @name CKEDITOR.config.atd_ignore_strings
 * @type String
 * @default ''
 * @example CKEDITOR.config.atd_ignore_strings = 'MindTouch';
 */
CKEDITOR.config.atd_ignore_strings = '';

/**
 * Enable "Ignore Always" menu item, uses cookies by default.
 * Set {@link CKEDITOR.config.atd_ignore_rpc_url} to a URL AtD should send ignore requests to.
 * @name CKEDITOR.config.atd_ignore_enable
 * @type boolean
 * @default false
 * @example CKEDITOR.config.atd_ignore_enable = true;
 */
CKEDITOR.config.atd_ignore_enable = false;

/**
 * URL to send ignore requests
 * @name CKEDITOR.config.atd_ignore_rpc_url
 * @type String
 * @default ''
 * @example CKEDITOR.config.atd_ignore_rpc_url = '';
 */
