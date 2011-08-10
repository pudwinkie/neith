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
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;

using Smdn.Formats.Feeds;

namespace Smdn.Net.Feeds {
  public class Aggregator : IDisposable {
    public const int DefaultRequestTimeout = 32 * 1000;
    public const int DefaultMaximumRedirections = 32;
    public static readonly string DefaultUserAgent;

    static Aggregator()
    {
      var assmName = System.Reflection.Assembly.GetExecutingAssembly().GetName();

      DefaultUserAgent = string.Format("{0}/{1}", assmName.Name, assmName.Version);
    }

    public int RequestTimeout {
      get
      {
        CheckDisposed();
        return requestTimeout;
      }
      set
      {
        CheckDisposed();
        if (value < -1)
          throw new ArgumentOutOfRangeException("RequestTimeout", "must be greater than or equals to -1");
        requestTimeout = value;
      }
    }

    public int MaximumRedirections {
      get
      {
        CheckDisposed();
        return maximumRedirections;
      }
      set
      {
        CheckDisposed();
        if (value < 0)
          throw new ArgumentOutOfRangeException("MaximumRedirections", "must be zero or positive number");
        maximumRedirections = value;
      }
    }

    public string UserAgent {
      get
      {
        CheckDisposed();
        return userAgent;
      }
      set
      {
        CheckDisposed();
        userAgent = value;
      }
    }

    public EntryHashAlgorithm EntryHashAlgorithm {
      get
      {
        CheckDisposed();
        return entryHashAlgorithm;
      }
      set
      {
        CheckDisposed();
        entryHashAlgorithm = value;
      }
    }

    public bool DiscardFeedSource {
      get
      {
        CheckDisposed();
        return discardFeedSource;
      }
      set
      {
        CheckDisposed();
        discardFeedSource = value;
      }
    }

    public X509CertificateCollection ClientCertificates {
      get
      {
        CheckDisposed();
        return clientCertificates;
      }
    }

    public bool ThrowIfRedirected {
      get; set;
    }

    public bool ThrowIfClientError {
      get; set;
    }

    public bool ThrowIfServerError {
      get; set;
    }

    public Aggregator()
    {
      this.ThrowIfRedirected  = false;
      this.ThrowIfClientError = true;
      this.ThrowIfServerError = false;
    }

    public void Dispose()
    {
      disposed = true;
    }

    public bool Aggregate(AggregationContext context)
    {
      CheckDisposed();

      context.LastStatusCode = (HttpStatusCode)0;

      var requestUri = context.FeedUri;

      for (var redirections = 0;; redirections++) {
        var request = CreateRequest(context, requestUri);

        request.Timeout = requestTimeout;

        bool redirected;

        var aggregated = GetResponse(context, request, ref requestUri, out redirected);

        if (redirected) {
          if (maximumRedirections <= redirections)
            throw new AggregationException("maximum redirections exceeded");
        }
        else {
          return aggregated;
        }
      } // for redirection
    }

    private WebRequest CreateRequest(AggregationContext context, Uri requestUri)
    {
      var request = WebRequest.Create(requestUri);

      if (request is HttpWebRequest) {
        var httpRequest = request as HttpWebRequest;

        // HTTP headers
        httpRequest.Accept = "text/xml,application/xml,application/atom+xml,application/rss+xml;q=0.9,*/*;q=0.5";
        httpRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
        httpRequest.KeepAlive = true;
        httpRequest.UserAgent = userAgent;

        if (DateTime.MinValue < context.LastModified)
          httpRequest.IfModifiedSince = context.LastModified;

        if (!string.IsNullOrEmpty(context.LastETag))
          httpRequest.Headers.Add("If-None-Match", context.LastETag);

        // client behavior
        httpRequest.AllowAutoRedirect = false;

        // authorization and authentication
        httpRequest.UseDefaultCredentials = false;
        httpRequest.Credentials = context.Credentials;

        if (requestUri.Scheme == Uri.UriSchemeHttps)
          httpRequest.ClientCertificates = clientCertificates;
      }

      return request;
    }

    private bool GetResponse(AggregationContext context, WebRequest request, ref Uri requestUri, out bool redirected)
    {
      redirected = false;

      try {
        using (var response = request.GetResponse()) {
          var hasContent = false;

          if (response is HttpWebResponse) {
            var httpResponse = response as HttpWebResponse;

            UpdateContextByHttpResponse(context, httpResponse);

            switch (httpResponse.StatusCode) {
              case HttpStatusCode.NotModified:
                break;

              case HttpStatusCode.OK:
                hasContent = true;
                if (httpResponse.LastModified == DateTime.MinValue)
                  context.LastModified = DateTime.Now;
                else
                  context.LastModified  = httpResponse.LastModified;
                break;

              default:
                break;
            }
          }
          else {
            hasContent = true;
            context.LastModified  = DateTime.Now;
            context.LastETag      = null;
          }

          if (hasContent) {
            ReadAndParseContent(context, response);
            return true;
          }
          else {
            return false;
          }
        }
      }
      catch (WebException ex) {
        switch (ex.Status) {
          case WebExceptionStatus.ProtocolError:
            break;
          default:
            throw new AggregationException(ex.Message, ex);
        }

        // protocol error
        if (ex.Response is HttpWebResponse) {
          var httpResponse = ex.Response as HttpWebResponse;

          UpdateContextByHttpResponse(context, httpResponse);

          switch ((int)httpResponse.StatusCode / 100) {
            case 3: // 3xx redirection
              if (ThrowIfRedirected) {
                throw new AggregationRedirectedException(httpResponse.StatusDescription, ex);
              }
              else {
                requestUri = httpResponse.ResponseUri;
                redirected = true;
              }
              break;

            case 4: // 4xx client error
              if (ThrowIfClientError &&
                  (string.IsNullOrEmpty(context.LastETag) || httpResponse.StatusCode != HttpStatusCode.PreconditionFailed))
                throw new AggregationClientErrorException(httpResponse.StatusDescription, ex);
              break;

            case 5: // 5xx server error
              if (ThrowIfServerError)
                throw new AggregationServerErrorException(httpResponse.StatusDescription, ex);
              break;
          } // switch status code
        } // if HttpWebResponse
        else {
          throw new AggregationException(ex.Message, ex);
        }
      } // catch WebException

      return false;
    }

    private void UpdateContextByHttpResponse(AggregationContext context, HttpWebResponse httpResponse)
    {
      context.LastStatusCode = httpResponse.StatusCode;

      var etag = httpResponse.Headers.Get("ETag");

      if (etag != null)
        context.LastETag = etag;
    }

    private void ReadAndParseContent(AggregationContext context, WebResponse response)
    {
      using (var responseStream = response.GetResponseStream()) {
        Stream feedSourceStream;

        if (discardFeedSource) {
          feedSourceStream = responseStream;
          context.FeedSource = null;
        }
        else {
          using (var bufferStream = new MemoryStream(0 <= response.ContentLength ? (int)response.ContentLength : 1024)) {
            var buffer = new byte[1024];

            for (;;) {
              var read = responseStream.Read(buffer, 0, buffer.Length);

              bufferStream.Write(buffer, 0, read);

              if (read <= 0)
                break;
            }

            bufferStream.Close();

            context.FeedSource = bufferStream.ToArray();
          }

          feedSourceStream = new MemoryStream(context.FeedSource, false);
        }

        try {
          context.Feed = Parser.Parse(feedSourceStream, discardFeedSource, entryHashAlgorithm);
        }
        catch (FeedFormatException) {
          throw;
        }
      }
    }

    private void CheckDisposed()
    {
      if (disposed)
        throw new ObjectDisposedException(GetType().FullName);
    }

    private bool disposed = false;

    private int requestTimeout = DefaultRequestTimeout;
    private int maximumRedirections = DefaultMaximumRedirections;
    private string userAgent = DefaultUserAgent;
    private EntryHashAlgorithm entryHashAlgorithm = null;
    private bool discardFeedSource = true;
    private X509CertificateCollection clientCertificates = new X509CertificateCollection();
  }
}
