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

var FCKInlineStyle = {
	
	GetNormalizedValue : function( el, property, attr )
	{
		var st = el.style, value = '' ;
		attr = attr || property ;
		
		switch ( property )
		{
			case 'backgroundImage':
				value = st.backgroundImage.replace(new RegExp("url\\('?([^']*)'?\\)", 'gi'), "$1") ;
				break ;
			case 'backgroundColor':
			case 'borderColor':
				if ( st[property].length > 0 )
				{
					value = this.ConvertRGBToHex(st[property]);
				}
				else if ( property == 'backgroundColor' && FCKDomTools.HasAttribute( el, 'bgColor' ) )
				{
					value = FCKDomTools.GetAttributeValue( el, 'bgColor' ) ;
				}
				else if ( property == 'borderColor' && FCKDomTools.HasAttribute( el, 'borderColor' ) )
				{
					value = FCKDomTools.GetAttributeValue( el, 'borderColor' ) ;
				}
				break ;
			case 'borderStyle':
				value = st.borderStyle ;

				if ( value.match(/([^\s]*)\s/) )
					value = RegExp.$1 ;

				break ;
			case 'borderWidth':
				value = FCKInlineStyle.GetNum( st.borderWidth ) ;
				break ;
			case 'whiteSpace':
				if ( st.whiteSpace.length > 0 )
				{
					value = st.whiteSpace ;
				}
				else
				{
					if ( el.attributes['noWrap'] != null && el.attributes['noWrap'].specified && oCell.noWrap )
						value = 'nowrap' ;
				}
				break ;
			default:
				value = FCKInlineStyle.GetValue( el, property, attr ) ;
		}
		
		return value ;
	},
	
	GetValue : function( el, property, attr )
	{
		var value = '', st = el.style ;
		attr = attr || property ;
		
		if ( FCKDomTools.HasAttribute( el, attr ) )
			value = FCKDomTools.GetAttributeValue( el, attr ) ;
		else if ( st[property] )
			value = st[property] ;
			
		return value ;
	},
	
	ConvertRGBToHex : function( col )
	{
		var re = new RegExp("rgb\\s*\\(\\s*([0-9]+).*,\\s*([0-9]+).*,\\s*([0-9]+).*\\)", "gi") ;
		
		var rgb = col.replace( re, "$1,$2,$3" ).split( ',' ) ;
		if ( rgb.length == 3 )
		{
			r = parseInt(rgb[0]).toString(16) ;
			g = parseInt(rgb[1]).toString(16) ;
			b = parseInt(rgb[2]).toString(16) ;
			
			r = r.length == 1 ? '0' + r : r ;
			g = g.length == 1 ? '0' + g : g ;
			b = b.length == 1 ? '0' + b : b ;
			
			return "#" + r + g + b ;
		}
		
		return col ;
	},
        
	ConvertHexToRGB : function( col )
	{
		if ( col.indexOf('#') != -1 )
		{
			col = col.replace( new RegExp('[^0-9A-F]', 'gi'), '' ) ;
			                
			r = parseInt(col.substring(0, 2), 16) ;
			g = parseInt(col.substring(2, 4), 16) ;
			b = parseInt(col.substring(4, 6), 16) ;
			
			return "rgb(" + r + "," + g + "," + b + ")" ;
		}
		
		return col ;
	},
	
	GetUnit : function( size )
	{
		return size.replace( /[0-9]+(px|%|in|cm|mm|em|ex|pt|pc)/, '$1' ) ;
	},
	
	GetNum : function( val )
	{
		val = parseInt( val ) ;

		if ( isNaN( val ) )
		{
			val = '' ;
		}
		
		return val ;
	}
	
}
