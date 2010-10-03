
/*
 * MindTouch DekiScript - embeddable web-oriented scripting runtime
 * Copyright (c) 2006-2010 MindTouch Inc.
 * www.mindtouch.com  oss@mindtouch.com
 *
 * For community documentation and downloads visit wiki.developer.mindtouch.com;
 * please review the licensing section.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.IO;
using System.Collections.Generic;
using MindTouch.Deki.Script.Expr;

namespace MindTouch.Deki.Script.Compiler {

internal class Token {
	public int kind;    // token kind
	public int pos;     // token position in the source text (starting at 0)
	public int col;     // token column (starting at 1)
	public int line;    // token line (starting at 1)
    public string origin; // origin of source code
	public string val;  // token value
	public Token next;  // ML 2005-03-11 Tokens are kept in linked list
	
	public Location Location { get { return new Location(origin, line, col); } }
}

//-----------------------------------------------------------------------------------
// Buffer
//-----------------------------------------------------------------------------------
internal class Buffer {
	// This Buffer supports the following cases:
	// 1) seekable stream (file)
	//    a) whole stream in buffer
	//    b) part of stream in buffer
	// 2) non seekable stream (network, console)

	public const int EOF = char.MaxValue + 1;
	const int MIN_BUFFER_LENGTH = 1024; // 1KB
	const int MAX_BUFFER_LENGTH = MIN_BUFFER_LENGTH * 64; // 64KB
	byte[] buf;         // input buffer
	int bufStart;       // position of first byte in buffer relative to input stream
	int bufLen;         // length of buffer
	int fileLen;        // length of input stream (may change if the stream is no file)
	int bufPos;         // current position in buffer
	Stream stream;      // input stream (seekable)
	bool isUserStream;  // was the stream opened by the user?
	
	public Buffer (Stream s, bool isUserStream) {
		stream = s; this.isUserStream = isUserStream;
		
		if (stream.CanSeek) {
			fileLen = (int) stream.Length;
			bufLen = Math.Min(fileLen, MAX_BUFFER_LENGTH);
			bufStart = Int32.MaxValue; // nothing in the buffer so far
		} else {
			fileLen = bufLen = bufStart = 0;
		}

		buf = new byte[(bufLen>0) ? bufLen : MIN_BUFFER_LENGTH];
		if (fileLen > 0) Pos = 0; // setup buffer to position 0 (start)
		else bufPos = 0; // index 0 is already after the file, thus Pos = 0 is invalid
		if (bufLen == fileLen && stream.CanSeek) Close();
	}
	
	protected Buffer(Buffer b) { // called in UTF8Buffer constructor
		buf = b.buf;
		bufStart = b.bufStart;
		bufLen = b.bufLen;
		fileLen = b.fileLen;
		bufPos = b.bufPos;
		stream = b.stream;
		// keep destructor from closing the stream
		b.stream = null;
		isUserStream = b.isUserStream;
	}

	~Buffer() { Close(); }
	
	protected void Close() {
		if (!isUserStream && stream != null) {
			stream.Close();
			stream = null;
		}
	}
	
	public virtual int Read () {
		if (bufPos < bufLen) {
			return buf[bufPos++];
		} else if (Pos < fileLen) {
			Pos = Pos; // shift buffer start to Pos
			return buf[bufPos++];
		} else if (stream != null && !stream.CanSeek && ReadNextStreamChunk() > 0) {
			return buf[bufPos++];
		} else {
			return EOF;
		}
	}

	public int Peek () {
		int curPos = Pos;
		int ch = Read();
		Pos = curPos;
		return ch;
	}
	
	public string GetString (int beg, int end) {
		int len = 0;
		char[] buf = new char[end - beg];
		int oldPos = Pos;
		Pos = beg;
		while (Pos < end) buf[len++] = (char) Read();
		Pos = oldPos;
		return new String(buf, 0, len);
	}

	public int Pos {
		get { return bufPos + bufStart; }
		set {
			if (value >= fileLen && stream != null && !stream.CanSeek) {
				// Wanted position is after buffer and the stream
				// is not seek-able e.g. network or console,
				// thus we have to read the stream manually till
				// the wanted position is in sight.
				while (value >= fileLen && ReadNextStreamChunk() > 0);
			}

			if (value < 0 || value > fileLen) {
				throw new FatalError("buffer out of bounds access, position: " + value);
			}

			if (value >= bufStart && value < bufStart + bufLen) { // already in buffer
				bufPos = value - bufStart;
			} else if (stream != null) { // must be swapped in
				stream.Seek(value, SeekOrigin.Begin);
				bufLen = stream.Read(buf, 0, buf.Length);
				bufStart = value; bufPos = 0;
			} else {
				// set the position to the end of the file, Pos will return fileLen.
				bufPos = fileLen - bufStart;
			}
		}
	}
	
	// Read the next chunk of bytes from the stream, increases the buffer
	// if needed and updates the fields fileLen and bufLen.
	// Returns the number of bytes read.
	private int ReadNextStreamChunk() {
		int free = buf.Length - bufLen;
		if (free == 0) {
			// in the case of a growing input stream
			// we can neither seek in the stream, nor can we
			// foresee the maximum length, thus we must adapt
			// the buffer size on demand.
			byte[] newBuf = new byte[bufLen * 2];
			Array.Copy(buf, newBuf, bufLen);
			buf = newBuf;
			free = bufLen;
		}
		int read = stream.Read(buf, bufLen, free);
		if (read > 0) {
			fileLen = bufLen = (bufLen + read);
			return read;
		}
		// end of stream reached
		return 0;
	}
}

//-----------------------------------------------------------------------------------
// UTF8Buffer
//-----------------------------------------------------------------------------------
internal class UTF8Buffer: Buffer {
	public UTF8Buffer(Buffer b): base(b) {}

	public override int Read() {
		int ch;
		do {
			ch = base.Read();
			// until we find a utf8 start (0xxxxxxx or 11xxxxxx)
		} while ((ch >= 128) && ((ch & 0xC0) != 0xC0) && (ch != EOF));
		if (ch < 128 || ch == EOF) {
			// nothing to do, first 127 chars are the same in ascii and utf8
			// 0xxxxxxx or end of file character
		} else if ((ch & 0xF0) == 0xF0) {
			// 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
			int c1 = ch & 0x07; ch = base.Read();
			int c2 = ch & 0x3F; ch = base.Read();
			int c3 = ch & 0x3F; ch = base.Read();
			int c4 = ch & 0x3F;
			ch = (((((c1 << 6) | c2) << 6) | c3) << 6) | c4;
		} else if ((ch & 0xE0) == 0xE0) {
			// 1110xxxx 10xxxxxx 10xxxxxx
			int c1 = ch & 0x0F; ch = base.Read();
			int c2 = ch & 0x3F; ch = base.Read();
			int c3 = ch & 0x3F;
			ch = (((c1 << 6) | c2) << 6) | c3;
		} else if ((ch & 0xC0) == 0xC0) {
			// 110xxxxx 10xxxxxx
			int c1 = ch & 0x1F; ch = base.Read();
			int c2 = ch & 0x3F;
			ch = (c1 << 6) | c2;
		}
		return ch;
	}
}

//-----------------------------------------------------------------------------------
// Scanner
//-----------------------------------------------------------------------------------
internal class Scanner {
	const char EOL = '\n';
	const int eofSym = 0; /* pdt */
	const int maxT = 74;
	const int noSym = 74;
	char valCh;       // current input character (for token.val)

	public Buffer buffer; // scanner buffer
	
	Token t;          // current token
	int ch;           // current input character
	int pos;          // byte position of current character
	int col;          // column number of current character
	int line;         // line number of current character
	string origin;	  // code origin (e.g. filename, web address, ...)
	int oldEols;      // EOLs that appeared in a comment;
	static readonly Dictionary<int, int> start; // maps first token character to start state

	Token tokens;     // list of tokens already peeked (first token is a dummy)
	Token pt;         // current peek token
	
	char[] tval = new char[128]; // text of current token
	int tlen;         // length of current token
	
	static Scanner() {
		start = new Dictionary<int, int>(128);
		for (int i = 97; i <= 122; ++i) start[i] = 1;
		for (int i = 49; i <= 57; ++i) start[i] = 32;
		start[95] = 76; 
		start[36] = 33; 
		start[64] = 5; 
		start[34] = 34; 
		start[39] = 35; 
		start[46] = 77; 
		start[48] = 36; 
		start[38] = 78; 
		start[59] = 49; 
		start[123] = 50; 
		start[125] = 51; 
		start[61] = 79; 
		start[43] = 80; 
		start[45] = 81; 
		start[42] = 82; 
		start[47] = 83; 
		start[37] = 84; 
		start[44] = 58; 
		start[40] = 59; 
		start[41] = 60; 
		start[58] = 61; 
		start[33] = 85; 
		start[63] = 86; 
		start[124] = 64; 
		start[60] = 87; 
		start[62] = 88; 
		start[35] = 71; 
		start[91] = 72; 
		start[93] = 73; 
		start[Buffer.EOF] = -1;

	}
	
	public Scanner (Stream s, string origin, int line, int column) {
		buffer = new Buffer(s, true);
		Init(origin, line, column);
	}
	
	void Init(string o, int l, int c) {
		pos = -1; line = l; col = c;
		origin = o;
		oldEols = 0;
		
		// NOTE: always use UTF-8 decoding
		buffer = new UTF8Buffer(buffer);
		
		NextCh();
		pt = tokens = new Token();  // first token is a dummy
	}
	
	void NextCh() {
		if (oldEols > 0) { ch = EOL; oldEols--; } 
		else {
			pos = buffer.Pos;
			ch = buffer.Read(); col++;
			// replace isolated '\r' by '\n' in order to make
			// eol handling uniform across Windows, Unix and Mac
			if (ch == '\r' && buffer.Peek() != '\n') ch = EOL;
			if (ch == EOL) { line++; col = 0; }
		}
		if (ch != Buffer.EOF) {
			valCh = (char) ch;
			ch = char.ToLower((char) ch);
		}

	}

	void AddCh() {
		if (tlen >= tval.Length) {
			char[] newBuf = new char[2 * tval.Length];
			Array.Copy(tval, 0, newBuf, 0, tval.Length);
			tval = newBuf;
		}
		if (ch != Buffer.EOF) {
			tval[tlen++] = valCh;
			NextCh();
		}
	}



	bool Comment0() {
		int level = 1, pos0 = pos, line0 = line, col0 = col;
		NextCh();
		if (ch == '/') {
			NextCh();
			for(;;) {
				if (ch == 10) {
					level--;
					if (level == 0) { oldEols = line - line0; NextCh(); return true; }
					NextCh();
				} else if (ch == Buffer.EOF) return false;
				else NextCh();
			}
		} else {
			buffer.Pos = pos0; NextCh(); line = line0; col = col0;
		}
		return false;
	}

	bool Comment1() {
		int level = 1, pos0 = pos, line0 = line, col0 = col;
		NextCh();
		if (ch == '*') {
			NextCh();
			for(;;) {
				if (ch == '*') {
					NextCh();
					if (ch == '/') {
						level--;
						if (level == 0) { oldEols = line - line0; NextCh(); return true; }
						NextCh();
					}
				} else if (ch == Buffer.EOF) return false;
				else NextCh();
			}
		} else {
			buffer.Pos = pos0; NextCh(); line = line0; col = col0;
		}
		return false;
	}


	void CheckLiteral() {
		switch (t.val.ToLower()) {
			case "let": t.kind = 11; break;
			case "var": t.kind = 19; break;
			case "if": t.kind = 21; break;
			case "else": t.kind = 24; break;
			case "try": t.kind = 25; break;
			case "catch": t.kind = 26; break;
			case "finally": t.kind = 27; break;
			case "foreach": t.kind = 28; break;
			case "switch": t.kind = 29; break;
			case "case": t.kind = 30; break;
			case "default": t.kind = 32; break;
			case "break": t.kind = 33; break;
			case "continue": t.kind = 34; break;
			case "return": t.kind = 35; break;
			case "is": t.kind = 49; break;
			case "not": t.kind = 50; break;
			case "nil": t.kind = 51; break;
			case "in": t.kind = 52; break;
			case "typeof": t.kind = 61; break;
			case "null": t.kind = 67; break;
			case "true": t.kind = 68; break;
			case "false": t.kind = 69; break;
			case "where": t.kind = 70; break;
			default: break;
		}
	}

	Token NextToken() {
		while (ch == ' ' ||
			ch >= 9 && ch <= 10 || ch == 13 || ch == 160 || ch == 173
		) NextCh();
		if (ch == '/' && Comment0() ||ch == '/' && Comment1()) return NextToken();
		int recKind = noSym;
		int recEnd = pos;
		t = new Token();
		t.pos = pos; t.col = col; t.line = line; t.origin = origin;
		int state;
		start.TryGetValue(ch, out state);
		tlen = 0; AddCh();
		
		switch (state) {
			case -1: { t.kind = eofSym; break; } // NextCh already done
			case 0: {
				if (recKind != noSym) {
					tlen = recEnd - t.pos;
					SetScannerBehindT();
				}
				t.kind = recKind; break;
			} // NextCh already done
			case 1:
				recEnd = pos; recKind = 1;
				if (ch >= '0' && ch <= '9' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 1;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 2:
				recEnd = pos; recKind = 1;
				if (ch >= '0' && ch <= '9' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 2;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 3:
				recEnd = pos; recKind = 1;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 3;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 4:
				recEnd = pos; recKind = 1;
				if (ch >= '0' && ch <= '9' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 4;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 5:
				if (ch >= 'a' && ch <= 'z') {AddCh(); goto case 6;}
				else {goto case 0;}
			case 6:
				recEnd = pos; recKind = 2;
				if (ch >= '0' && ch <= '9' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 6;}
				else {t.kind = 2; break;}
			case 7:
				if (ch <= '!' || ch >= '#' && ch <= 65535) {AddCh(); goto case 7;}
				else if (ch == '"') {AddCh(); goto case 37;}
				else {goto case 0;}
			case 8:
				if (ch <= '&' || ch >= '(' && ch <= 65535) {AddCh(); goto case 8;}
				else if (ch == 39) {AddCh(); goto case 38;}
				else {goto case 0;}
			case 9:
				if (ch <= '!' || ch >= '#' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 9;}
				else if (ch == '"') {AddCh(); goto case 11;}
				else if (ch == 92) {AddCh(); goto case 39;}
				else {goto case 0;}
			case 10:
				if (ch <= '&' || ch >= '(' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 10;}
				else if (ch == 39) {AddCh(); goto case 11;}
				else if (ch == 92) {AddCh(); goto case 40;}
				else {goto case 0;}
			case 11:
				{t.kind = 4; break;}
			case 12:
				recEnd = pos; recKind = 5;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 12;}
				else if (ch == 'e') {AddCh(); goto case 13;}
				else {t.kind = 5; break;}
			case 13:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 15;}
				else if (ch == '+' || ch == '-') {AddCh(); goto case 14;}
				else {goto case 0;}
			case 14:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 15;}
				else {goto case 0;}
			case 15:
				recEnd = pos; recKind = 5;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 15;}
				else {t.kind = 5; break;}
			case 16:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 17;}
				else {goto case 0;}
			case 17:
				recEnd = pos; recKind = 5;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 17;}
				else if (ch == 'e') {AddCh(); goto case 18;}
				else {t.kind = 5; break;}
			case 18:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 20;}
				else if (ch == '+' || ch == '-') {AddCh(); goto case 19;}
				else {goto case 0;}
			case 19:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 20;}
				else {goto case 0;}
			case 20:
				recEnd = pos; recKind = 5;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 20;}
				else {t.kind = 5; break;}
			case 21:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 23;}
				else if (ch == '+' || ch == '-') {AddCh(); goto case 22;}
				else {goto case 0;}
			case 22:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 23;}
				else {goto case 0;}
			case 23:
				recEnd = pos; recKind = 5;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 23;}
				else {t.kind = 5; break;}
			case 24:
				if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 25;}
				else {goto case 0;}
			case 25:
				recEnd = pos; recKind = 6;
				if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 25;}
				else {t.kind = 6; break;}
			case 26:
				if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 26;}
				else if (ch == ';') {AddCh(); goto case 31;}
				else {goto case 0;}
			case 27:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 28;}
				else if (ch == 'x') {AddCh(); goto case 29;}
				else {goto case 0;}
			case 28:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 28;}
				else if (ch == ';') {AddCh(); goto case 31;}
				else {goto case 0;}
			case 29:
				if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 30;}
				else {goto case 0;}
			case 30:
				if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 30;}
				else if (ch == ';') {AddCh(); goto case 31;}
				else {goto case 0;}
			case 31:
				{t.kind = 7; break;}
			case 32:
				recEnd = pos; recKind = 5;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 32;}
				else if (ch == '.') {AddCh(); goto case 16;}
				else if (ch == 'e') {AddCh(); goto case 21;}
				else {t.kind = 5; break;}
			case 33:
				recEnd = pos; recKind = 1;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 3;}
				else if (ch >= 'a' && ch <= 'z') {AddCh(); goto case 4;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 34:
				if (ch <= '!' || ch >= '#' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 9;}
				else if (ch == '"') {AddCh(); goto case 41;}
				else if (ch == 92) {AddCh(); goto case 39;}
				else {goto case 0;}
			case 35:
				if (ch <= '&' || ch >= '(' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 10;}
				else if (ch == 39) {AddCh(); goto case 42;}
				else if (ch == 92) {AddCh(); goto case 40;}
				else {goto case 0;}
			case 36:
				recEnd = pos; recKind = 5;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 32;}
				else if (ch == '.') {AddCh(); goto case 16;}
				else if (ch == 'e') {AddCh(); goto case 21;}
				else if (ch == 'x') {AddCh(); goto case 24;}
				else {t.kind = 5; break;}
			case 37:
				if (ch <= '!' || ch >= '#' && ch <= 65535) {AddCh(); goto case 7;}
				else if (ch == '"') {AddCh(); goto case 43;}
				else {goto case 0;}
			case 38:
				if (ch <= '&' || ch >= '(' && ch <= 65535) {AddCh(); goto case 8;}
				else if (ch == 39) {AddCh(); goto case 44;}
				else {goto case 0;}
			case 39:
				if (ch <= '!' || ch >= '#' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 9;}
				else if (ch == '"') {AddCh(); goto case 45;}
				else if (ch == 92) {AddCh(); goto case 39;}
				else {goto case 0;}
			case 40:
				if (ch <= '&' || ch >= '(' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 10;}
				else if (ch == 39) {AddCh(); goto case 46;}
				else if (ch == 92) {AddCh(); goto case 40;}
				else {goto case 0;}
			case 41:
				recEnd = pos; recKind = 4;
				if (ch == '"') {AddCh(); goto case 7;}
				else {t.kind = 4; break;}
			case 42:
				recEnd = pos; recKind = 4;
				if (ch == 39) {AddCh(); goto case 8;}
				else {t.kind = 4; break;}
			case 43:
				if (ch <= '!' || ch >= '#' && ch <= 65535) {AddCh(); goto case 7;}
				else if (ch == '"') {AddCh(); goto case 47;}
				else {goto case 0;}
			case 44:
				if (ch <= '&' || ch >= '(' && ch <= 65535) {AddCh(); goto case 8;}
				else if (ch == 39) {AddCh(); goto case 48;}
				else {goto case 0;}
			case 45:
				recEnd = pos; recKind = 4;
				if (ch <= '!' || ch >= '#' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 9;}
				else if (ch == '"') {AddCh(); goto case 11;}
				else if (ch == 92) {AddCh(); goto case 39;}
				else {t.kind = 4; break;}
			case 46:
				recEnd = pos; recKind = 4;
				if (ch <= '&' || ch >= '(' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 10;}
				else if (ch == 39) {AddCh(); goto case 11;}
				else if (ch == 92) {AddCh(); goto case 40;}
				else {t.kind = 4; break;}
			case 47:
				recEnd = pos; recKind = 3;
				if (ch <= '!' || ch >= '#' && ch <= 65535) {AddCh(); goto case 7;}
				else if (ch == '"') {AddCh(); goto case 47;}
				else {t.kind = 3; break;}
			case 48:
				recEnd = pos; recKind = 3;
				if (ch <= '&' || ch >= '(' && ch <= 65535) {AddCh(); goto case 8;}
				else if (ch == 39) {AddCh(); goto case 48;}
				else {t.kind = 3; break;}
			case 49:
				{t.kind = 8; break;}
			case 50:
				{t.kind = 9; break;}
			case 51:
				{t.kind = 10; break;}
			case 52:
				{t.kind = 13; break;}
			case 53:
				{t.kind = 14; break;}
			case 54:
				{t.kind = 15; break;}
			case 55:
				{t.kind = 16; break;}
			case 56:
				{t.kind = 17; break;}
			case 57:
				{t.kind = 18; break;}
			case 58:
				{t.kind = 20; break;}
			case 59:
				{t.kind = 22; break;}
			case 60:
				{t.kind = 23; break;}
			case 61:
				{t.kind = 31; break;}
			case 62:
				{t.kind = 36; break;}
			case 63:
				{t.kind = 38; break;}
			case 64:
				if (ch == '|') {AddCh(); goto case 65;}
				else {goto case 0;}
			case 65:
				{t.kind = 39; break;}
			case 66:
				{t.kind = 40; break;}
			case 67:
				{t.kind = 43; break;}
			case 68:
				{t.kind = 44; break;}
			case 69:
				{t.kind = 47; break;}
			case 70:
				{t.kind = 48; break;}
			case 71:
				{t.kind = 62; break;}
			case 72:
				{t.kind = 64; break;}
			case 73:
				{t.kind = 65; break;}
			case 74:
				{t.kind = 72; break;}
			case 75:
				{t.kind = 73; break;}
			case 76:
				recEnd = pos; recKind = 66;
				if (ch >= '0' && ch <= '9' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 2;}
				else {t.kind = 66; break;}
			case 77:
				recEnd = pos; recKind = 63;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 12;}
				else if (ch == '.') {AddCh(); goto case 89;}
				else {t.kind = 63; break;}
			case 78:
				recEnd = pos; recKind = 54;
				if (ch >= 'a' && ch <= 'z') {AddCh(); goto case 26;}
				else if (ch == '#') {AddCh(); goto case 27;}
				else if (ch == '&') {AddCh(); goto case 66;}
				else {t.kind = 54; break;}
			case 79:
				recEnd = pos; recKind = 12;
				if (ch == '=') {AddCh(); goto case 90;}
				else {t.kind = 12; break;}
			case 80:
				recEnd = pos; recKind = 55;
				if (ch == '=') {AddCh(); goto case 52;}
				else {t.kind = 55; break;}
			case 81:
				recEnd = pos; recKind = 56;
				if (ch == '=') {AddCh(); goto case 53;}
				else {t.kind = 56; break;}
			case 82:
				recEnd = pos; recKind = 57;
				if (ch == '=') {AddCh(); goto case 54;}
				else {t.kind = 57; break;}
			case 83:
				recEnd = pos; recKind = 58;
				if (ch == '=') {AddCh(); goto case 55;}
				else if (ch == '>') {AddCh(); goto case 75;}
				else {t.kind = 58; break;}
			case 84:
				recEnd = pos; recKind = 59;
				if (ch == '=') {AddCh(); goto case 56;}
				else {t.kind = 59; break;}
			case 85:
				recEnd = pos; recKind = 60;
				if (ch == '!') {AddCh(); goto case 62;}
				else if (ch == '=') {AddCh(); goto case 91;}
				else {t.kind = 60; break;}
			case 86:
				recEnd = pos; recKind = 37;
				if (ch == '?') {AddCh(); goto case 63;}
				else {t.kind = 37; break;}
			case 87:
				recEnd = pos; recKind = 45;
				if (ch == '=') {AddCh(); goto case 69;}
				else if (ch == '/') {AddCh(); goto case 92;}
				else {t.kind = 45; break;}
			case 88:
				recEnd = pos; recKind = 46;
				if (ch == '=') {AddCh(); goto case 70;}
				else {t.kind = 46; break;}
			case 89:
				recEnd = pos; recKind = 53;
				if (ch == '=') {AddCh(); goto case 57;}
				else {t.kind = 53; break;}
			case 90:
				recEnd = pos; recKind = 42;
				if (ch == '=') {AddCh(); goto case 68;}
				else {t.kind = 42; break;}
			case 91:
				recEnd = pos; recKind = 41;
				if (ch == '=') {AddCh(); goto case 67;}
				else {t.kind = 41; break;}
			case 92:
				recEnd = pos; recKind = 71;
				if (ch == '>') {AddCh(); goto case 74;}
				else {t.kind = 71; break;}

		}
		t.val = new String(tval, 0, tlen);
		return t;
	}
	
	private void SetScannerBehindT() {
		buffer.Pos = t.pos;
		NextCh();
		line = t.line; col = t.col;
		for (int i = 0; i < tlen; i++) NextCh();
	}
	
	// get the next token (possibly a token already seen during peeking)
	public Token Scan () {
		if (tokens.next == null) {
			return NextToken();
		} else {
			pt = tokens = tokens.next;
			return tokens;
		}
	}

	// peek for the next token, ignore pragmas
	public Token Peek () {
		do {
		if (pt.next == null) {
				pt.next = NextToken();
			}
			pt = pt.next;
			} while (pt.kind > maxT); // skip pragmas
	
		return pt;
	}
	
	// make sure that peeking starts at the current scan position
	public void ResetPeek () { pt = tokens; }

} // end Scanner

}