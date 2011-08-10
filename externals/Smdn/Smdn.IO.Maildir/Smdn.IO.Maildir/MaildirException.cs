using System;
using System.IO;

namespace Smdn.IO.Maildir {
  public class MaildirException : IOException {
    public MaildirException(string message)
       : base(message)
    {
    }

    public MaildirException(string message, Exception innerException)
       : base(message, innerException)
    {
    }
  }
}
