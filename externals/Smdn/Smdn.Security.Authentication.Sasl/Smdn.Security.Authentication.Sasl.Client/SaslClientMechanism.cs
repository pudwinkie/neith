// 
// Author:
//       smdn <smdn@mail.invisiblefulmoon.net>
// 
// Copyright (c) 2008-2010 smdn
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
using System.Net;
using System.Reflection;

namespace Smdn.Security.Authentication.Sasl.Client {
  public abstract class SaslClientMechanism : IDisposable {
    /*
     * class members
     */
    static SaslClientMechanism()
    {
      var type = typeof(SaslClientMechanism);

      foreach (var t in Assembly.GetExecutingAssembly().GetTypes()) {
        if (t.IsAbstract || !type.IsAssignableFrom(t))
          continue;

        RegisterMechanism(t);
      }
    }

    public static SaslClientMechanism Create(string mechanismName)
    {
      if (string.IsNullOrEmpty(mechanismName))
        throw new ArgumentException("invalid mechanism name", "mechanismName");

      try {
        Type mechanismType;

        if (knownMechanismTypes.TryGetValue(mechanismName, out mechanismType))
          return (SaslClientMechanism)Activator.CreateInstance(mechanismType);
        else
          throw new SaslMechanismNotSupportedException(mechanismName);
      }
      catch (TargetInvocationException ex) {
        if (ex.InnerException is NotSupportedException)
          throw new SaslMechanismNotSupportedException(mechanismName, ex.InnerException);
        else
          throw new SaslException("can't create instance", ex.InnerException);
      }
    }

    public static bool IsMechanismPlainText(string mechanismName)
    {
      if (string.IsNullOrEmpty(mechanismName))
        throw new ArgumentException("invalid mechanism name", "mechanismName");

      Type mechanismType;

      if (knownMechanismTypes.TryGetValue(mechanismName, out mechanismType))
        return GetAttributeFrom(mechanismType).IsPlainText;
      else
        throw new SaslMechanismNotSupportedException(mechanismName);
    }

    public static string[] GetAvailableMechanisms()
    {
      return (new List<string>(knownMechanismTypes.Keys)).ToArray();
    }

    public static void RegisterMechanism(Type mechanismType)
    {
      if (mechanismType == null)
        throw new ArgumentNullException("mechanismType");
      else if (mechanismType.IsAbstract)
        throw new ArgumentException("can't register abstract class type", "mechanismType");
      else if (!typeof(SaslClientMechanism).IsAssignableFrom(mechanismType))
        throw new ArgumentException("type is not SaslClientMechanism", "mechanismType");

      var attr = GetAttributeFrom(mechanismType);

      if (attr == null)
        throw new ArgumentException("SaslMechanismNameAttribute not applied", "mechanismType");

      if (string.IsNullOrEmpty(attr.Name))
        throw new ArgumentException("invalid mechanism name", "mechanismType");

      lock (((System.Collections.ICollection)knownMechanismTypes).SyncRoot) {
        knownMechanismTypes[attr.Name] = mechanismType;
      }
    }

    private static SaslMechanismAttribute GetAttributeFrom(Type mechanismType)
    {
      var attrs = mechanismType.GetCustomAttributes(typeof(SaslMechanismAttribute), false);

      if (attrs == null || attrs.Length <= 0)
        return null;
      else
        return (SaslMechanismAttribute)attrs[0];
    }

    private static Dictionary<string, Type> knownMechanismTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

    /*
     * instance members
     */
    public virtual bool ClientFirst  {
      get { return false; }
    }

    public string Name {
      get { return GetAttributeFrom(GetType()).Name; }
    }

    public bool IsPlainText {
      get { return GetAttributeFrom(GetType()).IsPlainText; }
    }

    public NetworkCredential Credential {
      get { CheckDisposed(); return credential; }
      set { CheckDisposed(); credential = value; }
    }

    /// <value>see http://www.iana.org/assignments/gssapi-service-names</value>
    public string ServiceName {
      get; set;
    }

    public SaslExchangeStatus ExchangeStatus {
      get { CheckDisposed(); return exchangeStatus; }
    }

    protected SaslClientMechanism()
    {
      Initialize();
    }

    ~SaslClientMechanism()
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

      credential = null;
    }

    public virtual void Initialize()
    {
      CheckDisposed();

      exchangeStatus = SaslExchangeStatus.None;
    }

    public virtual SaslExchangeStatus GetInitialResponse(out byte[] initialClientResponse)
    {
      if (ClientFirst)
        return Exchange(null, out initialClientResponse);
      else
        throw new NotSupportedException("initial response is not supported by this mechanism");
    }

    public SaslExchangeStatus Exchange(byte[] serverChallenge, out byte[] clientResponse)
    {
      CheckDisposed();

      CheckExchangeStatus();

      ByteString resp;

      exchangeStatus = Exchange((serverChallenge == null) ? ByteString.CreateEmpty() : new ByteString(serverChallenge), out resp);

      clientResponse = (resp == null) ? null : resp.ByteArray;

      return exchangeStatus;
    }

    protected abstract SaslExchangeStatus Exchange(ByteString serverChallenge, out ByteString clientResponse);

    protected void CheckDisposed()
    {
      if (disposed)
        throw new ObjectDisposedException(GetType().FullName);
    }

    protected void CheckExchangeStatus()
    {
      switch (exchangeStatus) {
        case SaslExchangeStatus.Succeeded: throw new InvalidOperationException("exchange already succeeded");
        case SaslExchangeStatus.Failed: throw new InvalidOperationException("exchange already failed");
        default: return;
      }
    }

    private bool disposed = false;
    private NetworkCredential credential = null;
    private SaslExchangeStatus exchangeStatus = SaslExchangeStatus.None;
  }
}
