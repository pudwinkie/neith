using System;
using System.IO;

namespace Smdn.IO.Maildir {
  public class Reader {
    public MaildirInfo Maildir {
      get { return maildir; }
    }

    public Reader(string maildir)
    {
      this.maildir = new MaildirInfo(maildir);
    }

    public FileInfo[] GetNewEntries()
    {
      return null;
    }

    private MaildirInfo maildir;
  }
}
