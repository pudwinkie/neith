/*
Copyright (c) 2003-2009, CKSource - Frederico Knabben. All rights reserved.
For licensing, see LICENSE.html or http://ckeditor.com/license
*/

(function()
{
	CKEDITOR.plugins.add( 'transformations',
	{
		requires : [ 'combobutton', 'styles' ],
		
		lang : [ 'en', 'cs', 'de', 'en-au', 'es', 'et', 'fr-ca', 'fr', 'he', 'hu', 'it', 'ja', 'ko', 'nl', 'pt-br', 'ru', 'sv' ],
		
		init : function( editor )
		{
			var config = editor.config,
				styles = {},
				plugin = this;
			
			editor.ui.addComboButton( 'Transformations',
					{
						label : editor.lang.transformations.label,
						title : editor.lang.transformations.panelTitle,
						className : 'cke_transformations',
						icon : plugin.path + 'images/page_white_transformations.png',

						panel :
						{
							css : editor.skin.editor.css.concat( config.contentsCss ),
							multiSelect : false,
							attributes : { 'aria-label' : editor.lang.panelTitle }
						},

						init : function()
						{
							var combo = this;
							
							Deki.$.getJSON( '/deki/gui/editor.php?method=transform', function( data )
								{
									var transformations = data.body;

									combo.startGroup( editor.lang.transformations.panelTitle );

									for ( var i = 0 ; i < transformations.length ; i++ )
									{
										var transform = transformations[ i ];
										var name = transform.name || transform.func;
										var text = ( transform.func.length ) ? transform.func : transform.title;
										
										var styleDefinition =
											{
												name : name,
												element : transform.tags,
												attributes :
												{
													'class' : 'deki-transform',
													'function' : transform.func
												}
											};
										
										styles[ name ] = new CKEDITOR.style( styleDefinition );

										// Add the tag entry to the panel list.
										combo.add( name, '<span>' + text + '</span>', text );
									}
									
									combo.commit();

									combo.onOpen();
								} );
						},

						onClick : function( value )
						{
							editor.focus();
							editor.fire( 'saveSnapshot' );

							var style = styles[ value ],
								selection = editor.getSelection();
							
							if ( !selection )
								return;

							if ( style.type == CKEDITOR.STYLE_OBJECT )
							{
								var element = selection.getSelectedElement();
								if ( element )
									style.applyToObject( element );

								return;
							}

							var elementPath = new CKEDITOR.dom.elementPath( selection.getStartElement() );

							if ( style.checkActive( elementPath ) )
							{
								switch ( style.type )
								{
									case CKEDITOR.STYLE_INLINE :
										style.remove( editor.document );
										break;
									case CKEDITOR.STYLE_BLOCK :
										var element = elementPath.block || elementPath.blockLimit;
										element.removeClass( 'deki-transform' );
										element.removeAttribute( 'function' );
										break;
								}
							}	
							else
								style.apply( editor.document );

							editor.fire( 'saveSnapshot' );
						},

						onRender : function()
						{
							var me = this;

							editor.on( 'selectionChange', function( ev )
								{
									var currentValue = this.getValue();
	
									var elementPath = ev.data.path,
										elements = elementPath.elements;
	
									// For each element into the elements path.
									for ( var i = 0, element ; i < elements.length ; i++ )
									{
										element = elements[i];
	
										// Check if the element is removable by any of
										// the styles.
										for ( var value in styles )
										{
											if ( styles[ value ].checkElementRemovable( element, true ) )
											{
												if ( value != currentValue )
												{
													this.setValue( value );
												}

												me.setState( CKEDITOR.TRISTATE_ON );

												return;
											}
										}
									}
	
									// If no styles match, just empty it.
									this.setValue( '' );

									me.setState( CKEDITOR.TRISTATE_OFF );
								},
								this);
						},
						
						onOpen : function()
						{
							if ( CKEDITOR.env.ie )
								editor.focus();

							var selection = editor.getSelection();
							
							if ( !selection )
								return;

							var element = selection.getSelectedElement(),
								elementName = element && element.getName(),
								elementPath = new CKEDITOR.dom.elementPath( element || selection.getStartElement() );

							var counter = [ 0, 0, 0, 0 ];
							this.showAll();
							this.unmarkAll();
							for ( var name in styles )
							{
								var style = styles[ name ],
									type = style.type;

								if ( type == CKEDITOR.STYLE_OBJECT )
								{
									if ( element && style.element == elementName )
									{
										if ( style.checkElementRemovable( element, true ) )
											this.mark( name );

										counter[ type ]++;
									}
									else
										this.hideItem( name );
								}
								else
								{
									if ( style.checkActive( elementPath ) )
										this.mark( name );

									counter[ type ]++;
								}
							}
						}

					});
		}
	});
})();
