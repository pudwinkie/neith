using System;
using NUnit.Framework;

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif
using Smdn.Net.Pop3.Client.Session;

namespace Smdn.Net.Pop3.Client {
  [TestFixture]
  public class PopClientOperationsTests {
    [Test]
    public void TestGetMessage()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // STAT
        server.EnqueueResponse("+OK 1 200\r\n");
        // LIST
        server.EnqueueResponse("+OK 1 200\r\n");

        var message = client.GetMessage(1L);

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 1\r\n");

        Assert.AreEqual(1L, client.MessageCount);
        Assert.AreEqual(200L, client.TotalSize);

        Assert.IsNotNull(message);
        Assert.AreEqual(1L, message.MessageNumber);
        Assert.AreEqual(200L, message.Length);

        // get again
        Assert.AreSame(message, client.GetMessage(1L));
      });
    }

    [Test]
    public void TestGetFirstMessage()
    {
      GetFirstMessage(false);
    }

    [Test]
    public void TestGetFirstMessageGetUniqueId()
    {
      GetFirstMessage(true);
    }

    private void GetFirstMessage(bool getUniqueId)
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // STAT
        server.EnqueueResponse("+OK 5 1200\r\n");
        // LIST
        server.EnqueueResponse("+OK 1 200\r\n");

        if (getUniqueId)
          // UIDL
          server.EnqueueResponse("+OK 1 whqtswO00WBw418f9t5JxYwZ\r\n");

        var message = getUniqueId
          ? client.GetFirstMessage(true)
          : client.GetFirstMessage();

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 1\r\n");

        if (getUniqueId)
          Assert.AreEqual(server.DequeueRequest(), "UIDL 1\r\n");

        Assert.AreEqual(5L, client.MessageCount);
        Assert.AreEqual(1200L, client.TotalSize);

        Assert.IsNotNull(message);
        Assert.AreEqual(1L, message.MessageNumber);
        Assert.AreEqual(200L, message.Length);

        if (getUniqueId)
          Assert.AreEqual("whqtswO00WBw418f9t5JxYwZ", message.UniqueId);

        // get again
        Assert.AreSame(message, getUniqueId ? client.GetFirstMessage(true) : client.GetFirstMessage());
      });
    }

    [Test]
    public void TestGetLastMessage()
    {
      GetLastMessage(false);
    }

    [Test]
    public void TestGetLastMessageGetUniqueId()
    {
      GetLastMessage(true);
    }

    private void GetLastMessage(bool getUniqueId)
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // STAT
        server.EnqueueResponse("+OK 5 1200\r\n");
        // LIST
        server.EnqueueResponse("+OK 5 200\r\n");

        if (getUniqueId)
          // UIDL
          server.EnqueueResponse("+OK 5 whqtswO00WBw418f9t5JxYwZ\r\n");

        var message = getUniqueId
          ? client.GetLastMessage(true)
          : client.GetLastMessage();

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 5\r\n");

        if (getUniqueId)
          Assert.AreEqual(server.DequeueRequest(), "UIDL 5\r\n");

        Assert.AreEqual(5L, client.MessageCount);
        Assert.AreEqual(1200L, client.TotalSize);

        Assert.IsNotNull(message);
        Assert.AreEqual(5L, message.MessageNumber);
        Assert.AreEqual(200L, message.Length);

        if (getUniqueId)
          Assert.AreEqual("whqtswO00WBw418f9t5JxYwZ", message.UniqueId);

        // get again
        Assert.AreSame(message, getUniqueId ? client.GetLastMessage(true) : client.GetLastMessage());
      });
    }

    [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestGetFirstMessageNoMessagesExist()
    {
      GetFirstOrLastMessage(delegate(PopClient client) {
        Assert.IsNull(client.GetFirstMessage());
      });
    }

    [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestGetLastMessageNoMessagesExist()
    {
      GetFirstOrLastMessage(delegate(PopClient client) {
        Assert.IsNull(client.GetLastMessage());
      });
    }

    private void GetFirstOrLastMessage(Action<PopClient> action)
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // STAT
        server.EnqueueResponse("+OK 0 0\r\n");

        action(client);
      });
    }

    [Test]
    public void TestGetMessageGetUniqueId()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // STAT
        server.EnqueueResponse("+OK 1 200\r\n");
        // LIST
        server.EnqueueResponse("+OK 1 200\r\n");
        // UIDL
        server.EnqueueResponse("+OK 1 whqtswO00WBw418f9t5JxYwZ\r\n");

        var message = client.GetMessage(1L, true);

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 1\r\n");
        Assert.AreEqual(server.DequeueRequest(), "UIDL 1\r\n");

        Assert.AreEqual(1L, client.MessageCount);
        Assert.AreEqual(200L, client.TotalSize);

        Assert.IsNotNull(message);
        Assert.AreEqual(1L, message.MessageNumber);
        Assert.AreEqual(200L, message.Length);
        Assert.AreEqual("whqtswO00WBw418f9t5JxYwZ", message.UniqueId);
      });
    }

    [Test]
    public void TestGetMessageDeleted()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // STAT
        server.EnqueueResponse("+OK 1 200\r\n");
        // LIST
        server.EnqueueResponse("+OK 1 200\r\n");

        var message = client.GetMessage(1L);

        Assert.AreEqual(1L, client.MessageCount);
        Assert.AreEqual(200L, client.TotalSize);

        // DELE
        server.EnqueueResponse("+OK\r\n");

        message.MarkAsDeleted();

        Assert.IsTrue(message.IsMarkedAsDeleted);

        try {
          client.GetMessage(1L);
          Assert.Fail("PopMessageDeletedException not thrown");
        }
        catch (PopMessageDeletedException ex) {
          Assert.AreEqual(1L, ex.MessageNumber);
        }
      });
    }

    [Test]
    public void TestGetMessageNotFound()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // STAT
        server.EnqueueResponse("+OK 1 120\r\n");
        // LIST
        server.EnqueueResponse("-ERR no such message\r\n");

        try {
          client.GetMessage(1L);
          Assert.Fail("PopMessageNotFoundException not thrown");
        }
        catch (PopMessageNotFoundException ex) {
          Assert.AreEqual(1L, ex.MessageNumber);
          Assert.IsNull(ex.UniqueId);
        }
      });
    }

    [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestGetMessageZero()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        client.GetMessage(0L);
      });
    }

    [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestGetMessageGreaterThanMessageCount()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // STAT
        server.EnqueueResponse("+OK 3 1024\r\n");

        client.GetMessage(4L);
      });
    }

    [Test, ExpectedException(typeof(InvalidOperationException))]
    public void TestGetMessageNotConnected()
    {
      using (var client = new PopClient()) {
        client.GetMessage(1L);
      }
    }

    [Test]
    public void TestGetMessageReconnect()
    {
      var sessionMessages = new PopMessageInfo[2];

      for (var i = 0; i < 2; i++) {
        TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
          // STAT
          server.EnqueueResponse("+OK 1 200\r\n");
          // LIST
          server.EnqueueResponse("+OK 1 200\r\n");

          var message = client.GetMessage(1L);

          Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
          Assert.AreEqual(server.DequeueRequest(), "LIST 1\r\n");

          Assert.AreEqual(1L, client.MessageCount);
          Assert.AreEqual(200L, client.TotalSize);

          Assert.IsNotNull(message);
          Assert.AreEqual(1L, message.MessageNumber);
          Assert.AreEqual(200L, message.Length);

          sessionMessages[i] = message;
        });
      }

      Assert.AreNotEqual(sessionMessages[0], sessionMessages[1]);
    }

    [Test]
    public void TestGetMessageByUid()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // UIDL
        server.EnqueueResponse("+OK\r\n" +
                               "1 whqtswO00WBw418f9t5JxYwZ\r\n" + 
                               "2 QhdPYR:00WBw1Ph7x7\r\n" + 
                               "3 xxxxxxxxx\r\n" + 
                               ".\r\n");
        // LIST
        server.EnqueueResponse("+OK 2 240\r\n");

        var message = client.GetMessage("QhdPYR:00WBw1Ph7x7");

        Assert.AreEqual(server.DequeueRequest(), "UIDL\r\n");
        Assert.AreEqual(server.DequeueRequest(), "LIST 2\r\n");

        Assert.AreEqual(2L, message.MessageNumber);
        Assert.AreEqual(240L, message.Length);
        Assert.AreEqual("QhdPYR:00WBw1Ph7x7", message.UniqueId);
      });
    }

    [Test]
    public void TestGetMessageByUidError()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // UIDL
        server.EnqueueResponse("-ERR\r\n");

        try {
          client.GetMessage("QhdPYR:00WBw1Ph7x7");
          Assert.Fail("PopErrorResponseException not thrown");
        }
        catch (PopErrorResponseException) {
        }

        Assert.AreEqual(server.DequeueRequest(), "UIDL\r\n");
      });
    }

    [Test]
    public void TestGetMessageByUidDeleted()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // UIDL
        server.EnqueueResponse("+OK\r\n" +
                               "1 whqtswO00WBw418f9t5JxYwZ\r\n" + 
                               "2 QhdPYR:00WBw1Ph7x7\r\n" + 
                               "3 xxxxxxxxx\r\n" + 
                               ".\r\n");
        // LIST
        server.EnqueueResponse("+OK 2 240\r\n");

        var message = client.GetMessage("QhdPYR:00WBw1Ph7x7");

        // DELE
        server.EnqueueResponse("+OK\r\n");

        message.MarkAsDeleted();

        try {
          client.GetMessage(message.UniqueId);
          Assert.Fail("PopMessageDeletedException not thrown");
        }
        catch (PopMessageDeletedException ex) {
          Assert.AreEqual(message.MessageNumber, ex.MessageNumber);
        }
      });
    }

    [Test]
    public void TestGetMessageByUidAlreadyListed()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // LIST
        server.EnqueueResponse("+OK\r\n" +
                               "1 120\r\n" +
                               "2 240\r\n" +
                               "3 360\r\n" +
                               ".\r\n");
        // UIDL
        server.EnqueueResponse("+OK\r\n" +
                               "1 whqtswO00WBw418f9t5JxYwZ\r\n" + 
                               "2 QhdPYR:00WBw1Ph7x7\r\n" + 
                               "3 xxxxxxxxx\r\n" + 
                               ".\r\n");

        var messages = client.GetMessages(true).ToArray();

        Assert.AreEqual(server.DequeueRequest(), "LIST\r\n");
        Assert.AreEqual(server.DequeueRequest(), "UIDL\r\n");

        Assert.AreEqual("QhdPYR:00WBw1Ph7x7", messages[1].UniqueId);

        var message = client.GetMessage("QhdPYR:00WBw1Ph7x7");

        Assert.IsNotNull(message);
        Assert.AreEqual(2L, message.MessageNumber);
        Assert.AreSame(messages[1], message);
      });
    }

    [Test, ExpectedException(typeof(ArgumentNullException))]
    public void TestGetMessageByUidNull()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        client.GetMessage((string)null);
      });
    }

    [Test, ExpectedException(typeof(ArgumentException))]
    public void TestGetMessageByUidEmpty()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        client.GetMessage(string.Empty);
      });
    }

    [Test]
    public void TestGetMessageByUidNotFound()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // UIDL
        server.EnqueueResponse("+OK\r\n" +
                               "1 whqtswO00WBw418f9t5JxYwZ\r\n" + 
                               "2 QhdPYR:00WBw1Ph7x7\r\n" + 
                               "3 xxxxxxxxx\r\n" + 
                               ".\r\n");

        try {
          client.GetMessage("non-existent");
          Assert.Fail("PopMessageNotFoundException not thrown");
        }
        catch (PopMessageNotFoundException ex) {
          Assert.AreEqual(0L, ex.MessageNumber);
          Assert.IsNotNull(ex.UniqueId);
          Assert.AreEqual("non-existent", ex.UniqueId);
        }
      });
    }

    [Test]
    public void TestGetMessages()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // LIST
        server.EnqueueResponse("+OK\r\n" +
                               "1 120\r\n" +
                               "2 240\r\n" +
                               "3 360\r\n" +
                               ".\r\n");

        var messages = client.GetMessages().ToArray();

        Assert.AreEqual(server.DequeueRequest(), "LIST\r\n");

        Assert.AreEqual(3, messages.Length);

        Assert.AreEqual(1L, messages[0].MessageNumber);
        Assert.AreEqual(120L, messages[0].Length);

        Assert.AreEqual(2L, messages[1].MessageNumber);
        Assert.AreEqual(240L, messages[1].Length);

        Assert.AreEqual(3L, messages[2].MessageNumber);
        Assert.AreEqual(360L, messages[2].Length);

        // LIST
        server.EnqueueResponse("+OK\r\n" +
                               "1 120\r\n" +
                               "2 240\r\n" +
                               "3 360\r\n" +
                               ".\r\n");

        var messagesSecond = client.GetMessages().ToArray();

        Assert.AreSame(messages[0], messagesSecond[0]);
        Assert.AreSame(messages[1], messagesSecond[1]);
        Assert.AreSame(messages[2], messagesSecond[2]);
      });
    }

    [Test]
    public void TestGetMessagesGetUniqueId()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // LIST
        server.EnqueueResponse("+OK\r\n" +
                               "1 120\r\n" +
                               "2 240\r\n" +
                               "3 360\r\n" +
                               ".\r\n");
        // UIDL
        server.EnqueueResponse("+OK\r\n" +
                               "1 whqtswO00WBw418f9t5JxYwZ\r\n" + 
                               "2 QhdPYR:00WBw1Ph7x7\r\n" + 
                               "3 xxxxxxxxx\r\n" + 
                               ".\r\n");

        var messages = client.GetMessages(true).ToArray();

        Assert.AreEqual(server.DequeueRequest(), "LIST\r\n");

        Assert.AreEqual(3, messages.Length);

        Assert.AreEqual(1L, messages[0].MessageNumber);
        Assert.AreEqual(120L, messages[0].Length);
        Assert.AreEqual("whqtswO00WBw418f9t5JxYwZ", messages[0].UniqueId);

        Assert.AreEqual(2L, messages[1].MessageNumber);
        Assert.AreEqual(240L, messages[1].Length);
        Assert.AreEqual("QhdPYR:00WBw1Ph7x7", messages[1].UniqueId);

        Assert.AreEqual(3L, messages[2].MessageNumber);
        Assert.AreEqual(360L, messages[2].Length);
        Assert.AreEqual("xxxxxxxxx", messages[2].UniqueId);
      });
    }

    [Test]
    public void TestGetMessagesEmpty()
    {
      GetMessagesEmpty(false);
    }

    [Test]
    public void TestGetMessagesEmptyGetUniqueId()
    {
      GetMessagesEmpty(true);
    }

    private void GetMessagesEmpty(bool getUniqueId)
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // LIST
        server.EnqueueResponse("+OK\r\n" +
                               ".\r\n");

        var messages = (getUniqueId ? client.GetMessages(true) : client.GetMessages()).ToArray();

        Assert.AreEqual(server.DequeueRequest(), "LIST\r\n");

        Assert.AreEqual(0, messages.Length);
      });
    }

    [Test]
    public void TestCancelDelete()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // STAT
        server.EnqueueResponse("+OK 1 200\r\n");

        Assert.AreEqual(1L, client.MessageCount);
        Assert.AreEqual(200L, client.TotalSize);

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");

        // LIST
        server.EnqueueResponse("+OK 1 200\r\n");

        var message = client.GetMessage(1L);

        server.DequeueRequest(); // LIST

        // DELE
        server.EnqueueResponse("+OK\r\n");

        message.MarkAsDeleted();

        server.DequeueRequest();

        Assert.IsTrue(message.IsMarkedAsDeleted);

        // STAT
        server.EnqueueResponse("+OK 0 0\r\n");

        Assert.AreEqual(0L, client.MessageCount);
        Assert.AreEqual(0L, client.TotalSize);

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");

        // RSET
        server.EnqueueResponse("+OK\r\n");

        client.CancelDelete();

        Assert.AreEqual(server.DequeueRequest(), "RSET\r\n");

        Assert.IsFalse(message.IsMarkedAsDeleted);

        // STAT
        server.EnqueueResponse("+OK 1 200\r\n");

        Assert.AreEqual(1L, client.MessageCount);
        Assert.AreEqual(200L, client.TotalSize);

        Assert.AreEqual(server.DequeueRequest(), "STAT\r\n");
      });
    }

    [Test]
    public void TestCancelDeleteMessageNotListed()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // RSET
        server.EnqueueResponse("+OK\r\n");

        client.CancelDelete();

        Assert.AreEqual(server.DequeueRequest(), "RSET\r\n");
      });
    }

    [Test]
    public void TestKeepAlive()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        // NOOP
        server.EnqueueResponse("+OK\r\n");

        client.KeepAlive();

        Assert.AreEqual(server.DequeueRequest(), "NOOP\r\n");
      });
    }

    [Test, ExpectedException(typeof(Smdn.Net.Pop3.Protocol.PopConnectionException))]
    public void TestKeepAliveDisconnected()
    {
      TestUtils.TestAuthenticated(delegate(PopPseudoServer server, PopClient client) {
        server.Stop();

        client.KeepAlive();
      });
    }
  }
}
