// 
// Author:
//       smdn <smdn@mail.invisiblefulmoon.net>
// 
// Copyright (c) 2010 smdn
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
using System.Net;

using Smdn.Net.Pop3.Client.Session;

namespace Smdn.Net.Pop3.Client {
  public class PopClientProfile : IPopSessionProfile, ICloneable {
    public Uri Authority {
      get { return authority.Uri; }
    }

    public string Host {
      get { return authority.Host; }
      set { authority.Host = value; }
    }

    public int Port {
      get { return authority.Port; }
      set { authority.Port = value; }
    }

    public bool SecurePort {
      get { return string.Equals(authority.Scheme, PopUri.UriSchemePops, StringComparison.OrdinalIgnoreCase); }
      set
      {
        if (value)
          authority.Scheme = PopUri.UriSchemePops;
        else
          authority.Scheme = PopUri.UriSchemePop;
      }
    }

    public string UserName {
      get { return authority.UserName; }
      set { authority.UserName = value; }
    }

    public string AuthType {
      get { return (authority.AuthType == null) ? null : (string)authority.AuthType; }
      set
      {
        if (value == null)
          authority.AuthType = null;
        else
          authority.AuthType = PopAuthenticationMechanism.GetKnownOrCreate(value);
      }
    }

    private int timeout = System.Threading.Timeout.Infinite;

    public int Timeout {
      get { return timeout; }
      set
      {
        if (value < -1)
          throw new ArgumentOutOfRangeException("Timeout", value, "must be greater than or equals to -1");
        timeout = value;
      }
    }

    private int sendTimeout = System.Threading.Timeout.Infinite;

    public int SendTimeout {
      get { return sendTimeout; }
      set
      {
        if (value < -1)
          throw new ArgumentOutOfRangeException("SendTimeout", value, "must be greater than or equals to -1");
        sendTimeout = value;
      }
    }

    private int receiveTimeout = System.Threading.Timeout.Infinite;

    public int ReceiveTimeout {
      get { return receiveTimeout; }
      set
      {
        if (value < -1)
          throw new ArgumentOutOfRangeException("ReceiveTimeout", value, "must be greater than or equals to -1");
        receiveTimeout = value;
      }
    }

    private ICredentialsByHost credentials = null;

    ICredentialsByHost IPopSessionProfile.Credentials {
      get { return credentials; }
    }

    private bool useTlsIfAvailable = true;

    public bool UseTlsIfAvailable {
      get { return useTlsIfAvailable; }
      set { useTlsIfAvailable = value; }
    }

    private string[] usingSaslMechanisms = new[] {"DIGEST-MD5", "CRAM-MD5", "NTLM"};

    public string[] UsingSaslMechanisms {
      get { return usingSaslMechanisms; }
      set { usingSaslMechanisms = value; }
    }

    private bool allowInsecureLogin = false;

    public bool AllowInsecureLogin {
      get { return allowInsecureLogin; }
      set { allowInsecureLogin = value; }
    }

    /*
     * constructors
     */
    public PopClientProfile()
    {
      this.authority = new PopUriBuilder();
    }

    public PopClientProfile(Uri authority)
    {
      this.authority = new PopUriBuilder(authority);
    }

    public PopClientProfile(string host)
      : this()
    {
      this.Host = host;
    }

    public PopClientProfile(string host, string userName)
      : this()
    {
      this.Host = host;
      this.UserName = userName;
    }

    public PopClientProfile(string host, bool securePort, string userName)
      : this()
    {
      this.Host = host;
      this.SecurePort = securePort;
      this.UserName = userName;
    }

    public PopClientProfile(string host, int port)
      : this()
    {
      this.Host = host;
      this.Port = port;
    }

    public PopClientProfile(string host, int port, string userName)
      : this()
    {
      this.Host = host;
      this.Port = port;
      this.UserName = userName;
    }

    public PopClientProfile(string host, int port, bool securePort)
      : this()
    {
      this.Host = host;
      this.Port = port;
      this.SecurePort = securePort;
    }

    public PopClientProfile(string host, int port, bool securePort, string userName)
      : this()
    {
      this.Host = host;
      this.Port = port;
      this.SecurePort = securePort;
      this.UserName = userName;
    }

    public PopClientProfile(string host, int port, string userName, string authType)
      : this()
    {
      this.Host = host;
      this.Port = port;
      this.UserName = userName;
      this.AuthType = authType;
    }

    public PopClientProfile(string host, int port, bool securePort, string userName, string authType)
      : this()
    {
      this.Host = host;
      this.Port = port;
      this.SecurePort = securePort;
      this.UserName = userName;
      this.AuthType = authType;
    }

    public PopClientProfile(string host, int port, bool securePort, string userName, string authType, int timeout)
      : this()
    {
      this.Host = host;
      this.Port = port;
      this.SecurePort = securePort;
      this.UserName = userName;
      this.AuthType = authType;
      this.Timeout = timeout;
    }

    internal void SetCredentials(ICredentialsByHost credentials)
    {
      this.credentials = credentials;
    }

    public PopClientProfile Clone()
    {
      var cloned = (PopClientProfile)MemberwiseClone();

      cloned.authority = this.authority.Clone();

      if (this.usingSaslMechanisms == null)
        cloned.usingSaslMechanisms = null;
      else
        cloned.usingSaslMechanisms = (string[])this.usingSaslMechanisms.Clone();

      return cloned;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    private PopUriBuilder authority;
  }
}
