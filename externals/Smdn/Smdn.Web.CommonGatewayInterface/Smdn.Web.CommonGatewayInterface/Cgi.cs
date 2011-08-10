// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2009-2011 smdn
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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

using Smdn.Formats;

namespace Smdn.Web.CommonGatewayInterface {
  /*
   * RFC 3875 - The Common Gateway Interface (CGI) Version 1.1
   * http://tools.ietf.org/html/rfc3875
   */
  public class Cgi {
    public IDictionary<string, string> Queries {
      get { return queries; }
    }

    public IDictionary<string, string> Variables {
      get { return variables; }
    }

    public IDictionary<string, string> Headers {
      get { return headers; }
    }

    public HttpStatusCode ResponseCode {
      get; set;
    }

    public bool IsPostRequest {
      get { return string.Equals(GetVariable("REQUEST_METHOD"), "POST"); }
    }

    public bool IsGetRequest {
      get { return string.Equals(GetVariable("REQUEST_METHOD"), "GET"); }
    }

    public Uri ScriptUrl {
      get { return scriptUrl; }
    }

    public TextWriter Out {
      get; private set;
    }

    public TextWriter Error {
      get; private set;
    }

    public Cgi()
    {
      this.Out = new StringWriter(outStringBuilder);
      this.Error = Console.Error;

      Initialize();
    }

    private void Initialize()
    {
      foreach (System.Collections.DictionaryEntry pair in Environment.GetEnvironmentVariables()) {
        variables.Add((string)pair.Key, (string)pair.Value);
      }

      if (IsPostRequest) {
        var reader = new StreamReader(Console.OpenStandardInput());

        queries = ParseQueryString(reader.ReadLine() ?? string.Empty);
      }
      else {
        queries = ParseQueryString(variables.ContainsKey("QUERY_STRING") ? variables["QUERY_STRING"] : string.Empty);
      }

      scriptUrl = new Uri(string.Format("http://{0}{1}", GetVariable("HTTP_HOST"), GetVariable("REQUEST_URI")));

      ResponseCode = HttpStatusCode.OK;
    }

    private static Dictionary<string, string> ParseQueryString(string line)
    {
      var queries = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

      foreach (var pair in line.Split(new[] {'&'}, StringSplitOptions.RemoveEmptyEntries)) {
        var delim = pair.IndexOf("=");

        if (0 <= delim)
          queries[pair.Substring(0, delim)] = pair.Substring(delim + 1);
        else
          queries[pair] = string.Empty;
      }

      return queries;
    }

    public string GetQuery(string key)
    {
      return GetQuery(key, Encoding.ASCII);
    }

    public string GetQuery(string key, Encoding encoding)
    {
      if (queries.ContainsKey(key))
        return PercentEncoding.GetDecodedString(queries[key], encoding, true);
      else
        return string.Empty;
    }

    public string GetVariable(string key)
    {
      if (variables.ContainsKey(key))
        return variables[key];
      else
        return string.Empty;
    }

    public void Flush(Encoding outputEncoding)
    {
      if (flushed)
        throw new InvalidOperationException("already flushed");

      if (!headers.ContainsKey("Content-Type"))
        headers.Add("Content-Type", string.Format("text/plain; charset={0};", outputEncoding.BodyName));

      using (var stdout = Console.OpenStandardOutput()) {
        var writer = new BinaryWriter(stdout);
        var header = new StringBuilder();

        header.AppendFormat("HTTP/1.1 {0} {1}{2}", (int)ResponseCode, ResponseCode, Chars.CRLF);

        foreach (var pair in headers) {
          header.AppendFormat("{0}: {1}{2}", pair.Key, pair.Value, Chars.CRLF);
        }

        header.Append(Chars.CRLF);

        writer.Write(Encoding.ASCII.GetBytes(header.ToString()));
        writer.Flush();

        writer.Write(outputEncoding.GetBytes(outStringBuilder.ToString()));
        writer.Flush();
      }

      flushed = true;
    }

    private Dictionary<string, string> queries = null;
    private Dictionary<string, string> variables = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
    private Dictionary<string, string> headers = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
    private Uri scriptUrl;
    private StringBuilder outStringBuilder = new StringBuilder();
    private bool flushed = false;
  }
}
