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

// @see bug #0007138
// height of document.body is height of viewport in Quirks Mode
if ( FCKBrowserInfo.IsSafari )
{
	FCKConfig.DocType = '<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN"' +
						'"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">' ;
}

FCKConfig.AutoDetectLanguage	= false ;

FCKConfig.ProcessHTMLEntities	= false ;
FCKConfig.IncludeLatinEntities	= false ;
FCKConfig.IncludeGreekEntities	= false ;

FCKConfig.BeautifySource		= false ;

FCKConfig.StartupFocus	= true ;
FCKConfig.DekiTabSpaces = 4 ;
FCKConfig.ToolbarCanCollapse	= false ;

FCKConfig.ToolbarSets["Default"] = [
    ['MindTouchDeki_Save','-','Source'],
    ['RemoveFormat','-','TextColor','BGColor'],
    ['MindTouchDeki_InsertExtension','-','MindTouchDeki_Transform'],
    '/',
    ['MindTouchDeki_Cancel'],
    ['Bold','Italic','Underline','StrikeThrough','-','OrderedList','UnorderedList','JustifyLeft','JustifyCenter','JustifyRight','JustifyFull','-','Outdent','Indent','Blockquote'],
    ['MindTouchDeki_InsertLink','MindTouchDeki_InsertImage','MindTouchDeki_AttachImage','MindTouchDeki_InsertVideo','MindTouchDeki_InsertTemplate','Table'],
    '/',
    ['Style','FontFormat','FontName','FontSize','-','FitWindow']
] ;

FCKConfig.ToolbarSets["Advanced"] = [
    ['MindTouchDeki_Save','-','Source'],
    ['Undo','Redo','-','Find','Replace','-','RemoveFormat','-','TextColor','BGColor'],
    ['MindTouchDeki_InsertExtension','-','MindTouchDeki_Transform'],
    '/',
    ['MindTouchDeki_Cancel'],
    ['PasteText','PasteWord'],
    ['Bold','Italic','Underline','StrikeThrough','-','OrderedList','UnorderedList','JustifyLeft','JustifyCenter','JustifyRight','JustifyFull','-','Outdent','Indent','Blockquote','CreateDiv'],
    ['MindTouchDeki_InsertLink','MindTouchDeki_InsertImage','MindTouchDeki_AttachImage','MindTouchDeki_InsertVideo','MindTouchDeki_InsertTemplate','Table'],
    '/',
    ['Style','FontFormat','FontName','FontSize','-','FitWindow']
] ;

FCKConfig.ToolbarSets["Everything"] = [
	['MindTouchDeki_Save','-','Source','Preview','-','MindTouchDeki_InsertTemplate'],
	['Cut','Copy','Paste','PasteText','PasteWord','-','Print','SpellCheck'],
	['Undo','Redo','-','Find','Replace','-','SelectAll','RemoveFormat'],
	['Form','Checkbox','Radio','TextField','Textarea','Select','Button','ImageButton','HiddenField'],
	'/',
    ['MindTouchDeki_Cancel'],
	['Bold','Italic','Underline','StrikeThrough','-','Subscript','Superscript'],
	['OrderedList','UnorderedList','-','Outdent','Indent','Blockquote','CreateDiv'],
	['JustifyLeft','JustifyCenter','JustifyRight','JustifyFull'],
	['MindTouchDeki_InsertLink','Unlink','Anchor'],
	['MindTouchDeki_InsertImage','MindTouchDeki_AttachImage','MindTouchDeki_InsertVideo','Flash','Table','Rule','Smiley','SpecialChar','PageBreak'],
	'/',
	['Style','FontFormat','FontName','FontSize'],
    ['MindTouchDeki_Transform','-','MindTouchDeki_InsertExtension'],
	['TextColor','BGColor'],
	['FitWindow','ShowBlocks','-','About']		// No comma for the last row.
] ;

FCKConfig.ToolbarSets["Simple"] = [
    ['MindTouchDeki_Save','-','Source'],
    ['RemoveFormat','-','FontFormat','-','TextColor','BGColor'],
    ['MindTouchDeki_InsertExtension','-','FitWindow'],
    '/',
    ['MindTouchDeki_Cancel'],
    ['PasteText','PasteWord'],
    ['Bold','Italic','Underline','StrikeThrough','-','OrderedList','UnorderedList','JustifyLeft','JustifyCenter','JustifyRight','JustifyFull','-','Outdent','Indent'],
    ['MindTouchDeki_InsertLink','MindTouchDeki_InsertImage','MindTouchDeki_InsertTemplate','Table']
] ;

FCKConfig.ToolbarSets["Basic"] = [
    ['MindTouchDeki_Save','-','MindTouchDeki_Cancel'],
    ['MindTouchDeki_InsertLink','MindTouchDeki_InsertImage','MindTouchDeki_InsertTemplate','Table'],
    ['MindTouchDeki_InsertExtension'],
    ['FitWindow']
] ;

FCKConfig.ToolbarSets["Deki_ReadOnly"] = [
    ['MindTouchDeki_Cancel','-','Source'],
    ['FitWindow']
] ;

FCKConfig.Keystrokes = [
	[ CTRL + 65 /*A*/, true ],
	[ CTRL + 67 /*C*/, true ],
	[ CTRL + 70 /*F*/, true ],
	[ CTRL + 83 /*S*/, true ],
	[ CTRL + 84 /*T*/, true ],
	[ CTRL + 88 /*X*/, true ],
	[ CTRL + 86 /*V*/, 'Paste' ],
	[ CTRL + 45 /*INS*/, true ],
	[ SHIFT + 45 /*INS*/, 'Paste' ],
	[ CTRL + 88 /*X*/, 'Cut' ],
	[ SHIFT + 46 /*DEL*/, 'Cut' ],
	[ CTRL + 90 /*Z*/, 'Undo' ],
	[ CTRL + 89 /*Y*/, 'Redo' ],
	[ CTRL + SHIFT + 90 /*Z*/, 'Redo' ],
	[ CTRL + 75 /*K*/, 'MindTouchDeki_InsertLink' ],
    [ CTRL + 87 /*W*/, 'MindTouchDeki_InsertQuickLink' ],
	[ CTRL + 66 /*B*/, 'Bold' ],
	[ CTRL + 73 /*I*/, 'Italic' ],
	[ CTRL + 85 /*U*/, 'Underline' ],
	[ CTRL + SHIFT + 83 /*S*/, 'MindTouchDeki_Save' ],
	[ CTRL + ALT + 13 /*ENTER*/, 'FitWindow' ],
    [ CTRL + 76 /*L*/, 'JustifyLeft' ],
    [ CTRL + 69 /*E*/, 'JustifyCenter' ],
    [ CTRL + 82 /*R*/, 'JustifyRight' ],
    [ CTRL + 74 /*J*/, 'JustifyFull' ],
	[ SHIFT + 32 /*SPACE*/, 'Nbsp' ]
] ;

FCKConfig.ContextMenu = ['Generic','MindTouchDeki_Link','Anchor','MindTouchDeki_Image','Flash','Select','Textarea','Checkbox','Radio','TextField','HiddenField','ImageButton','Button','BulletedList','NumberedList','Table','TableToText','TextToTable','Form','DivContainer'] ;

FCKConfig.FontFormats	= 'p;pre;h1;h2;h3;h4;h5;h6' ;
FCKConfig.FontNames		= 'Courier New;Times New Roman;Verdana' ;
FCKConfig.FontSizes		= 'xx-small;x-small;small;medium;large;x-large;xx-large' ;

FCKConfig.MaxUndoLevels = 20 ;

FCKConfig.AutoGrowMin = 200 ;

FCKConfig.AutoGrow = true ;
FCKConfig.SourceAutoGrow = true ;

// IE only. Autogrowing in source mode is disable by default in IE
// since IE losts undo/ready history of textarea. see #0005686
FCKConfig.IESourceAutoGrow = false ;

// This will be applied to the body element of the editor
FCKConfig.BodyId = 'topic' ;
FCKConfig.BodyClass = 'deki-content-edit' ;

FCKConfig.Transformations = 
{
    'Loading' : { Function : '' },
    'NoTransformation' : { Function : '' }
};

FCKConfig.CustomStyles = 
{
    'Code'	: { Element : 'code' },
    'DekiScript'  : { Element : 'pre', Attributes : { 'class' : 'script' } },	
    'Comment'  : { Element : 'p', Attributes : { 'class' : 'comment' } },
    'Plaintext': { Element : 'span', Attributes : { 'class' : 'plain' } }
};

FCKConfig.LinkBrowser = false ;
FCKConfig.ImageBrowser = false ;
FCKConfig.FlashBrowser = false ;
FCKConfig.LinkUpload = false ;
FCKConfig.ImageUpload = false ;
FCKConfig.FlashUpload = false ;
