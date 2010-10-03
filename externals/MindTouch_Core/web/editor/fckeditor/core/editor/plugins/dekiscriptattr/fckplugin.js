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

(function() {
	var pluginPath = FCKConfig.PluginsPath + 'dekiscriptattr/' ;
	
	var FCKDekiScriptAtrrCommand = function( name, title, url, width, height, getStateFunction, getStateParam, customValue )
	{
		this.Name	= name ;
		this.Title	= title ;
		this.Url	= url ;
		this.Width	= width ;
		this.Height	= height ;
		this.CustomValue = customValue ;

		this.GetStateFunction	= getStateFunction ;
		this.GetStateParam		= getStateParam ;

		this.Resizable = false ;
	} ;

	FCKDekiScriptAtrrCommand.prototype.Execute = function()
	{
		var currentNode = FCKSelection.GetParentElement() ;
		FCKDialog.OpenDialog( 'FCKDialog_' + this.Name , this.Title, this.Url, this.Width, this.Height, currentNode, null, this.Resizable ) ;
	} ;
	
	FCKDekiScriptAtrrCommand.prototype.GetState = MindTouchDeki.GetState ;

	FCKCommands.RegisterCommand('DekiScriptAttr', new FCKDekiScriptAtrrCommand('DekiScriptAttr', FCKLang.DlgDekiScriptAttrTitle, pluginPath + 'dekiscriptattr.html', 480, 200)) ;	
	FCKToolbarItems.RegisterItem( 'DekiScriptAttr', new FCKToolbarButton( 'DekiScriptAttr', FCKLang.DekiScriptAttr, null, null, null, true, pluginPath + 'dekiscript_icon.gif' ) ) ;
	
	FCK.ContextMenu.RegisterListener( {
		AddItems: function(menu, tag, tagName) {
			if ( FCKCommands.GetCommand( 'DekiScriptAttr' ).GetState() == FCK_TRISTATE_DISABLED )
				return ;
			
			var currentNode = FCKSelection.GetParentElement() ;
		
			menu.AddSeparator() ;
			menu.AddItem('DekiScriptAttr', FCKLang.DekiScriptAttr, pluginPath + 'dekiscript_icon.gif', false, currentNode) ;
		}
	}) ;
	
})() ;
