// ドキュメントおよびサンプルは以下のページにあります
// http://smdn.invisiblefulmoon.net/works/libs/Smdn.Net.Imap4/doc/

using System;
using System.Net;

using Smdn.Net.Imap4.WebClients;

class MainClass {
  public static void Main(string[] args)
  {
    ImapWebRequestCreator.RegisterPrefix();
    ImapSessionManager.ServerCertificateValidationCallback += delegate {
      // 適切な証明書の検証を行うように書き換えてください
      return true;
    };

    var request = WebRequest.Create("imap://user@imap.example.net/") as ImapWebRequest;

    request.Credentials = new NetworkCredential("user", "pass");
    request.Method = ImapWebRequestMethods.NoOp;
    request.KeepAlive = false;

    using (var response = request.GetResponse()) {
      Console.WriteLine("connected!!");
    }
  }
}
