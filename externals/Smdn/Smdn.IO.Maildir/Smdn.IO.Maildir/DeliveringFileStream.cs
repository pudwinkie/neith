using System;
using System.IO;

namespace Smdn.IO.Maildir {
  internal class DeliveringFileStream : FileStream {
    internal MessageFileInfo FileInfo {
      get; private set;
    }

    internal DeliveringFileStream(MessageFileInfo file)
      : base(file.FullName, FileMode.CreateNew, FileAccess.Write, FileShare.None)
    {
      this.FileInfo = file;
    }

    public override void Close()
    {
      base.Close();

      try {
        FileInfo.MoveToNew();
      }
      catch (IOException ex) {
        try {
          if (FileInfo.Exists)
            FileInfo.Delete();
        }
        catch (IOException) {
          // ignore
        }

        throw new MaildirException("deliver failed", ex);
      }
    }
  }
}
