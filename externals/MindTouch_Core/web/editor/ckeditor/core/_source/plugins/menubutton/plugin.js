/*
Copyright (c) 2003-2010, CKSource - Frederico Knabben. All rights reserved.
For licensing, see LICENSE.html or http://ckeditor.com/license
*/

CKEDITOR.plugins.add( 'menubutton',
{
	requires : [ 'button', 'contextmenu' ],
	beforeInit : function( editor )
	{
		editor.ui.addHandler( CKEDITOR.UI_MENUBUTTON, CKEDITOR.ui.menuButton.handler );
	}
});

/**
 * Button UI element.
 * @constant
 * @example
 */
CKEDITOR.UI_MENUBUTTON = 5;

(function()
{
	var clickFn = function( editor )
	{
		var _ = this._;

		// Do nothing if this button is disabled.
		if ( _.state === CKEDITOR.TRISTATE_DISABLED )
			return;

		/**
		 * Don't save the state if menu is opened
		 * to prevent enabled button after double click
		 *
		 * @author MindTouch
		 *
		 */
		if ( !_.on )
		{
			_.previousState = _.state;
		}
		/* END */

		// Check if we already have a menu for it, otherwise just create it.
		var menu = _.menu;
		if ( !menu )
		{
			menu = _.menu = new CKEDITOR.plugins.contextMenu( editor );
			menu.definition.panel.attributes[ 'aria-label' ] = editor.lang.common.options;

			menu.onHide = CKEDITOR.tools.bind( function()
				{
					this.setState( _.previousState );
					/**
					 * @author MindTouch
					 */
					_.on = false;
					/* END */
				},
				this );

			// Initialize the menu items at this point.
			if ( this.onMenu )
			{
				menu.addListener( this.onMenu );
			}
		}

		/**
		 * The second click sould hide menu
		 * 
		 * @author MindTouch
		 */
		if ( _.on )
		{
			menu._.menu.hide();
			menu.editor.focus();
			return;
		}
//		if ( _.on )
//		{
//			menu.hide();
//			return;
//		}
		/* END */

		menu.show( CKEDITOR.document.getById( this._.id ), 4 );

		/**
		 * menu.show hides the menu before show it
		 * so let set state to ON after menu.show
		 * to prevent enabled button after second click
		 *
		 * @author MindTouch
		 * 
		 */
		this.setState( CKEDITOR.TRISTATE_ON );

		_.on = true;
		/* END */
	};


	CKEDITOR.ui.menuButton = CKEDITOR.tools.createClass(
	{
		base : CKEDITOR.ui.button,

		$ : function( definition )
		{
			// We don't want the panel definition in this object.
			var panelDefinition = definition.panel;
			delete definition.panel;

			this.base( definition );

			this.hasArrow = true;

			this.click = clickFn;
		},

		statics :
		{
			handler :
			{
				create : function( definition )
				{
					return new CKEDITOR.ui.menuButton( definition );
				}
			}
		}
	});
})();
