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

#if false

using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Smdn.Security.Authentication.Sasl.Server {
  public abstract class SaslServerMechanism : IDisposable {
    /*
     * class members
     */
    static SaslServerMechanism()
    {
      var type = typeof(SaslServerMechanism);

      foreach (var t in Assembly.GetExecutingAssembly().GetTypes()) {
        if (t.IsAbstract || !type.IsAssignableFrom(t))
          continue;

        var attrs = t.GetCustomAttributes(typeof(SaslMechanismAttribute), false);

        if (attrs == null || attrs.Length <= 0)
          continue;

        knownMechanismTypes.Add((attrs[0] as SaslMechanismAttribute).Name, t);
      }
    }

    public static SaslServerMechanism Create(string mechanismName)
    {
      if (string.IsNullOrEmpty(mechanismName))
        throw new ArgumentException("invalid mechanism name", "mechanismName");

      try {
        if (knownMechanismTypes.ContainsKey(mechanismName))
          return (SaslServerMechanism)Activator.CreateInstance(knownMechanismTypes[mechanismName]);
        else
          throw new SaslMechanismNotSupportedException(mechanismName);
      }
      catch (TargetInvocationException ex) {
        if (ex.InnerException is NotSupportedException)
          throw new SaslMechanismNotSupportedException(mechanismName, ex.InnerException);
        else
          throw new SaslException("can't create instance", ex);
      }
    }

    private static Dictionary<string, Type> knownMechanismTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

    /*
     * instance members
     */
    /// <value>see http://www.iana.org/assignments/gssapi-service-names</value>
    public string ServiceName {
      get; set;
    }

    protected SaslServerMechanism()
    {
    }

    ~SaslServerMechanism()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      disposed = true;
    }

    public virtual void Initialize()
    {
      if (disposed)
        throw new ObjectDisposedException(GetType().FullName);
    }

    public SaslExchangeStatus Exchange(byte[] clientResponse, out byte[] serverChallenge)
    {
      if (disposed)
        throw new ObjectDisposedException(GetType().FullName);

      ByteString resp;

      var status = Exchange(new ByteString(clientResponse), out resp);

      serverChallenge = (resp == null) ? null : resp.ByteArray;

      return status;
    }

    protected abstract SaslExchangeStatus Exchange(ByteString clientResponse, out ByteString serverChallenge);

    private bool disposed = false;
  }
}

#endif