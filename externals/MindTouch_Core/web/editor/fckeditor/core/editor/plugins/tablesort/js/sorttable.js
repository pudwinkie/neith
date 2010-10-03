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

/*
 SortTable
 version 2
 7th April 2007
 Stuart Langridge, http://www.kryogenix.org/code/browser/sorttable/

 Instructions:
 Download this file
 Add <script src="sorttable.js"></script> to your HTML
 Add class="sortable" to any table you'd like to make sortable
 Click on the headers to sort

 Thanks to many, many people for contributions and suggestions.
 Licenced as X11: http://www.kryogenix.org/code/browser/licence.html
 This basically means: do what you want with it.
 */

var SortTable = function(oTable)
{
	if ( !oTable )
		return;
	
	this.oTable = oTable;
}

SortTable.prototype =
{
	Sort : function(sPart, aCols)
	{
		var sort = function(row1, row2)
		{
			var result ;
			
			for ( var i = 0 ; i < aCols.length ; i++ )
			{
				var oCol = aCols[i] ;
				
				switch ( oCol.type )
				{
					case 'numeric':
						result = SortTable.Sort.Numeric( row1[oCol.column], row2[oCol.column] ) ;
						break ;
					case 'date':
						result = SortTable.Sort.Date( row1[oCol.column], row2[oCol.column] ) ;
						break ;
					case 'alphanumeric':
					default:
						result = SortTable.Sort.Alpha( row1[oCol.column], row2[oCol.column] ) ;
						break ;
				}
				
				if ( oCol.order == 'desc' )
					result *= -1 ;
				
				if ( result == 0 )
					continue ;
				else
					break ;
			}
			
			return result ;
		};
	
		var aPart = [] ;
		
		switch ( sPart )
		{
			case 'thead': aPart.push( this.oTable.tHead ) ; break ;
			case 'tfoot': aPart.push( this.oTable.tFoot ) ; break ;
			case 'tbody':
			default:
				aPart = this.oTable.tBodies ;
				break ;
		}
		
		for ( var i = 0 ; i < aPart.length ; i++ )
		{
			var aRows = [], j ;
			
			for ( j = 0 ; j < aPart[i].rows.length ; j++ )
			{
				var aRow = [] ;
				
				for ( var k = 0 ; k < aPart[i].rows[j].cells.length ; k++ )
				{
					aRow.push( this.GetInnerText( aPart[i].rows[j].cells[k] ) ) ;
				}
				
				aRow.push( aPart[i].rows[j] ) ;
				aRows.push( aRow ) ;
			}
			
			aRows.sort( sort ) ;
			
			for ( j = 0 ; j < aRows.length ; j++ )
			{
				aPart[i].appendChild( aRows[j][aRows[j].length - 1] ) ;
			}
		}
	},
	
	/**
	 * gets the text we want to use for sorting for a cell.
	 * strips leading and trailing whitespace.
	 * this is *not* a generic getInnerText function; it's special to sorttable.
	 * for example, you can override the cell text with a customkey attribute.
	 * it also gets .value for <input> fields.
	 */
	GetInnerText : function( node )
	{
		hasInputs = (typeof node.getElementsByTagName == 'function')
				&& node.getElementsByTagName( 'input' ).length;
	
		if ( typeof node.textContent != 'undefined' && !hasInputs )
		{
			return node.textContent.replace( /^\s+|\s+$/g, '' );
		}
		else if ( typeof node.innerText != 'undefined' && !hasInputs )
		{
			return node.innerText.replace( /^\s+|\s+$/g, '' );
		}
		else if ( typeof node.text != 'undefined' && !hasInputs )
		{
			return node.text.replace( /^\s+|\s+$/g, '' );
		}
		else
		{
			switch ( node.nodeType )
			{
				case 3 :
					if ( node.nodeName.toLowerCase() == 'input' )
					{
						return node.value.replace( /^\s+|\s+$/g, '' );
					}
				case 4 :
					return node.nodeValue.replace( /^\s+|\s+$/g, '' );
					break;
				case 1 :
				case 11 :
					var innerText = '';
					for ( var i = 0 ; i < node.childNodes.length ; i++ )
					{
						innerText += this.getInnerText( node.childNodes[i] );
					}
					return innerText.replace( /^\s+|\s+$/g, '' );
					break;
				default :
					return '';
			}
		}
	}	
};

/**
 * sort functions each sort function takes two parameters, a and b you are
 * comparing a[0] and b[0]
 */
SortTable.Sort =
{
	Numeric : function(a, b)
	{
		var aa = parseFloat( a.replace( /[^0-9.-]/g, '' ) );
		
		if ( isNaN( aa ) )
			aa = 0;
		
		var bb = parseFloat( b.replace( /[^0-9.-]/g, '' ) );
		
		if ( isNaN( bb ) )
			bb = 0;
		
		return aa - bb;
	},
	
	Alpha : function(a, b)
	{
		if ( a == b )
			return 0;
		
		if ( a < b )
			return -1;
		
		return 1;
	},
	
	Date : function(a, b)
	{
		var oDateA = new Date( a ) ;
		var oDateB = new Date( b ) ;
		
		var nDateA = oDateA.getTime() ;
		var nDateB = oDateB.getTime() ;
		
		if ( isNaN( nDateA ) || isNaN( nDateB ) || nDateA == nDateB )
			return 0 ;
		
		return nDateA - nDateB ;
	}
}
