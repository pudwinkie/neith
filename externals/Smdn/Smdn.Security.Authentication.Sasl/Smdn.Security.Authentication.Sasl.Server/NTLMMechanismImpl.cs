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

#if false

#define MONO_SECURITY_DLL
#undef MONO_SECURITY_DLL

using System;
using System.Net;

using Smdn.Interop.Sspi;

#if MONO_SECURITY_DLL
using Mono.Security.Protocol.Ntlm;
#else
using System.Reflection;
#endif

namespace Smdn.Security.Authentication.Sasl.Server {
  internal abstract class NTLMMechanismImpl : IDisposable {
    public NetworkCredential Credential {
      get; set;
    }

    public string TargetHost {
      get; set;
    }

    public abstract void Initialize();
    public abstract void Dispose();
    public abstract void Negotiate(byte[] clientResponse, out byte[] serverChallenge);
    public abstract void Authenticate(byte[] clientResponse, out byte[] serverChallenge);

    public static NTLMMechanismImpl Create()
    {
      try {
        return new SspiNTLMMechanismImpl();
      }
      catch (NotSupportedException) {
        return new MonoNTLMMechanismImpl();
      }
    }
  }

  internal class MonoNTLMMechanismImpl : NTLMMechanismImpl {
    public MonoNTLMMechanismImpl()
    {
#if !MONO_SECURITY_DLL
      try {
        var assm = Assembly.Load("Mono.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756");

        typeOfType1Message = assm.GetType("Mono.Security.Protocol.Ntlm.Type1Message", true);
        typeOfType2Message = assm.GetType("Mono.Security.Protocol.Ntlm.Type2Message", true);
        typeOfType3Message = assm.GetType("Mono.Security.Protocol.Ntlm.Type3Message", true);
      }
      catch (System.IO.FileNotFoundException) {
        throw new PlatformNotSupportedException();
      }
      catch (System.IO.FileLoadException) {
        throw new PlatformNotSupportedException();
      }
      catch (BadImageFormatException) {
        throw new PlatformNotSupportedException();
      }
      catch (TypeLoadException) {
        throw new PlatformNotSupportedException();
      }
#endif
    }

    public override void Initialize()
    {
      // nothing to do
    }

    public override void Dispose()
    {
      // nothing to do
    }

    public override void Negotiate(byte[] clientResponse, out byte[] serverChallenge)
    {
#if MONO_SECURITY_DLL
      var type1 = new Type1Message(clientResponse);
      var type2 = new Type2Message();

      if ((int)(type1.Flags & NtlmFlags.NegotiateUnicode) != 0)
        type2.Flags |= NtlmFlags.NegotiateUnicode;
      else if ((int)(type1.Flags & NtlmFlags.NegotiateOem) != 0)
        type2.Flags |= NtlmFlags.NegotiateOem;

      type2.Nonce = Nonce.Create(8, false);

      serverChallenge = type2.GetBytes();
#else
      var type1 = Activator.CreateInstance(typeOfType1Message, clientResponse);
      var type2 = Activator.CreateInstance(typeOfType2Message);
      var type1Flags = (int)typeOfType1Message.GetProperty("Flags").GetValue(type1, null);
      var type2Flags = 0x00000200; // NegotiateNtlm

      if ((type1Flags & 0x00000001) != 0) // NegotiateUnicode
        type2Flags |= 0x00000001;
      else if ((type1Flags & 0x00000002) != 0) // NegotiateOem
        type2Flags |= 0x00000002;

      typeOfType2Message.GetProperty("Flags").SetValue(type2, type2Flags, null);
      typeOfType2Message.GetProperty("Nonce").SetValue(type2, Nonce.Generate(8, false), null);

      serverChallenge = (byte[])typeOfType2Message.GetMethod("GetBytes").Invoke(type2, null);
#endif
    }

    public override void Authenticate(byte[] clientResponse, out byte[] serverChallenge)
    {
#pragma warning disable 168
#if MONO_SECURITY_DLL
      var type3 = new Type1Message(clientResponse);
#else
      var type3 = Activator.CreateInstance(typeOfType3Message, clientResponse);
#endif
#pragma warning restore 168

      throw new NotImplementedException();
    }

#if !MONO_SECURITY_DLL
    private /*readonly*/ Type typeOfType1Message;
    private /*readonly*/ Type typeOfType2Message;
    private /*readonly*/ Type typeOfType3Message;
#endif
  }

  internal class SspiNTLMMechanismImpl : NTLMMechanismImpl {
    public SspiNTLMMechanismImpl()
    {
      try {
        SecPkgInfo.QuerySecurityPackageInfo("NTLM");
      }
      catch (TypeLoadException) { // DllNotFoundException, EntryPointNotFoundException
        throw new PlatformNotSupportedException();
      }
      catch (SspiException) {
        throw new NotSupportedException();
      }
    }

    public override void Initialize()
    {
      if (server != null)
        server.Dispose();

      server = new SspiServer();
    }

    public override void Dispose()
    {
      if (server != null) {
        server.Dispose();
        server = null;
      }
    }

    public override void Negotiate(byte[] clientResponse, out byte[] serverChallenge)
    {
      //server.AcquireCredentials(base.Credential, "NTLM");
      server.AcquireCredentials(null, "NTLM");
      server.InitializeSecurityContext(clientResponse, out serverChallenge);
    }

    public override void Authenticate(byte[] clientResponse, out byte[] serverChallenge)
    {
      server.InitializeSecurityContext(clientResponse, out serverChallenge);
    }

    private SspiServer server;
  }
}

#endif
#endif