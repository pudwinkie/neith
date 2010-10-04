// ドキュメントおよびサンプルは以下のページにあります
// http://smdn.invisiblefulmoon.net/works/libs/Smdn.Net.Pop3/doc/

using System;
using System.Net;

using Smdn.Net.Pop3.WebClients;

class MainClass {
  public static void Main(string[] args)
  {
    PopWebRequestCreator.RegisterPrefix();
    PopSessionManager.ServerCertificateValidationCallback += delegate {
      // 適切な証明書の検証を行うように書き換えてください
      return true;
    };

    var request = WebRequest.Create("pop://user@pop.example.net/") as PopWebRequest;

    request.Credentials = new NetworkCredential("user", "pass");
    request.Method = PopWebRequestMethods.NoOp;
    request.KeepAlive = false;

    using (var response = request.GetResponse()) {
      Console.WriteLine("connected!!");
    }
  }
}
