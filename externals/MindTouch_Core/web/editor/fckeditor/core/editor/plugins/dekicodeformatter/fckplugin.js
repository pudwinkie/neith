/*
 * FCKeditor - The text editor for Internet - http://www.fckeditor.net
 * Copyright (C) 2003-2009 Frederico Caldeira Knabben
 *
 * == BEGIN LICENSE ==
 *
 * Licensed under the terms of any of the following licenses at your
 * choice:
 *
 *  - GNU General Public License Version 2 or later (the "GPL")
 *    http://www.gnu.org/licenses/gpl.html
 *
 *  - GNU Lesser General Public License Version 2.1 or later (the "LGPL")
 *    http://www.gnu.org/licenses/lgpl.html
 *
 *  - Mozilla Public License Version 1.1 or later (the "MPL")
 *    http://www.mozilla.org/MPL/MPL-1.1.html
 *
 * == END LICENSE ==
 *
 * Format the HTML.
 */

/*

Style HTML
---------------


Written by Nochum Sossonko, (nsossonko@hotmail.com)
	$Date$
	$Revision$

Based on code initially developed by: Einars "elfz" Lielmanis, <elfz@laacz.lv>
	http://elfz.laacz.lv/beautify/


You are free to use this in any way you want, in case you find this useful or working for you.

Usage:
	style_html(html_source);

*/

/*

JS Beautifier
---------------
$Date$
$Revision$


Written by Einars Lielmanis, <einars@gmail.com>
	http://elfz.laacz.lv/beautify/

Originally converted to javascript by Vital, <vital76@gmail.com>
	http://my.opera.com/Vital/blog/2007/11/21/javascript-beautify-on-javascript-translated


You are free to use this in any way you want, in case you find this useful or working for you.

Usage:
	js_beautify(js_source_text);
	js_beautify(js_source_text, options);

The options are:
	indent_size (default 4) — indentation size,
	indent_char (default space) — character to indent with,
	preserve_newlines (default true) — whether existing line breaks should be preserved,
	indent_level (default 0)  — initial indentation level, you probably won't need this ever,

	e.g

	js_beautify(js_source_text, {indent_size: 1, indent_char: '\t'});


*/

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

var FCKCodeFormatter = new Object() ;

FCKCodeFormatter.Init = function()
{
	var oRegex = this.Regex = new Object() ;

	// Regex for line breaks.
	oRegex.BlocksOpener = /\<(P|DIV|H1|H2|H3|H4|H5|H6|ADDRESS|PRE|OL|UL|LI|DL|DT|DD|TITLE|META|LINK|BASE|SCRIPT|LINK|TD|TH|AREA|OPTION)[^\>]*\>/gi ;
	oRegex.BlocksCloser = /\<\/(P|DIV|H1|H2|H3|H4|H5|H6|ADDRESS|PRE|OL|UL|LI|DL|DT|DD|TITLE|META|LINK|BASE|SCRIPT|LINK|TD|TH|AREA|OPTION)[^\>]*\>/gi ;

	oRegex.NewLineTags	= /\<(BR|HR)[^\>]*\>/gi ;

	oRegex.MainTags = /\<\/?(HTML|HEAD|BODY|FORM|TABLE|TBODY|THEAD|TR)[^\>]*\>/gi ;

	oRegex.LineSplitter = /\s*\n+\s*/g ;

	// Regex for indentation.
	oRegex.IncreaseIndent = /^\<(HTML|HEAD|BODY|FORM|TABLE|TBODY|THEAD|TR|UL|OL|DL)[ \/\>]/i ;
	oRegex.DecreaseIndent = /^\<\/(HTML|HEAD|BODY|FORM|TABLE|TBODY|THEAD|TR|UL|OL|DL)[ \>]/i ;
	oRegex.FormatIndentatorRemove = new RegExp( '^' + FCKConfig.FormatIndentator ) ;

	oRegex.ProtectedTags = /(<PRE[^>]*>)([\s\S]*?)(<\/PRE>)/gi ;
}

FCKCodeFormatter._ProtectData = function( outer, opener, data, closer )
{
	return opener + '___FCKpd___' + ( FCKCodeFormatter.ProtectedData.push( data ) - 1 ) + closer ;
}

FCKCodeFormatter.Format = function( html )
{
	if ( !this.Regex )
		this.Init() ;

	// Protected content that remain untouched during the
	// process go in the following array.
	FCKCodeFormatter.ProtectedData = new Array() ;

	var sFormatted = html.replace( this.Regex.ProtectedTags, FCKCodeFormatter._ProtectData ) ;
	
	if ( FCKConfig.BeautifySource )
	{
		var Beautifier = new FCKCodeFormatter.HTMLBeautify( sFormatted, {
				'IndentChar' : FCKConfig.FormatIndentator,
				'IndentSize' : 1
		} ) ;
		
		sFormatted = Beautifier.GetFormattedCode() ;
	}
	else
	{
		// Line breaks.
		sFormatted		= sFormatted.replace( this.Regex.BlocksOpener, '\n$&' ) ;
		sFormatted		= sFormatted.replace( this.Regex.BlocksCloser, '$&\n' ) ;
		sFormatted		= sFormatted.replace( this.Regex.NewLineTags, '$&\n' ) ;
		sFormatted		= sFormatted.replace( this.Regex.MainTags, '\n$&\n' ) ;
	
		// Indentation.
		var sIndentation = '' ;
	
		var asLines = sFormatted.split( this.Regex.LineSplitter ) ;
		sFormatted = '' ;
	
		for ( var i = 0 ; i < asLines.length ; i++ )
		{
			var sLine = asLines[i] ;
	
			if ( sLine.length == 0 )
				continue ;
	
			if ( this.Regex.DecreaseIndent.test( sLine ) )
				sIndentation = sIndentation.replace( this.Regex.FormatIndentatorRemove, '' ) ;
	
			sFormatted += sIndentation + sLine + '\n' ;
	
			if ( this.Regex.IncreaseIndent.test( sLine ) )
				sIndentation += FCKConfig.FormatIndentator ;
		}
	}

	// Now we put back the protected data.
	for ( var j = 0 ; j < FCKCodeFormatter.ProtectedData.length ; j++ )
	{
		var oRegex = new RegExp( '___FCKpd___' + j ) ;
		sFormatted = sFormatted.replace( oRegex, FCKCodeFormatter.ProtectedData[j].replace( /\$/g, '$$$$' ) ) ;
	}
	
	return sFormatted.Trim() ;
}

FCKCodeFormatter.HTMLBeautify = function( html, options )
{
	this.Options = options || {} ;
	
	this.Html = html || '' ;
	this.Output = [] ;

	this.IndentCharacter = options['IndentChar'] || ' ' ;
	this.IndentSize = options['IndentSize'] || 4 ;

	this.IndentString = '' ;
	this.IndentLevel = options['IndentLevel'] || 0 ;
	this.MaxChar = options['LineLength'] || 0 ; // maximum amount of characters per line, 0 - no limit
	this.LineCharCount = 0 ; // count to see if MaxChar was exceeded

	for ( var i=0 ; i < this.IndentSize ; i++ )
	{
		this.IndentString += this.IndentCharacter ;
	}
	
	this.Pos = 0 ; // Parser position
	this.Token = '' ;
	this.CurrentMode = 'CONTENT' ; // reflects the current Parser mode: TAG/CONTENT

	// An object to hold tags, their position, and their parent-tags,
	// initiated with default values
	this.Tags =
	{
		'parent': 'parent1',
		'parentcount': 1,
		'parent1': ''
	};
	
	this.TagType = '' ;
	this.TokenText = this.LastToken = this.LastText = this.TokenType = '' ;
	
	//Uilities made available to the various functions
	this.Utils =
	{
		'Whitespace': "\n\r\t ".split(''),
		'SingleToken': 'br,input,link,meta,!doctype,basefont,base,area,hr,wbr,param,img,isindex,?xml,embed'.split(','), //all the single tags for HTML
		'ExtraLiners': 'head,body,/html'.split(',') //for tags that need a line of whitespace before them
	}
}

FCKCodeFormatter.HTMLBeautify.prototype =
{
	TagName : null,
	ForceNewLine : false,
	
	GetFormattedCode : function()
	{
		while ( true )
		{
			var Beautifier = this ;

			var t = this.GetToken() ;

			this.TokenText = t[0] ;
			this.TokenType = t[1] ;
	
			if ( this.TokenType === 'TK_EOF' )
			{
				break ;
			}
	
			switch ( this.TokenType )
			{
				case 'TK_TAG_START':
				case 'TK_TAG_SCRIPT':
				case 'TK_TAG_DEKISCRIPT':
				case 'TK_TAG_STYLE':
					var tagName = this.GetTagName( this.TokenText ) ;
					var isBlock = ( FCKListsLib.InlineNonEmptyElements[tagName] == null ) ;

					if ( isBlock || this.ForceNewLine )
					{
						this.PrintNewline( false ) ;
						this.Indent() ;
					}
					
					this.TokenText = this.TokenText.ReplaceNewLineChars( '' ) ;
					this.TokenText = this.TokenText.replace( /\s{1}(init|block|if|foreach|ctor)="(.+?)"/ig, function( str, attrName, attrVal, offset, s ) {
						return ' ' + attrName + '="' + Beautifier.FormatDekiScript.apply( Beautifier, [attrVal, false] ) + '"' ;
					} ) ;
					
					this.PrintToken( this.TokenText ) ;
					
					this.ForceNewLine = false ;
					this.CurrentMode = 'CONTENT' ;
					break ;


				case 'TK_TAG_END':
					// don't print new line after content or inline tags
					if ( this.ForceNewLine )
					{
						this.PrintNewline( true ) ;
					}
					
					this.PrintToken( this.TokenText ) ;
					
					var tagName = this.GetTagName( this.TokenText ) ;
					var isBlock = ( FCKListsLib.InlineNonEmptyElements[tagName] == null ) ;

					// put next TAG_END to new line only after closed block tag
					this.ForceNewLine = isBlock ;
					this.CurrentMode = 'CONTENT' ;
					
					break ;


				case 'TK_TAG_SINGLE':
					var tagName = this.GetTagName( this.TokenText ) ;
					var re = new RegExp( FCKCodeFormatter.Regex.NewLineTags ) ;
					
					// new line only after br/hr and no new lines before/after img
					if ( ! (re.test( this.TokenText ) || tagName == 'img') || this.ForceNewLine )
					{
						this.PrintNewline( false ) ;
					}
					
					this.PrintToken( this.TokenText ) ;
					this.ForceNewLine = ( tagName != 'img' ) ; // img is inline
					this.CurrentMode = 'CONTENT' ;
					break ;


				case 'TK_CONTENT' :
					if ( this.TokenText !== '' )
					{
						if ( this.ForceNewLine )
						{
							this.PrintNewline( true ) ;
							this.TokenText = this.TokenText.Trim() ;
						}

						if ( this.TokenText.indexOf('{{') != -1 )
						{
							this.TokenText = this.TokenText.replace( /{{([\s\S]*?)}}/g, function( str, source, offset, s ) {
								return Beautifier.FormatDekiScript.apply( Beautifier, [source, true] ) ;
							} ) ;
							
							if ( this.ForceNewLine )
							{
								this.TokenText = this.TokenText.replace( /}}([\s\S]*?){{/g, function( str, source, offset, s ) {
									return Beautifier.FormatMultiScripts.call( Beautifier, source ) ;
								} ) ;
							}

							this.ForceNewLine = this.ForceNewLine && ( this.TokenText.lastIndexOf('}}') == this.TokenText.length - 2 ) ;
						}
						
						this.PrintToken( this.TokenText ) ;
					}
					
					this.CurrentMode = 'TAG' ;
					break ;
			}
			
			this.LastToken = this.TokenType ;
			this.LastText = this.TokenText ;
		}
		
		if ( this.Output.length )
		{
			// start indentation
			for ( var i = 0 ; i < this.Options['IndentLevel'] ; i++ )
			{
				this.Output.unshift( this.IndentString ) ;
			}
		}
	
		return this.Output.join('') ;
	},
	
	FormatDekiScript : function( source, isBlock )
	{
			var ScriptBeautifier = new FCKCodeFormatter.ScriptBeautify(source, {
				'IndentSize'  : this.IndentSize,
				'IndentChar'  : this.IndentCharacter,
				'IndentLevel' : this.IndentLevel + 1
			}) ;
			var DekiScript = ScriptBeautifier.GetFormattedCode() ;
			var isMultiLine = ScriptBeautifier.LineBreaksCount > 0 ;
			
			var script = [ '' ] ; // should contain one ore more elements for PrintNewline
			
			if ( !this.ForceNewLine && isMultiLine && isBlock )
			{
				this.PrintNewline( false, script ) ;
			}
			
			if ( isBlock )
				script.push( '{{' ) ;
			
			if ( isMultiLine )
			{
				this.Indent() ;
				this.PrintNewline( false, script ) ;
				this.Outdent() ;
			}
			else if ( isBlock )
			{
				script.push(' ') ;
			}
			
			script.push( DekiScript ) ;
			
			if ( isMultiLine )
			{
				this.PrintNewline( false, script ) ;
			}
			else if ( isBlock )
			{
				script.push(' ') ;
			}
			
			if ( isBlock )
				script.push( '}}' ) ;

			if ( isMultiLine && isBlock )			
				this.ForceNewLine = true ;

			return script.join('') ;
	},
	
	/*
	 * Formats content between }} and {{
	 * 
	 */
	FormatMultiScripts : function( source )
	{
		var script = [] ;
		script.push( '}}' ) ;
		
		this.PrintNewline( false, script ) ;
		
		if ( source.Trim() != '' )
		{
			script.push( source ) ;
			this.PrintNewline( false, script ) ;
		}
		
		script.push( '{{' ) ;
		
		return script.join('') ;
	},
	
	// function to capture regular content between tags
	GetContent : function()
	{
		var sChar = '' ;
		var content = [] ;
		var space = false ; // if a space is needed
		var isEmpty = true ;
		
		var DekiScriptFlag = false ;
		
		while ( this.Html.charAt( this.Pos ) !== '<' || DekiScriptFlag )
		{
			if ( this.Pos >= this.Html.length )
			{
				return content.length ? content.join('') : ['', 'TK_EOF'] ;
			}

			sChar = this.Html.charAt( this.Pos ) ;
			this.Pos++ ;
			this.LineCharCount++ ;

			if ( this.Utils.Whitespace.IndexOf( sChar ) != -1 )
			{
				space = true ;
				this.LineCharCount-- ;
				continue ; // don't want to insert unnecessary space
			}
			else if ( space )
			{
				if ( this.MaxChar > 0 && this.LineCharCount >= this.MaxChar )
				{
					// insert a line when the MaxChar is reached
					content.push('\n') ;
					
					for ( var i = 0 ; i < this.IndentLevel ; i++ )
					{
						content.push( this.IndentString ) ;
					}
					
					this.LineCharCount = 0 ;
				}
				else
				{
					content.push(' ') ;
					this.LineCharCount++ ;
				}
				space = false ;
			}
			
			if ( sChar == '{' )
			{
				if ( this.Html.charAt( this.Pos ) == '{' )
				{
					DekiScriptFlag = true ;
				}
			}
			else if ( sChar == '}' )
			{
				if ( this.Html.charAt( this.Pos ) == '}' )
				{
					DekiScriptFlag = false ;
				}
			}

			content.push( sChar ) ; // letter at-a-time (or string) inserted to an array
			isEmpty = false ;
		}
		
		if ( space )
		{
			// add space if it is at the end of the content
			content.push(' ') ;
		}
		
		return !isEmpty ? content.join('') : '' ;
	},

	// get the full content of a javascript to pass to ScriptBeautify
	GetScript : function()
	{
		var sChar = '' ;
		var content = [] ;
		
		var re = new RegExp( '\<\/script' + '\>', 'igm' ) ;
		re.lastIndex = this.Pos ;
		
		var reArray = re.exec( this.Html ) ;
		var endScript = reArray ? reArray.index : this.Html.length ; // absolute end of script

		// get everything in between the script tags
		while ( this.Pos < endScript )
		{
			if ( this.Pos >= this.Html.length )
			{
				return content.length ? content.join('') : ['', 'TK_EOF'] ;
			}

			sChar = this.Html.charAt( this.Pos ) ;
			this.Pos++ ;

			content.push( sChar ) ;
		}

		return content.length ? content.join('') : '' ; // we might not have any content at all
	},

	// function to record a tag and its parent in this.Tags Object
	RecordTag : function( tag )
	{
		if ( this.Tags[tag + 'count'] )
		{
			//check for the existence of this tag type
			this.Tags[tag + 'count']++ ;
			// and record the present indent level
			this.Tags[tag + this.Tags[tag + 'count']] = this.IndentLevel ;
		}
		else
		{
			//otherwise initialize this tag type
			this.Tags[tag + 'count'] = 1 ;
			// and record the present indent level
			this.Tags[tag + this.Tags[tag + 'count']] = this.IndentLevel ;
		}

		//set the parent (i.e. in the case of a div this.Tags.div1parent)
		this.Tags[tag + this.Tags[tag + 'count'] + 'parent'] = this.Tags.parent ;
		//and make this the current parent (i.e. in the case of a div 'div1')
		this.Tags.parent = tag + this.Tags[tag + 'count'] ;
	},

	// function to retrieve the opening tag to the corresponding closer
	RetrieveTag : function( tag )
	{
		if ( this.Tags[tag + 'count'] )
		{
			// if the openener is not in the Object we ignore it
			// check to see if it's a closable tag.
			var temp_parent = this.Tags.parent ;

			while ( temp_parent )
			{
				// till we reach '' (the initial value)
				if ( tag + this.Tags[tag + 'count'] === temp_parent )
				{
					// if this is it use it
					break ;
				}
				
				// otherwise keep on climbing up the DOM Tree
				temp_parent = this.Tags[temp_parent + 'parent'] ;
			}

			// if we caught something
			if ( temp_parent )
			{
				// set the IndentLevel accordingly
				this.IndentLevel = this.Tags[tag + this.Tags[tag + 'count']] ;
				// and set the current parent
				this.Tags.parent = this.Tags[temp_parent + 'parent'] ;
			}

			// delete the closed tags parent reference...
			delete this.Tags[tag + this.Tags[tag + 'count'] + 'parent'] ;
			// ...and the tag itself
			delete this.Tags[tag + this.Tags[tag + 'count']];

			if ( this.Tags[tag + 'count'] == 1 )
			{
				delete this.Tags[tag + 'count'] ;
			}
			else
			{
				this.Tags[tag + 'count']-- ;
			}
		}
	},

	GetTag : function()
	{
		//function to get a full tag and parse its type
		var sChar = '' ;
		var content = [] ;
		var space = false ;

		do
		{
			if ( this.Pos >= this.Html.length )
			{
				return content.length ? content.join('') : ['', 'TK_EOF'] ;
			}

			sChar = this.Html.charAt( this.Pos ) ;
			
			this.Pos++ ;
			this.LineCharCount++ ;

			if ( this.Utils.Whitespace.IndexOf( sChar ) != -1 )
			{
				//don't want to insert unnecessary space
				space = true ;
				this.LineCharCount-- ;
				continue ;
			}

			if ( sChar === "'" || sChar === '"' )
			{
				if ( !content[1] || content[1] !== '!' )
				{
					//if we're in a comment strings don't get treated specially
					sChar += this.GetUnformatted( sChar ) ;
					space = true ;
				}
			}

			if ( sChar === '=' )
			{
				//no space before =
				space = false ;
			}

			if ( content.length && content[content.length-1] !== '=' && sChar !== '>' && space )
			{
				//no space after = or before >
				if ( this.MaxChar > 0 && this.LineCharCount >= this.MaxChar )
				{
					this.PrintNewline( false, content ) ;
					this.LineCharCount = 0 ;
				}
				else
				{
					content.push(' ') ;
					this.LineCharCount++ ;
				}
				
				space = false ;
			}

			//inserts character at-a-time (or string)
			content.push( sChar ) ;
			
		} while ( sChar !== '>' ) ;

		var tagComplete = content.join('') ;
		var tagIndex ;

		if ( tagComplete.indexOf(' ') != -1 )
		{
			// if there's whitespace, thats where the tag name ends
			tagIndex = tagComplete.indexOf(' ') ;
		}
		else
		{
			// otherwise go with the tag ending
			tagIndex = tagComplete.indexOf('>') ;
		}

		this.TagName = tagComplete.substring( 1, tagIndex ).toLowerCase() ;

		if ( tagComplete.charAt(tagComplete.length-2) === '/' || this.Utils.SingleToken.IndexOf( this.TagName ) != -1 )
		{

			// if this tag name is a single tag type (either in the list or has a closing /)
			this.TagType = 'SINGLE' ;

		}
		else if ( this.TagName === 'script' )
		{

			// for later script handling
			this.RecordTag( this.TagName ) ;
			this.TagType = 'SCRIPT' ;

		}
		else if ( this.TagName === 'style' )
		{

			// for future style handling (for now it justs uses GetContent)
			this.RecordTag( this.TagName ) ;
			this.TagType = 'STYLE' ;

		}
		else if ( this.TagName.charAt(0) === '!' )
		{

			// peek for <!-- comment
			if ( this.TagName.indexOf('[if') != -1 )
			{
				// peek for <!--[if conditional comment
				if ( tagComplete.indexOf('!IE') != -1 )
				{
					// this type needs a closing --> so...
					// ...delegate to GetUnformatted
					var sComment = this.GetUnformatted( '-->', tagComplete ) ;
					content.push( sComment ) ;
				}

				this.TagType = 'START' ;

			}
			else if ( this.TagName.indexOf('[endif') != -1 )
			{

				// peek for <!--[endif end conditional comment
				this.TagType = 'END' ;
				this.Outdent() ;

			}
			else if ( this.TagName.indexOf('[cdata[') != -1 )
			{

				// if it's a <[cdata[ comment...
				// ...delegate to GetUnformatted function
				var sComment = this.GetUnformatted( ']]>', tagComplete ) ;
				content.push( sComment ) ;
				
				// <![CDATA[ comments are treated like single tags
				this.TagType = 'SINGLE' ;

			}
			else
			{

				var sComment = this.GetUnformatted( '-->', tagComplete ) ;
				content.push( sComment ) ;
				this.TagType = 'SINGLE' ;

			}

		}
		else
		{

			if ( this.TagName.charAt(0) === '/' )
			{
				// this tag is a double tag so check for tag-ending
				this.RetrieveTag( this.TagName.substring(1) ) ; // remove it and all ancestors
				this.TagType = 'END';
			}
			else
			{
				// otherwise it's a start-tag
				this.RecordTag( this.TagName ) ; // push it on the tag stack
				this.TagType = 'START';
			}

			if ( this.Utils.ExtraLiners.IndexOf( this.TagName ) != -1 )
			{
				// check if this double needs an extra line
				this.PrintNewline( true ) ;
			}

		}
		
		return content.join('') ; // returns fully formatted tag
	},

	// function to return unformatted content in its entirety
	GetUnformatted : function( delimiter, orig_tag )
	{
		if ( orig_tag && orig_tag.indexOf(delimiter) != -1 )
		{
			return '' ;
		}

		var sChar = '' ;
		var content = '' ;
		var space = true ;

		do
		{
			sChar = this.Html.charAt( this.Pos ) ;
			this.Pos++ ;

			if ( this.Utils.Whitespace.IndexOf( sChar ) != -1 )
			{
				if ( !space )
				{
					this.LineCharCount-- ;
					continue ;
				}

				if ( sChar === '\n' || sChar === '\r' )
				{
					content += '\n' ;

					for ( var i = 0 ; i < this.IndentLevel ; i++ )
					{
						content += this.IndentString ;
					}

					space = false ; // ...and make sure other indentation is erased
					this.LineCharCount = 0 ;
					continue ;
				}
			}

			content += sChar ;
			this.LineCharCount++ ;
			space = true ;

		} while ( content.indexOf( delimiter ) == -1 ) ;

		return content ;
	},

	// initial handler for token-retrieval
	GetToken : function()
	{
		var token ;

		if ( this.LastToken === 'TK_TAG_SCRIPT' )
		{
			// check if we need to format script
			var temp_token = this.GetScript() ;

			if ( typeof temp_token !== 'string' )
			{
				return temp_token ;
			}

			// call the Script Beautifier
			var Beautifier = new FCKCodeFormatter.ScriptBeautify(temp_token, {
				'IndentSize'  : this.IndentSize,
				'IndentChar'  : this.IndentCharacter,
				'IndentLevel' : this.IndentLevel
			}) ;
			
			token = Beautifier.GetFormattedCode() ;
			
			return [token, 'TK_CONTENT'] ;
		}

		if ( this.CurrentMode === 'CONTENT' )
		{
			token = this.GetContent() ;
			
			if ( typeof token !== 'string' )
			{
				return token ;
			}
			else
			{
				return [token, 'TK_CONTENT'] ;
			}
		}

		if( this.CurrentMode === 'TAG' )
		{
			token = this.GetTag() ;
			
			if ( typeof token !== 'string' )
			{
				return token ;
			}
			else
			{
				var tagNameType = 'TK_TAG_' + this.TagType ;
				return [token, tagNameType] ;
			}
		}
	},
	
	GetTagName : function( token )
	{
		var re = new RegExp(/<[\/]?([A-Za-z]+?[A-Za-z0-9]*?)(?=\s|\/|>)/i) ;
		var result = re.exec(token) ;
		
		var tag = ( result ) ? result[1] : '' ;
		
		return tag.toLowerCase() ;
	},
	
	// handles input/output and some other printing functions
	PrintNewline : function( ignore, content )
	{
		this.LineCharCount = 0 ;

		content = content || this.Output ;
		
		if ( !content || !content.length )
		{
			return ;
		}

		if ( !ignore )
		{
			// we might want the extra line
			while ( this.Utils.Whitespace.IndexOf( content[content.length-1] ) != -1 )
			{
				content.pop() ;
			}
		}

		content.push('\n') ;

		for ( var i=0 ; i < this.IndentLevel ; i++ )
		{
			content.push( this.IndentString ) ;
		}
	},

	PrintToken : function( text )
	{
		this.Output.push( text ) ;
	},

	Indent : function()
	{
		this.IndentLevel++ ;
	},

	Outdent : function()
	{
		if (this.IndentLevel > 0)
		{
			this.IndentLevel-- ;
		}
	}
}

FCKCodeFormatter.ScriptBeautify = function( source, options )
{
	this.Input = source ;
	this.IndentString = '' ;
	
	this.Options = options || {} ;

	var IndentSize = options['IndentSize'] || 4 ;
	var IndentChar = options['IndentChar'] || ' ' ;
	
	while ( IndentSize-- )
	{
		this.IndentString += IndentChar ;
	}

	this.PreserveNewlines =
		typeof options['PreserveNewlines'] === 'undefined' ? true : options['PreserveNewlines'] ;

	this.IndentLevel = options['IndentLevel'] || 0 ; // starting indentation
	
	this.LastWord = '' ; // last 'TK_WORD' passed
	this.LastType = 'TK_START_EXPR' ; // last token type
	this.LastText = '' ; // last token text
	this.Output = [] ;

	this.DoBlockJustClosed = false ;
	this.VarLine = false ; // currently drawing var .... ;
	this.VarLineTainted = false ; // false: var a = 5; true: var a = 5, b = 6
	
	this.LineBreaksCount = 0 ;

	this.Whitespace = "\n\r\t ".split('') ;
	this.Wordchar = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_$'.split('') ;
	this.Punct = '+ - * / % & ++ -- = += -= *= /= %= == === != !== > < >= <= >> << >>> >>>= >>= <<= && &= | || ! !! , : ? ?? ^ ^= |= :: . .. ..='.split(' ') ;

	// words which should always start on new line.
	this.LineStarters = 'continue,try,throw,return,var,if,switch,case,default,for,while,break,function,foreach,let'.split(',') ;

	// states showing if we are currently in expression (i.e. "if" case) - 'EXPRESSION',
	// or in usual block (like, procedure), 'BLOCK'.
	// some formatting depends on that.
	this.CurrentMode = 'BLOCK' ;
	this.Modes = [this.CurrentMode] ;
}

FCKCodeFormatter.ScriptBeautify.prototype =
{
	TokenText : null,
	TokenType : null,
	
	ParserPos : 0,

	// flag for parser that case/default has been processed,
	// and next colon needs special attention
	InCase : false,
	
	IfLineFlag : false,
	
	Prefix : null,
	
	GetFormattedCode : function()
	{
		while (true)
		{
			var t = this.GetNextToken() ;
			
			this.TokenText = t[0] ;
			this.TokenType = t[1] ;
			
			if ( this.TokenType === 'TK_EOF' )
			{
				break ;
			}
			
			switch ( this.TokenType )
			{
				case 'TK_START_EXPR':
					this.VarLine = false ;
					this.SetMode( 'EXPRESSION' ) ;
					
					if ( this.LastType === 'TK_END_EXPR' || this.LastType === 'TK_START_EXPR' )
					{
						// do nothing on (( and )( and ][ and ]( ..
					}
					else if ( this.LastType !== 'TK_WORD' && this.LastType !== 'TK_OPERATOR' )
					{
						this.PrintSpace() ;
					}
					else if ( this.InArray(this.LastWord, this.LineStarters) && this.LastWord !== 'function' )
					{
						this.PrintSpace() ;
					}
					
					this.PrintToken() ;
					break ;


				case 'TK_END_EXPR':
					this.PrintToken() ;
					this.RestoreMode() ;
					break ;


				case 'TK_START_BLOCK':
					if ( this.LastWord === 'do' )
					{
						this.SetMode( 'DO_BLOCK' ) ;
					}
					else if ( !this.IsExpression() )
					{
						this.SetMode( 'BLOCK' ) ;
					}
					
					if ( this.LastType !== 'TK_OPERATOR' && this.LastType !== 'TK_START_EXPR' )
					{
						if ( this.LastType === 'TK_START_BLOCK' )
						{
							this.PrintNewline() ;
						}
						else
						{
							this.PrintSpace() ;
						}
					}
					
					this.PrintToken() ;
					
					if ( !this.IsExpression() )
					{
						this.Indent() ;
					}
					
					break ;


				case 'TK_END_BLOCK':
					if ( this.LastType === 'TK_START_BLOCK' )
					{
						// nothing
						this.TrimOutput() ;
						this.Outdent() ;
					}
					else if ( this.IsExpression() )
					{
						this.PrintSpace() ;
					}
					else
					{
						this.Outdent() ;
						this.PrintNewline() ;
						this.RestoreMode() ;
					}
					
					this.PrintToken() ;
					break;


				case 'TK_WORD':
					if ( this.DoBlockJustClosed )
					{
						this.PrintSpace() ;
						this.PrintToken() ;
						this.PrintSpace() ;
						break ;
					}
		
					if ( this.TokenText === 'case' || this.TokenText === 'default' )
					{
						if ( this.LastText === ':' )
						{
							// switch cases following one another
							this.RemoveIndent() ;
						}
						else
						{
							// case statement starts in the same line where switch
							this.Outdent() ;
							this.PrintNewline() ;
							this.Indent() ;
						}
						
						this.PrintToken() ;
						this.InCase = true ;
						break ;
					}
		
					this.Prefix = 'NONE' ;
					
					if ( this.LastType === 'TK_END_BLOCK' )
					{
						if ( !this.InArray(this.TokenText.toLowerCase(), ['else', 'catch', 'finally']) )
						{
							this.Prefix = 'NEWLINE' ;
						}
						else
						{
							this.Prefix = 'SPACE' ;
							this.PrintSpace() ;
						}
					}
					else if ( this.LastType === 'TK_SEMICOLON' && (this.CurrentMode === 'BLOCK' || this.CurrentMode === 'DO_BLOCK') )
					{
						this.Prefix = 'NEWLINE' ;
					}
					else if ( this.LastType === 'TK_SEMICOLON' && this.CurrentMode === 'EXPRESSION' )
					{
						this.Prefix = 'SPACE' ;
					}
					else if ( this.LastType === 'TK_STRING' )
					{
						this.Prefix = 'NEWLINE' ;
					}
					else if ( this.LastType === 'TK_WORD' )
					{
						this.Prefix = 'SPACE' ;
					}
					else if ( this.LastType === 'TK_START_BLOCK' )
					{
						this.Prefix = this.IsExpression() ? 'SPACE' : 'NEWLINE' ;
					}
					else if ( this.LastType === 'TK_END_EXPR' )
					{
						this.PrintSpace() ;
						this.Prefix = 'NEWLINE' ;
					}
		
					if ( this.LastType !== 'TK_END_BLOCK' && this.InArray(this.TokenText.toLowerCase(), ['else', 'catch', 'finally']) )
					{
						this.PrintNewline() ;
					}
					else if ( this.InArray(this.TokenText, this.LineStarters) || this.Prefix === 'NEWLINE' )
					{
						if ( this.LastText === 'else' )
						{
							// no need to force newline on else break
							this.PrintSpace() ;
						}
						else if ( (this.LastType === 'TK_START_EXPR' || this.LastText === '=') && this.TokenText === 'function' )
						{
							// no need to force newline on 'function': (function
							// DONOTHING
						}
						else if ( this.LastType === 'TK_WORD' && (this.LastText === 'return' || this.LastText === 'throw') )
						{
							// no newline between 'return nnn'
							this.PrintSpace() ;
						}
						else if ( this.LastType !== 'TK_END_EXPR' )
						{
							if ( (this.LastType !== 'TK_START_EXPR' || this.TokenText !== 'var') && this.LastText !== ':' )
							{
								// no need to force newline on 'var': for (var x = 0...)
								if ( this.TokenText === 'if' && this.LastType === 'TK_WORD' && this.LastWord === 'else' )
								{
									// no newline for } else if {
									this.PrintSpace() ;
								}
								else
								{
									this.PrintNewline() ;
								}
							}
						}
						else
						{
							if ( this.InArray(this.TokenText, this.LineStarters) && this.LastText !== ')' )
							{
								this.PrintNewline() ;
							}
						}
					}
					else if ( this.Prefix === 'SPACE' )
					{
						this.PrintSpace() ;
					}
					
					this.PrintToken() ;
					this.LastWord = this.TokenText ;
		
					if ( this.TokenText === 'var' )
					{
						this.VarLine = true ;
						this.VarLineTainted = false ;
					}
		
					if ( this.TokenText === 'if' || this.TokenText === 'else' )
					{
						this.IfLineFlag = true ;
					}
		
					break;


				case 'TK_SEMICOLON':
					this.PrintToken() ;
					this.VarLine = false ;
					break ;


				case 'TK_STRING':
					if ( this.LastType === 'TK_START_BLOCK' || this.LastType === 'TK_END_BLOCK' || this.LastType == 'TK_SEMICOLON' )
					{
						this.PrintNewline() ;
					}
					else if ( this.LastType === 'TK_WORD' )
					{
						this.PrintSpace() ;
					}
					
					this.PrintToken() ;
					break;


				case 'TK_OPERATOR':
					var start_delim = true ;
					var end_delim = true ;
					
					if ( this.VarLine && this.TokenText !== ',' )
					{
						this.VarLineTainted = true ;
						
						if ( this.TokenText === ':' )
						{
							this.VarLine = false ;
						}
					}
		
					if ( this.TokenText === ':' && this.InCase )
					{
						this.PrintToken() ; // colon really asks for separate treatment
						this.PrintNewline() ;
						break ;
					}
		
					if ( this.TokenText === '::' )
					{
						// no spaces around exotic namespacing syntax operator
						this.PrintToken() ;
						break ;
					}
		
					this.InCase = false ;
		
					if ( this.TokenText === ',' )
					{
						if ( this.VarLine )
						{
							if ( this.VarLineTainted )
							{
								this.PrintToken() ;
								this.PrintNewline() ;
								this.VarLineTainted = false ;
							}
							else
							{
								this.PrintToken() ;
								this.PrintSpace() ;
							}
						}
						else if ( this.LastType === 'TK_END_BLOCK' && !this.IsExpression() )
						{
							this.PrintToken() ;
							this.PrintNewline() ;
						}
						else
						{
							if ( this.CurrentMode === 'BLOCK' )
							{
								this.PrintToken() ;
								this.PrintNewline() ;
							}
							else
							{
								// EXPR od DO_BLOCK
								this.PrintToken() ;
								this.PrintSpace() ;
							}
						}
						break ;
					}
					else if ( this.TokenText === '--' || this.TokenText === '++' )
					{
						// unary operators special case
						if ( this.LastText === ';' )
						{
							// space for (;; ++i)
							start_delim = true ;
							end_delim = false ;
						}
						else
						{
							start_delim = false ;
							end_delim = false ;
						}
					}
					else if ( this.TokenText === '!' && this.LastType === 'TK_START_EXPR' )
					{
						// special case handling: if (!a)
						start_delim = false ;
						end_delim = false ;
					}
					else if ( this.LastType === 'TK_OPERATOR' )
					{
						start_delim = false ;
						end_delim = false ;
					}
					else if ( this.TokenText === '.' )
					{
						// decimal digits or object.property
						start_delim = false ;
						end_delim = false ;
					}
					else if ( this.LastType === 'TK_END_EXPR' )
					{
						start_delim = true ;
						end_delim = true ;
					}
					else if ( this.TokenText === ':' )
					{
						// zz: xx
						// can't differentiate ternary op,
						// so for now it's a ? b: c; without space before colon
						if ( this.LastText.match(/^\d+$/) )
						{
							// a little help for ternary a ? 1 : 0;
							start_delim = true ;
						}
						else
						{
							start_delim = false ;
						}
					}
					
					if ( start_delim )
					{
						this.PrintSpace() ;
					}
		
					this.PrintToken() ;
		
					if ( end_delim )
					{
						this.PrintSpace() ;
					}
					break ;


				case 'TK_BLOCK_COMMENT':
					this.PrintNewline() ;
					this.PrintToken() ;
					this.PrintNewline() ;
					break ;
		
				case 'TK_COMMENT':
					// this.PrintNewline();
					this.PrintSpace() ;
					this.PrintToken() ;
					this.PrintNewline() ;
					break;


				case 'TK_UNKNOWN':
					this.PrintToken() ;
					break;
			}
	
			this.LastType = this.TokenType ;
			this.LastText = this.TokenText ;
		}
		
		return this.Output.join('') ;
	},
	
	TrimOutput : function()
	{
		while ( this.Output.length &&
			(this.Output[this.Output.length - 1] === ' ' || this.Output[this.Output.length - 1] === this.IndentString) )
		{
			this.Output.pop() ;
		}
	},

	PrintNewline : function( ignoreRepeated )
	{
		ignoreRepeated = typeof ignoreRepeated === 'undefined' ? true : ignoreRepeated ;

		this.IfLineFlag = false ;
		this.TrimOutput() ;

		if ( !this.Output.length )
		{
			return ; // no newline on start of file
		}

		if ( this.Output[this.Output.length - 1] !== "\n" || !ignoreRepeated )
		{
			this.Output.push("\n") ;
			this.LineBreaksCount++ ;
		}
		
		this.Output = this.PrintIndentation( this.Output ) ;
	},

	PrintSpace : function()
	{
		var last_output = this.Output.length ? this.Output[this.Output.length - 1] : ' ' ;
		if ( last_output !== ' ' && last_output !== '\n' && last_output !== this.IndentString )
		{
			// prevent occassional duplicate space
			this.Output.push(' ') ;
		}
	},

	PrintToken : function()
	{
		this.Output.push( this.TokenText ) ;
	},

	Indent : function()
	{
		this.IndentLevel++ ;
	},

	Outdent : function()
	{
		if ( this.IndentLevel )
		{
			this.IndentLevel-- ;
		}
	},

	RemoveIndent : function()
	{
		if ( this.Output.length && this.Output[this.Output.length - 1] === this.IndentString )
		{
			this.Output.pop() ;
		}
	},

	SetMode : function( mode )
	{
		this.Modes.push( this.CurrentMode ) ;
		this.CurrentMode = mode ;
	},

	RestoreMode : function()
	{
		this.DoBlockJustClosed = this.CurrentMode === 'DO_BLOCK' ;
		this.CurrentMode = this.Modes.pop() ;
	},

	InArray : function( Item, aArray )
	{
		if ( aArray.IndexOf )
		{
			return aArray.IndexOf( Item ) != -1 ;
		}
		
		return false ;
	},
	
	PrintIndentation : function( source )
	{
		for ( var i = 0 ; i < this.IndentLevel ; i++ )
		{
			if ( source.push )
			{
				source.push( this.IndentString ) ;
			}
			else if ( typeof source == 'string' )
			{
				source += this.IndentString ;
			}
		}
		
		return source ;
	},
	
	IsExpression : function()
	{
		return this.CurrentMode === 'EXPRESSION' ;
	},

	GetNextToken : function()
	{
		var n_newlines = 0 ;
		var c = '' ;

		do
		{
			if ( this.ParserPos >= this.Input.length )
			{
				return ['', 'TK_EOF'] ;
			}
			
			c = this.Input.charAt( this.ParserPos ) ;
			this.ParserPos += 1 ;

			if ( c === "\n" )
			{
				n_newlines += 1 ;
			}
		}
		while ( this.InArray(c, this.Whitespace) ) ;

		var wanted_newline = false ;

		if ( this.PreserveNewlines )
		{
			if ( n_newlines > 1 )
			{
				for ( var i = 0 ; i < 2 ; i++ )
				{
					this.PrintNewline(i === 0) ;
				}
			}
			
			wanted_newline = ( n_newlines === 1 ) ;
		}

		if ( this.InArray(c, this.Wordchar) )
		{
			if ( this.ParserPos < this.Input.length )
			{
				while ( this.InArray(this.Input.charAt(this.ParserPos), this.Wordchar) )
				{
					c += this.Input.charAt( this.ParserPos ) ;
					this.ParserPos += 1 ;
				
					if ( this.ParserPos === this.Input.length )
					{
						break ;
					}
				}
			}

			// small and surprisingly unugly hack for 1E-10 representation
			if ( this.ParserPos !== this.Input.length && c.match(/^[0-9]+[Ee]$/)
					&& this.Input.charAt(this.ParserPos) === '-' )
			{
				this.ParserPos += 1 ;

				var t = this.GetNextToken( this.ParserPos ) ;
				c += '-' + t[0] ;
				return [c, 'TK_WORD'] ;
			}

			if ( c === 'in' )
			{
				// hack for 'in' operator
				return [c, 'TK_OPERATOR'] ;
			}
			
			if ( wanted_newline && this.LastType !== 'TK_OPERATOR' && !this.IfLineFlag )
			{
				this.PrintNewline() ;
			}
			
			return [c, 'TK_WORD'] ;
		}

		if ( c === '(' || c === '[' )
		{
			return [c, 'TK_START_EXPR'] ;
		}

		if ( c === ')' || c === ']' )
		{
			return [c, 'TK_END_EXPR'] ;
		}

		if ( c === '{' )
		{
			return [c, 'TK_START_BLOCK'] ;
		}

		if ( c === '}' )
		{
			return [c, 'TK_END_BLOCK'] ;
		}

		if ( c === ';' )
		{
			return [c, 'TK_SEMICOLON'] ;
		}

		if ( c === '/' )
		{
			var sComment = '', curChar = '', isMultiLine = false ;
			// peek for comment /* ... */
		
			if ( this.Input.charAt(this.ParserPos) === '*' )
			{
				this.ParserPos += 1 ;
				
				if ( this.ParserPos < this.Input.length )
				{
					while ( ! (this.Input.charAt(this.ParserPos) === '*' && this.Input.charAt(this.ParserPos + 1) && this.Input.charAt(this.ParserPos + 1) === '/') && this.ParserPos < this.Input.length )
					{
						curChar = this.Input.charAt( this.ParserPos ) ;
						if ( curChar == '*' )
						{
							sComment += '\n ' ;
							sComment = this.PrintIndentation( sComment ) ;
							isMultiLine = true ;
						}
						
						sComment += curChar ;
						this.ParserPos += 1 ;
						
						if ( this.ParserPos >= this.Input.length )
						{
							break ;
						}
					}
				}
				this.ParserPos += 2 ;
				
				if ( isMultiLine )
				{
					sComment += '\n ' ;
					sComment = this.PrintIndentation( sComment ) ;
				}
				
				return ['/*' + sComment + '*/', 'TK_BLOCK_COMMENT'] ;
			}
			
			// peek for comment // ...
			if ( this.Input.charAt(this.ParserPos) === '/' )
			{
				sComment = c ;
				while ( this.Input.charAt(this.ParserPos) !== "\x0d" && this.Input.charAt(this.ParserPos) !== "\x0a" )
				{
					sComment += this.Input.charAt( this.ParserPos ) ;
					this.ParserPos += 1 ;
					if ( this.ParserPos >= this.Input.length )
					{
						break ;
					}
				}
				
				this.ParserPos += 1 ;
				if ( wanted_newline )
				{
					this.PrintNewline() ;
				}
				return [sComment, 'TK_COMMENT'] ;
			}
		}

		if ( c === "'" || // string
			 c === '"' || // string
			(c === '/' &&
			((this.LastType === 'TK_WORD' && this.LastText === 'return') || (this.LastType === 'TK_START_EXPR' || this.LastType === 'TK_END_BLOCK' || this.LastType === 'TK_OPERATOR' || this.LastType === 'TK_EOF' || this.LastType === 'TK_SEMICOLON'))) )
		{
			// regexp
			var sep = c ;
			var esc = false ;
			var resulting_string = '' ;

			if ( this.ParserPos < this.Input.length )
			{
				while ( esc || this.Input.charAt(this.ParserPos) !== sep )
				{
					resulting_string += this.Input.charAt( this.ParserPos ) ;
					
					if ( !esc )
					{
						esc = this.Input.charAt( this.ParserPos ) === '\\' ;
					}
					else
					{
						esc = false ;
					}
					
					this.ParserPos += 1 ;
					if ( this.ParserPos >= this.Input.length )
					{
						break ;
					}
				}
			}

			this.ParserPos += 1 ;

			resulting_string = sep + resulting_string + sep ;

			if ( sep == '/' )
			{
				// regexps may have modifiers /regexp/MOD , so fetch those, too
				while ( this.ParserPos < this.Input.length && this.InArray(this.Input.charAt(this.ParserPos), this.Wordchar) )
				{
					resulting_string += this.Input.charAt( this.ParserPos ) ;
					this.ParserPos += 1 ;
				}
			}
			return [resulting_string, 'TK_STRING'] ;
		}
		
		if ( this.InArray(c, this.Punct) )
		{
			// Process html entities
			if ( c == "&" && !this.InArray(this.Input.charAt(this.ParserPos), this.Punct) )
			{
				// & is probably the start of entity
				// move back current position and reset c
				this.ParserPos-- ;
				c = '' ;

				var curChar = this.Input.charAt( this.ParserPos ) ;
				
				var loopChar = function( curChar )
				{
					c += curChar ;
					this.ParserPos++ ;
					
					if ( this.ParserPos >= this.Input.length )
					{
						return null ;
					}
	
					curChar = this.Input.charAt( this.ParserPos ) ;
					
					return curChar ;
				};
				
				while ( curChar == '&' )
				{
					curChar = loopChar.call( this, curChar ) ;
					
					while ( this.ParserPos < this.Input.length &&
							this.InArray(curChar, this.Wordchar) || curChar == ';' )
					{
						curChar = loopChar.call( this, curChar ) ;
						
						if ( c.lastIndexOf(';') == c.length - 1 )
						{
							break ;
						}
					}
				}
			}
			
			while ( this.ParserPos < this.Input.length && this.InArray(FCKTools.HTMLDecode( c ) + this.Input.charAt(this.ParserPos), this.Punct) )
			{
				c += this.Input.charAt( this.ParserPos ) ;
				this.ParserPos += 1 ;
				if ( this.ParserPos >= this.Input.length )
				{
					break ;
				}
			}
			
			// Process brs
			// formatter doesn't support brs with attributes
			if ( c == '<' && this.Input.substr(this.ParserPos, 5) == 'br />' )
			{
				c += 'br />' ;
				this.ParserPos += 5 ;
				
				return [c, 'TK_WORD'] ;
			}
			
			return [c, 'TK_OPERATOR'] ;
		}
		
		return [c, 'TK_UNKNOWN'] ;
	}
}

