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

CKEDITOR.editorConfig = function( config )
{
	config.customConfig = CKEDITOR.basePath + '../editor/config.php';

	config.bodyId = 'topic';
	config.bodyClass = 'deki-content-edit';

	// @see #0007806
	if ( CKEDITOR.env.ie )
	{
		config.docType = '<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">';
	}
	
	config.plugins = config.plugins ? config.plugins + ',' : '';
	config.plugins += 'autogrow,ckoverrides,combobutton,definitionlist,dsbar,extensions,floatingtoolbar,inlinestyle,menubuttons,mindtouch,mindtouchdialog,mindtouchimage,mindtouchlink,mindtouchkeystrokes,mindtouchsave,mindtouchtemplates,styletools,tableadvanced,transformations,video';

	config.skin = 'mindtouch';
	config.theme = 'mindtouch';
	config.resize_enabled = false;

	config.scayt_autoStartup = false;

	config.entities = false;
	config.entities_greek = false;
	config.entities_latin = false;
	
	config.startupFocus = true;
	
	config.format_tags = 'p;pre;h1;h2;h3;h4;h5;h6';

	config.tabSpaces = 4;

	config.format_p = { element : 'p', attributes : { 'class' : '' } };

	config.stylesSet = [
		{ name : 'Normal'				, element : 'p', attributes : { 'class' : '' } },
		{ name : 'Formatted'			, element : 'pre' },
		{ name : 'Block Quote'			, element : 'blockquote' },
		{ name : 'Plaintext (nowiki)'	, element : 'span',	attributes : { 'class' : 'plain' } }
	];

	config.toolbar_Everything =
		[
			['MindTouchSave','MindTouchCancel'],
			['ViewMenu'],
			['NewPage','Preview'],
			['Cut','Copy','Paste','PasteText','PasteFromWord','-','Print', 'SpellChecker', 'Scayt'],
			['Undo','Redo','-','Find','Replace','-','SelectAll','RemoveFormat'],
			'/',
			['Bold','Italic','Underline','Strike','-','Subscript','Superscript'],
			['NumberedList','BulletedList','-','DefinitionList','DefinitionTerm','DefinitionDescription','-','Outdent','Indent','CreateDiv'],
			['JustifyLeft','JustifyCenter','JustifyRight','JustifyBlock'],
			'/',
			['Font','FontSize','-','TextColor','BGColor'],
			['Form', 'Checkbox', 'Radio', 'TextField', 'Textarea', 'Select', 'Button', 'ImageButton', 'HiddenField'],
			['HorizontalRule','Smiley','SpecialChar'],
			'/',
			['Normal','H1','H2','H3','Hx','Styles'],
			['InsertMenu','MindTouchLink','Unlink','Anchor','Table','MindTouchImage','MindTouchTemplates','Video'],
			['Maximize','-','About']
		];
	
	config.toolbar_Default =
		[
			['MindTouchSave','MindTouchCancel'],
			['ViewMenu'],
			['Undo','Redo'],
			['Replace'],
			['Cut','Copy','Paste','PasteText','PasteFromWord'],
			['Transformations'],
			['Maximize'],
			'/',
			['Font','FontSize','-','TextColor','BGColor','-','RemoveFormat'],
			['Bold','Italic','Underline','Strike','Subscript','Superscript','Code'],
			['NumberedList','BulletedList','DefinitionList','-','Outdent','Indent'],
			['JustifyLeft','JustifyCenter','JustifyRight','JustifyBlock'],
			'/',
			['Normal','H1','H2','H3','Hx','Styles'],
			['InsertMenu','MindTouchLink','Unlink','Table','MindTouchImage','MindTouchTemplates','Video']
		];

	config.toolbar_Advanced =
		[
			['MindTouchSave','MindTouchCancel'],
			['ViewMenu'],
			['Undo','Redo','-','Find','Replace','-','SelectAll','RemoveFormat'],
			['Cut','Copy','Paste','PasteText','PasteFromWord'],
			['SpellChecker', 'Scayt'],
			['Transformations'],
			['Maximize'],
			'/',
			['Font','FontSize'],
			['Bold','Italic','Underline','Strike','Subscript','Superscript','Code'],
			['NumberedList','BulletedList','-','DefinitionList','DefinitionTerm','DefinitionDescription','-','Outdent','Indent','CreateDiv'],
			['JustifyLeft','JustifyCenter','JustifyRight','JustifyBlock'],
			'/',
			['Normal','H1','H2','H3','Hx','Styles'],
			['InsertMenu','MindTouchLink','Unlink','Table','MindTouchImage','MindTouchTemplates','Video'],
			['TextColor','BGColor']
		];

	config.toolbar_Simple =
		[
			['MindTouchSave','MindTouchCancel'],
			['ViewMenu'],
			['PasteText','PasteFromWord'],
			['Bold','Italic','Underline','Strike','Subscript','Superscript','Code'],
			['NumberedList','BulletedList','-','Outdent','Indent'],
			['JustifyLeft','JustifyCenter','JustifyRight','JustifyBlock'],
			['Transformations'],
			'/',
			['Normal','H1','H2','H3','Hx','Styles'],
			['InsertMenu','MindTouchLink','Table','MindTouchImage','MindTouchTemplates','Video'],
			['TextColor','BGColor','-','RemoveFormat'],
			['Maximize']
		];

	config.toolbar_Basic =
		[
			['MindTouchSave','MindTouchCancel'],
			['ViewMenu'],
			['InsertMenu','MindTouchLink','Unlink','Table','MindTouchImage','MindTouchTemplates','Video'],
			['Maximize']
		];

	config.toolbar_ReadOnly =
		[
			['MindTouchCancel','-','Source'],
			['Maximize']
		];
	
	config.keystrokes =
		[
			[ CKEDITOR.ALT + 121 /*F10*/, 'toolbarFocus' ],
			[ CKEDITOR.ALT + 122 /*F11*/, 'elementsPathFocus' ],

			[ CKEDITOR.SHIFT + 121 /*F10*/, 'contextMenu' ],
			[ CKEDITOR.CTRL + CKEDITOR.SHIFT + 121 /*F10*/, 'contextMenu' ],

			[ CKEDITOR.CTRL + 90 /*Z*/, 'undo' ],
			[ CKEDITOR.CTRL + 89 /*Y*/, 'redo' ],
			[ CKEDITOR.CTRL + CKEDITOR.SHIFT + 90 /*Z*/, 'redo' ],

			[ CKEDITOR.CTRL + 66 /*B*/, 'bold' ],
			[ CKEDITOR.CTRL + 73 /*I*/, 'italic' ],
			[ CKEDITOR.CTRL + 85 /*U*/, 'underline' ],

			[ CKEDITOR.ALT + 109 /*-*/, 'toolbarCollapse' ],
			[ CKEDITOR.ALT + 48 /*0*/, 'a11yHelp' ],
			
			[ CKEDITOR.CTRL + 76 /*L*/, 'justifyleft' ],
			[ CKEDITOR.CTRL + 69 /*E*/, 'justifycenter' ],
			[ CKEDITOR.CTRL + 82 /*R*/, 'justifyright' ],
			[ CKEDITOR.CTRL + 74 /*J*/, 'justifyblock' ],
			
			[ CKEDITOR.CTRL + CKEDITOR.ALT + 13 /*ENTER*/, 'maximize' ]
		];
};
