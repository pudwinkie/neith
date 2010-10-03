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

var FCKToolbarTransformContentCombo = function( tooltip, style )
{
	if ( tooltip === false )
		return ;

	this.CommandName = 'TransformContent' ;
	this.Label      = this.GetLabel() ;
	this.Tooltip    = tooltip ? tooltip : this.Label ;
	this.Style      = style ? style : FCK_TOOLBARITEM_ICONTEXT ;

	this.PanelWidth = 130 ;
	this.FieldWidth = 130 ;

	this.DefaultLabel = FCKLang.NoTransformation || '' ;
}

FCKToolbarTransformContentCombo.oTransformCache = null;

// Inherit from FCKToolbarSpecialCombo.
FCKToolbarTransformContentCombo.prototype = new FCKToolbarSpecialCombo ;

FCKToolbarTransformContentCombo.prototype.GetLabel = function()
{
	return FCKLang.Transform ;
}

FCKToolbarTransformContentCombo.GetTransformations = function( targetSpecialCombo )
{
	var transformations = {}, transform ;

	if ( FCKToolbarTransformContentCombo.oTransformCache == null )
	{
		transformations[ 'loading' ] = FCKConfig.Transformations[ 'Loading' ] ;
		transformations[ 'loading' ].Label = FCKLang.Loading ;
		FCKToolbarTransformContentCombo.LoadTransformations( targetSpecialCombo ) ;
	}
	else
	{
		transformations[ 'notransformation' ] = FCKConfig.Transformations[ 'NoTransformation' ] ;
		transformations[ 'notransformation' ].Label = FCKLang.NoTransformation ;

		for ( var i = 0 ; i < FCKToolbarTransformContentCombo.oTransformCache.length ; i++ )
		{
			transform = FCKToolbarTransformContentCombo.oTransformCache[i] ;
			var sFunctionName = transform.func ;
			var sTags = ( transform.tags ) ? transform.tags.toLowerCase() + ',' : '' ;

			transformations[ sFunctionName ] = { Function : sFunctionName, Tags : sTags } ;
			transformations[ sFunctionName ].Label = sFunctionName ;
		}
	}

	return transformations ;
}

FCKToolbarTransformContentCombo.prototype.CreateItems = function( targetSpecialCombo )
{
	var targetDoc = targetSpecialCombo._Panel.Document ;

	// Add the Editor Area CSS to the panel so the style classes are previewed correctly.
	FCKTools.AppendStyleSheet( targetDoc, FCKConfig.ToolbarComboPreviewCSS ) ;
	FCKTools.AppendStyleString( targetDoc, FCKConfig.EditorAreaStyles ) ;
	targetDoc.body.className += ' ForceBaseFont' ;

	// Add ID and Class to the body.
	FCKConfig.ApplyBodyAttributes( targetDoc.body ) ;

	FCKToolbarTransformContentCombo.CreateItems( targetSpecialCombo ) ;

	// We must prepare the list before showing it.
	targetSpecialCombo.OnBeforeClick = this.TransformCombo_OnBeforeClick ;
}

FCKToolbarTransformContentCombo.CreateItems = function( targetSpecialCombo )
{
	// Get the items list.
	var transformations = FCKToolbarTransformContentCombo.GetTransformations( targetSpecialCombo ) ;

	for ( var transformName in transformations )
	{
		var transform = transformations[ transformName ] ;

		var item = targetSpecialCombo.AddItem( transformName, transform.Label || transformName ) ;

		item.Transform = transform ;
	}
}

FCKToolbarTransformContentCombo.prototype.RefreshActiveItems = function( targetSpecialCombo )
{
	var startElement = FCK.ToolbarSet.CurrentInstance.Selection.GetBoundaryParentElement( true ) ;

	if ( startElement )
	{
		var path = new FCKElementPath( startElement ) ;

		for ( var i = 0 ; i < path.Elements.length ; i++ )
		{
			var pathElement = path.Elements[i] ;
			var sFunction = pathElement.getAttribute( 'function' ) ;

			if ( sFunction )
			{
				for ( var j in targetSpecialCombo.Items )
				{
					var item = targetSpecialCombo.Items[j] ;
					var transform = item.Transform ;

					if ( transform.Function == sFunction )
					{
						if ( transform.Tags && transform.Tags.Contains( pathElement.nodeName.toLowerCase() + ',' ) )
						{
							targetSpecialCombo.SetLabel( transform.Label ) ;
							return ;
						}
					}
				}
			}
		}
	}

	targetSpecialCombo.SetLabel( this.DefaultLabel ) ;
}

FCKToolbarTransformContentCombo.prototype.TransformCombo_OnBeforeClick = function( targetSpecialCombo )
{
	// Clear the current selection.
	targetSpecialCombo.DeselectAll() ;

	var startElement = FCK.ToolbarSet.CurrentInstance.Selection.GetBoundaryParentElement( true ) ;
	var bNoTransform = true ;

	if ( ! targetSpecialCombo.Items[ 'loading' ] )
	{
		if ( startElement )
		{
			var path = new FCKElementPath( startElement ) ;
			var pathElement, item, transform, tag, sFunction ;

			transformations:

			for ( var i in targetSpecialCombo.Items )
			{
				item = targetSpecialCombo.Items[i] ;
				transform = item.Transform ;

				if ( 'notransformation' == i )
				{
					item.style.display = '' ;
					continue transformations ;
				}

				for ( var j = 0 ; j < path.Elements.length ; j++ )
				{
					pathElement = path.Elements[j] ;
					tag = pathElement.nodeName.toLowerCase() + ',' ;

					if ( transform.Tags && transform.Tags.Contains( tag ) )
					{
						item.style.display = '' ;

						sFunction = pathElement.getAttribute( 'function' ) ;

						if ( sFunction == transform.Function )
						{
							bNoTransform = false ;
							targetSpecialCombo.SelectItem( item ) ;
						}

						continue transformations ;
					}
					else
					{
						item.style.display = 'none' ;
					}
				}
			}

			if ( bNoTransform )
			{
				targetSpecialCombo.SelectItem( targetSpecialCombo.Items[ 'notransformation' ] ) ;
			}
		}
	}
	else
	{
		item = targetSpecialCombo.Items[ 'loading' ] ;
		item.style.display = '' ;
	}
}

// TODO: consolidate into a common library since this is used for @gui calls
FCKToolbarTransformContentCombo.getContentType = function(o)
{
	// bugfix for german IE
	var contentType = o.getResponseHeader['Content-Type'] ?
					o.getResponseHeader['Content-Type'] :
					o.getResponseHeader['Content-type'];

	var aSplit = contentType.split(';'); // only grab the content type not the charset

	return String(aSplit[0]).replace(/\s+$/, ''); // IE reports Content-Type having trailing ASCII 13
};

FCKToolbarTransformContentCombo.LoadTransformations = function( targetSpecialCombo )
{
	// callback functions for ajax
	var success = function(o)
	{
		var oData = null;
		var sContentType = FCKToolbarTransformContentCombo.getContentType(o);//o.getResponseHeader[ 'Content-Type' ];
		if ('application/json' == sContentType)
		{
			try
			{
				oData = parent.YAHOO.lang.JSON.parse( o.responseText ) ;
				FCKToolbarTransformContentCombo.oTransformCache = oData.body ;

				var bPanelOpened = targetSpecialCombo._Panel.CheckIsOpened() ;

				targetSpecialCombo._Panel.Hide() ;
				targetSpecialCombo.ClearItems() ;

				FCKToolbarTransformContentCombo.CreateItems( targetSpecialCombo ) ;

				if ( bPanelOpened )
				{
					var oField = parent.YAHOO.util.Dom.getElementsByClassName( 'SC_Field', 'div', targetSpecialCombo._OuterTable );
					FCKSpecialCombo_OnClick.call( oField[0], null, targetSpecialCombo );
				}

				return;
			}
			catch (e) {}
		}
		failure();
	};

	var failure = function(o) {};
	// end callbacks

	// need to fetch the transform results from the api
	// send a post request to verify the move is allowed
	var oCallback = {
					 'success': success,
					 'failure': failure,
					 'timeout': 6000
					};
	var oRequest = parent.YAHOO.util.Connect.asyncRequest( 'GET', '/deki/gui/editor.php?method=transform', oCallback, null );
}

var FCKTransformCommand = function()
{}

FCKTransformCommand.prototype =
{
	Name : 'TransformContent',

	Execute : function( transformName, transformComboItem )
	{
		var YAHOO = parent.YAHOO ;

		if ( transformName != 'loading' )
		{
			FCKUndo.SaveUndoStep() ;

			// need to transform the content
			var startElement = FCK.ToolbarSet.CurrentInstance.Selection.GetBoundaryParentElement( true ) ;

			if ( startElement )
			{
				var path = new FCKElementPath( startElement ) ;
				var transform = transformComboItem.Transform ;

				for ( var i = 0 ; i < path.Elements.length ; i++ )
				{
					var pathElement = path.Elements[i] ;

					if ( transform.Tags && transform.Tags.Contains( pathElement.nodeName.toLowerCase() + ',' ) )
					{
						YAHOO.util.Dom.addClass( pathElement, 'deki-transform' ) ;
						pathElement.setAttribute( 'function', transformName ) ;
						break ;
					}
					else if ( transformName == 'notransformation' )
					{
						if ( FCKDomTools.HasAttribute( pathElement, 'function' ) )
						{
							YAHOO.util.Dom.removeClass( pathElement, 'deki-transform' ) ;
							pathElement.removeAttribute( 'function', 0 ) ;
							break ;
						}
					}
				}
			}
			FCKUndo.SaveUndoStep() ;
		}

		FCK.Focus() ;
		FCK.Events.FireEvent( 'OnSelectionChange' ) ;
	},

	GetState : MindTouchDeki.GetState
}

FCKCommands.RegisterCommand( 'TransformContent', new FCKTransformCommand );

var oTransformItem = new FCKToolbarTransformContentCombo( null, FCK_TOOLBARITEM_ONLYTEXT ) ;
FCKToolbarItems.RegisterItem( 'MindTouchDeki_Transform', oTransformItem ) ;
