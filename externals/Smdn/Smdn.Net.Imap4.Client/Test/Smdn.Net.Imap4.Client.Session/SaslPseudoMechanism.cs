using System;
using Smdn.Security.Authentication.Sasl;
using Smdn.Security.Authentication.Sasl.Client;

namespace Smdn.Net.Imap4.Client.Session {
  [SaslMechanism("X-PSEUDO-MECHANISM", false)]
  internal class SaslPseudoMechanism : SaslClientMechanism {
    private int step;
    private readonly int maxStep;

    public SaslPseudoMechanism(int maxStep)
    {
      this.maxStep = maxStep;
    }

    public override bool ClientFirst {
      get { return true; }
    }

    public override void Initialize()
    {
      step = 0;

      base.Initialize();
    }

    protected override SaslExchangeStatus Exchange(ByteString serverChallenge, out ByteString clientResponse)
    {
      if (maxStep <= step) {
        clientResponse = null;
        return SaslExchangeStatus.Failed;
      }
      else {
        clientResponse = ByteString.CreateImmutable(string.Format("step{0}", step++));

        if (step == maxStep)
          return SaslExchangeStatus.Succeeded;
        else
          return SaslExchangeStatus.Continuing;
      }
    }
  }
}
