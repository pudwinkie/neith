// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2008-2011 smdn
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

using Octets = Smdn.Formats.Octets;

namespace Smdn.Net.Imap4.Protocol.Server {
  public class ImapCommandReceiver : ImapReceiver {
    public bool CommandContinuing {
      get { return commandContinuing; }
    }

    public ImapCommandReceiver(ImapBufferedStream stream)
      : base(stream)
    {
      this.fragmentedCommandBuffer = new ByteStringBuilder(stream.BufferSize);
    }

    private ByteStringBuilder fragmentedCommandBuffer;
    private int requiredDataLengthAtLeast = 0;
    private bool commandContinuing = false;

    public ImapCommand ReceiveCommand()
    {
      var line = Receive(requiredDataLengthAtLeast);

      fragmentedCommandBuffer.Append(line);

      if (fragmentedCommandBuffer.Length < requiredDataLengthAtLeast)
        return null; // need to read more

      try {
        ByteString notParsedLines = null;

        var command = (commandContinuing)
          ? ParseContinuingCommand(fragmentedCommandBuffer.ToByteString(), out notParsedLines, out requiredDataLengthAtLeast)
          : ParseCommand(fragmentedCommandBuffer.ToByteString(), out notParsedLines, out requiredDataLengthAtLeast);

        fragmentedCommandBuffer.Length = 0; // discard buffered

        if (notParsedLines != null)
          fragmentedCommandBuffer.Append(notParsedLines);

        return command;
      }
      catch (Exception ex) {
        fragmentedCommandBuffer.Length = 0; // discard buffered
        if (ex is ImapMalformedDataException)
          throw new ImapMalformedCommandException(ex.Message);
        else
          throw ex;
      }
    }

    private ImapCommand ParseCommand(ByteString line, out ByteString notParsedLines, out int requiredBytes)
    {
      notParsedLines = null;
      requiredBytes = 0;

      /*
      command         = tag SP (command-any / command-auth / command-nonauth /
                        command-select) CRLF
                          ; Modal based on state
      */

      /*
      astring         = 1*ASTRING-CHAR / string
      ASTRING-CHAR   = ATOM-CHAR / resp-specials
      atom            = 1*ATOM-CHAR
      ATOM-CHAR       = <any CHAR except atom-specials>
      atom-specials   = "(" / ")" / "{" / SP / CTL / list-wildcards /
                        quoted-specials / resp-specials
      tag             = 1*<any ASTRING-CHAR except "+">
      resp-specials   = "]"
      */
      var delimTag = line.IndexOf(Octets.SP);

      if (delimTag < 0)
        throw new ImapMalformedCommandException("invalid command");

      // TODO: validation
      var tag = line.Substring(0, delimTag);

      var delimCommand = line.IndexOf(Octets.SP, delimTag + 1);

      var command = (delimCommand < 0)
        ? line.Substring(delimTag + 1, line.Length - delimTag - 3)
        : line.Substring(delimTag + 1, delimCommand - delimTag - 1);
      var uid = false;

      if (0 <= delimCommand && command.EqualsIgnoreCase("UID")) {
        uid = true;

        throw new NotImplementedException("uid");
      }

      // TODO: validation
      if (delimCommand < 0)
        return new ImapCommand(tag.ToString(), uid, command.ToString(), new ImapData[] {});

      // args
      var dataReader = new DataReader(line, delimCommand + 1);

      ImapData[] args;

      if (base.ParseData(dataReader, out args, ref requiredBytes)) {
        commandContinuing = false;
        requiredBytes = 0;
        notParsedLines = dataReader.ReadToEnd();
        return new ImapCommand(tag.ToString(), uid, command.ToString(), args);
      }
      else {
        commandContinuing = true;
        requiredBytes += delimCommand + 1;
        notParsedLines = line;
        return null;
      }
    }

    private ImapCommand ParseContinuingCommand(ByteString line, out ByteString notParsedLines, out int requiredBytes)
    {
      throw new NotImplementedException();
    }
  }
}
