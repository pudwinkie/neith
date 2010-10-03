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

/* global Deki object */
if (typeof Deki == "undefined") {
	var Deki = {};
}
// setup our jQuery reference
Deki.$ = jQuery;

Deki.url = {};
Deki.url.encode = function(plaintext)
{
	// The Javascript escape and unescape functions do not correspond
	// with what browsers actually do...
	var SAFECHARS = "0123456789" +					// Numeric
					"ABCDEFGHIJKLMNOPQRSTUVWXYZ" +	// Alphabetic
					"abcdefghijklmnopqrstuvwxyz" +
					"-_.!~*'()";					// RFC2396 Mark characters
	var HEX = "0123456789ABCDEF";

	var encoded = "";
	for (var i = 0; i < plaintext.length; i++ ) {
		var ch = plaintext.charAt(i);
	    if (ch == " ") {
		    encoded += "+";				// x-www-urlencoded, rather than %20
		} else if (SAFECHARS.indexOf(ch) != -1) {
		    encoded += ch;
		} else {
		    var charCode = ch.charCodeAt(0);
			if (charCode > 255) {
				encoded += "+";
			} else {
				encoded += "%";
				encoded += HEX.charAt((charCode >> 4) & 0xF);
				encoded += HEX.charAt(charCode & 0xF);
			}
		}
	} // for

	return encoded;
}

Deki.util = {};
Deki.util.Dom = {};
Deki.util.Dom.getDimensions = function(element) {
	var region 	= YAHOO.util.Dom.getRegion(element);
	var width 	= region.right - region.left;
	var height 	= region.bottom - region.top;
	return {"width": width, "height": height};
}
Deki.util.Dom.getText = function(node) {
	if(typeof(node.innerText) != 'undefined') {
		return node.innerText;
	} else {
		return node.textContent;
	}
}
Deki.util.Dom.setInnerHTML = function (el, html) {
    el = YAHOO.util.Dom.get(el);
    if (!el || typeof html !== 'string') {
        return null;
    }

    // Break circular references.
    (function (o) {
        var a = o.attributes, i, l, n, c;
        if (a) {
            l = a.length;
            for (i = 0; i < l; i += 1) {
                n = a[i].name;
                if (typeof o[n] === 'function') {
                    o[n] = null;
                }
            }
        }
        a = o.childNodes;
        if (a) {
            l = a.length;
            for (i = 0; i < l; i += 1) {
                c = o.childNodes[i];

                // Purge child nodes.
                arguments.callee(c);

                // Removes all listeners attached to the element via YUI's addListener.
                YAHOO.util.Event.purgeElement(c);
            }
        }
    })(el);

    // Remove scripts from HTML string, and set innerHTML property
    el.innerHTML = html.replace(/<script[^>]*>((.|[\r\n])*?)<\\?\/script>/ig, "");

    // Return a reference to the first child
    return el.firstChild;
};
Deki.util.Dom.setInnerText = function(el, text) {
  if(typeof(el.innerText) != 'undefined') el.innerText = text;
  else el.textContent = text;
};
Deki.publish = function(c,d) { 
  if((name != null) && (name.indexOf("*") == -1)) Deki._query_store[c] = d;
  window.PageBus.publish(c,d); 
};
Deki.subscribe = function(c,o,f,d) { window.PageBus.subscribe(c,o,f,d) };
Deki._query_store = { };
Deki.query = function(c) { return Deki._query_store[c]; };
Deki.hasValue = function(v, d) { return (v != 'undefined') && (v != null) && (v != '') ? v : ((typeof d != 'undefined') ? d : null); };

Deki.Editor = null;

Deki.LoadEditor = function( sectionElement, action )
{
	if ( Deki.Editor )
	{
		var oEditor = Deki.Editor;
		
		if ( oEditor.IsLoading || oEditor.IsStarted )
		{
			return false;
		}
		
		if ( oEditor.SectionToEdit )
		{
			if ( !oEditor.Cancel() )
			{
				return false;
			}
		}
		
		// if we have a message, hide it
		Deki.$('#sessionMsg').hide();

		oEditor.IsLoading = true;
	}
	
	var oCurrentSection = '', oSectionToEdit = null;
	
	if ( !YAHOO.lang.isValue( sectionElement ) )
	{
		Deki.$('.hideforedit').hide();
	}
	else
	{
		oSectionToEdit = Deki.$( sectionElement ).parent().parent().parent(); // edit section
		oCurrentSection = oSectionToEdit.attr('id').substr(8); 
	}
	
	var callback = {
		success : function(o) {
			
			var oResponse = YAHOO.lang.JSON.parse( o.responseText );
			
			if ( YAHOO.lang.isArray(oResponse.scripts) )
			{
				YAHOO.util.Get.script( oResponse.scripts, {
					onSuccess : function() {
						
						if ( Deki.Editor )
						{
							Deki.Editor.Start(oResponse, oSectionToEdit, oCurrentSection);
						}
						
					}
				} );
			}
		},
		failure : function(o) { if ( Deki.Editor ) Deki.Editor.Cancel() }
	};
	
	var params = {}, url = [], param;
	
	if ( window.location.search.length > 0 )
	{
		var query = window.location.search.substring(1).split('&');

		for ( var i = 0 ; i < query.length ; i++ )
		{
			param = query[i].split('=');
			params[param[0]] = param[1] || '';
		}
	}
	
	if ( !params.text )
	{
		params.text = encodeURIComponent(Deki.PageTitle);
	}
		
	if ( !params.pageId )
	{
		params.pageId = encodeURIComponent(Deki.PageId);
	}

	if ( !params.sectionId )
	{
		params.sectionId = encodeURIComponent(oCurrentSection);
	}

	// Article::loadContent stops to work with this params in some cases
	params.action && delete params.action;
	params.diff && delete params.diff;
	params.revision && delete params.revision;

	if ( action && action == 'source' )
	{
		params.source = 'true';
	}

	for ( param in params )
	{
		url.push( param + '=' + params[param] );
	}
	
	url = '/deki/gui/loadeditor.php' + (( url.length > 0 ) ? '?' : '') + url.join('&');
	
	YAHOO.util.Connect.asyncRequest( 'GET', url, callback );

	return false;
};
