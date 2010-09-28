// ドキュメントおよびサンプルは以下のページにあります
// http://smdn.invisiblefulmoon.net/works/libs/Smdn.Net.Pop3/doc/

using System;

using Smdn.Net.Pop3.Client;
using Smdn.Net.Pop3.Protocol.Client;

class MainClass {
  public static void Main(string[] args)
  {
    PopConnection.ServerCertificateValidationCallback += delegate {
      // 適切な証明書の検証を行うように書き換えてください
      return true;
    };

    using (var client = new PopClient(new Uri("pop://user@pop.example.net/"))) {
      client.Connect("pass");

      Console.WriteLine("connected!!");
    }
  }
}