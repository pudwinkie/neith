using System;
using Smdn.Security.Authentication.Sasl;
using Smdn.Security.Authentication.Sasl.Client;

namespace Smdn.Net.Pop3.Client.Session {
  [SaslMechanism("X-PSEUDO-MECHANISM", false)]
  internal class SaslPseudoMechanism : SaslClientMechanism {
    private int step;
    private readonly bool clientFirst;
    private readonly int maxStep;

    public SaslPseudoMechanism(bool clientFirst, int maxStep)
    {
      this.clientFirst = clientFirst;
      this.maxStep = maxStep;
    }

    public override bool ClientFirst {
      get { return clientFirst; }
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
        clientResponse = new ByteString(string.Format("step{0}", step++));

        if (step == maxStep)
          return SaslExchangeStatus.Succeeded;
        else
          return SaslExchangeStatus.Continuing;
      }
    }
  }
}
