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

var MindTouchDeki_Save = function()
{}

MindTouchDeki_Save.prototype.Execute = function()
{
	var Deki = parent.Deki ;
	
	if ( Deki.Editor && Deki.Editor.CheckPermissions )
	{
		Deki.Editor.CheckPermissions( this.Save, this ) ;
	}
	else
	{
		this.Save() ;
	}
}

MindTouchDeki_Save.prototype.Save = function()
{
	if ( FCKConfig.ReadOnly )
		return false ;
	
	MindTouchDeki.ScrollToTop() ;
	
	// see #0004799
	if ( FCKBrowserInfo.IsOpera )
	{
		FCK.UpdateLinkedField() ;
	}

	FCK.Events.FireEvent( 'OnSave' ) ;
	
	FCK.GetParentForm().submit() ;
}

MindTouchDeki_Save.prototype.GetState = function()
{
	if ( FCKConfig.ReadOnly )
		return FCK_TRISTATE_DISABLED ;

	return FCK_TRISTATE_OFF ;
}

var MindTouchDeki_Cancel = function()
{}

MindTouchDeki_Cancel.prototype.Execute = function()
{
	var oFitWindowCommand = FCKCommands.GetCommand( 'FitWindow' ) ;

	if ( oFitWindowCommand.GetState() == FCK_TRISTATE_ON )
	{
		oFitWindowCommand.Execute() ;
	}
	
	MindTouchDeki.ScrollToTop() ;

	FCK.Events.FireEvent( 'OnCancel' ) ;
}

MindTouchDeki_Cancel.prototype.GetState = function()
{
	return FCK_TRISTATE_OFF ;
}


// Register the related commands.
FCKCommands.RegisterCommand( 'MindTouchDeki_Save', new MindTouchDeki_Save );
FCKCommands.RegisterCommand( 'MindTouchDeki_Cancel', new MindTouchDeki_Cancel );

// Create the "Save" toolbar button.
var oSaveItem = new FCKToolbarButton( 'MindTouchDeki_Save', FCKLang.Save, null, FCK_TOOLBARITEM_ICONTEXT, true, true, FCKConfig.DekiCommonPath + "/icons/save_18.gif" ) ;

// 'MindTouchDeki_Save' is the name used in the Toolbar config.
FCKToolbarItems.RegisterItem( 'MindTouchDeki_Save', oSaveItem ) ;

// Create the "Cancel" toolbar button.
var oCancelItem = new FCKToolbarButton( 'MindTouchDeki_Cancel', FCKLang.Cancel, null, FCK_TOOLBARITEM_ICONTEXT, true, true, [ FCKConfig.DekiCommonPath + "/icons/icons.gif", 16, 101 ] );

// 'MindTouchDeki_Cancel' is the name used in the Toolbar config.
FCKToolbarItems.RegisterItem( 'MindTouchDeki_Cancel', oCancelItem );
