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
	var pluginPath = FCKConfig.PluginsPath + 'tableoperations/' ;
	
	FCKCommands.RegisterCommand('Table', new FCKDialogCommand('Table', FCKLang.DlgInsertTableTitle, pluginPath + 'fck_table.html', 500, 330));
	FCKCommands.RegisterCommand('TableProp', new FCKDialogCommand('Table', FCKLang.DlgTableTitle, pluginPath + 'fck_table.html?Parent', 500, 330));
	FCKCommands.RegisterCommand('TableCellProp', new FCKDialogCommand('TableCell', FCKLang.DlgCellTitle, pluginPath + 'fck_tablecell.html', 550, 300));
	FCKCommands.RegisterCommand('TableRowProp', new FCKDialogCommand('TableRow', FCKLang.DlgRowTitle, pluginPath + 'fck_tablerow.html', 450, 290));
	
	FCKToolbarItems.RegisterItem( 'TableRowProp', new FCKToolbarButton( 'TableRowProp', FCKLang.RowProperties, null, null, null, true, pluginPath + 'fck_rowprop.gif' ) ) ;
	
	FCK.ContextMenu.RegisterListener( {
		AddItems: function(menu, tag, tagName){
			var bIsTable = (tagName == 'TABLE');
			var bIsCell = (!bIsTable && FCKSelection.HasAncestorNode('TABLE'));
			
			if (bIsCell) {
				for (var i = 0; i < menu._MenuBlock._Items.length; i++) {
					var oItem = menu._MenuBlock._Items[i];
					if (oItem.Name == 'Row') {
						oItem.AddSeparator();
						oItem.AddItem('TableRowProp', FCKLang.RowProperties, pluginPath + 'fck_rowprop.gif', FCKCommands.GetCommand('TableRowProp').GetState() == FCK_TRISTATE_DISABLED);
					}
				}
			}
		}
	}) ;
	
	if ( !FCKCodeFormatter.Regex )
		FCKCodeFormatter.Init() ;
		
	// Added TFOOT
	FCKCodeFormatter.Regex.MainTags = /\<\/?(HTML|HEAD|BODY|FORM|TABLE|TBODY|THEAD|TFOOT|TR)[^\>]*\>/gi ;
	FCKCodeFormatter.Regex.IncreaseIndent = /^\<(HTML|HEAD|BODY|FORM|TABLE|TBODY|THEAD|TFOOT|TR|UL|OL|DL)[ \/\>]/i ;
	FCKCodeFormatter.Regex.DecreaseIndent = /^\<\/(HTML|HEAD|BODY|FORM|TABLE|TBODY|THEAD|TFOOT|TR|UL|OL|DL)[ \>]/i ;
	
})() ;
