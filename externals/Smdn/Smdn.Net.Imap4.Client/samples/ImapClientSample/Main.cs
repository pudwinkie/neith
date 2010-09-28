// ドキュメントおよびサンプルは以下のページにあります
// http://smdn.invisiblefulmoon.net/works/libs/Smdn.Net.Imap4/doc/

using System;

using Smdn.Net.Imap4.Client;
using Smdn.Net.Imap4.Protocol.Client;

class MainClass {
  public static void Main(string[] args)
  {
    ImapConnection.ServerCertificateValidationCallback += delegate {
      // 適切な証明書の検証を行うように書き換えてください
      return true;
    };

    using (var client = new ImapClient(new Uri("imap://user@imap.example.net/"))) {
      client.Connect("pass");

      Console.WriteLine("connected!!");
    }
  }
}