using System;
using System.IO;
using System.Text;
using System.Threading;

using Smdn.Formats;

namespace Smdn.IO.Maildir {
  public class Deliverer {
    private static int numberOfDeliveries = 0;

    public MaildirInfo Maildir {
      get { return maildir; }
    }

    public virtual bool EncodeDirectoryName {
      get; set;
    }

    public Deliverer(string maildir)
    {
      this.maildir = new MaildirInfo(maildir);
      this.hostname = PathUtils.ReplaceInvalidFileNameChars(System.Net.Dns.GetHostName(), delegate(char ch, string str, int index) {
        return string.Format("\\{0}", (int)ch);
      });

      this.EncodeDirectoryName = true;
    }

    protected virtual string GetUniqueName()
    {
      var now = DateTime.Now;

      return string.Format("{0}.M{1}P{2}Q{3}.{4}",
                           UnixTimeStamp.ToInt64(now),
                           (long)(now.TimeOfDay.TotalMilliseconds * 1000.0) % 1000000, // microseconds
                           System.Diagnostics.Process.GetCurrentProcess().Id,
                           (uint)numberOfDeliveries,
                           hostname);
    }

    public FileStream CreateStream()
    {
      return CreateStream(10);
    }

    public FileStream CreateStream(int maxRetry)
    {
      return CreateStream(null, maxRetry);
    }

    public FileStream CreateStream(string directory)
    {
      return CreateStream(directory, 10);
    }

    public FileStream CreateStream(string directory, int maxRetry)
    {
      if (maxRetry < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("maxRetry", maxRetry);

      var dir = maildir;

      if (!string.IsNullOrEmpty(directory))
        dir = dir.CreateSubdirectory(ModifiedUTF7.Encode(directory));

      dir.EnsureDirectoriesExist();

      foreach (var d in new[] {dir.TmpDirectory, dir.NewDirectory}) {
        if ((int)(d.Attributes & FileAttributes.ReadOnly) != 0)
          throw new MaildirException(string.Format("directory '{0}' is not writable", d.FullName));
      }

      Interlocked.Increment(ref numberOfDeliveries);

      for (;;) {
        try {
          var file = new MessageFileInfo(Path.Combine(dir.TmpDirectory.FullName, GetUniqueName()));

          if (file.Exists)
            throw new MaildirException("file exists");

          return new DeliveringFileStream(file);
        }
        catch (IOException ex) {
          Thread.Sleep(2000);

          if (--maxRetry < 0)
            throw ex;
          else
            continue;
        }
      }
    }

    public MessageFileInfo Deliver(byte[] data)
    {
      return Deliver(null, data);
    }

    public MessageFileInfo Deliver(string directory, byte[] data)
    {
      return Deliver(directory, delegate(FileStream stream) {
        stream.Write(data, 0, data.Length);
      });
    }

    public MessageFileInfo Deliver(string text)
    {
      return Deliver(null, text);
    }

    public MessageFileInfo Deliver(string directory, string text)
    {
      return Deliver(directory, text, Encoding.ASCII);
    }

    public MessageFileInfo Deliver(string text, Encoding encoding)
    {
      return Deliver(null, text, encoding);
    }

    public MessageFileInfo Deliver(string directory, string text, Encoding encoding)
    {
      return Deliver(directory, delegate(FileStream stream) {
        var writer = new StreamWriter(stream, encoding);

        writer.Write(text);
        writer.Flush();
      });
    }

    public MessageFileInfo Deliver(Stream dataStream)
    {
      return Deliver(null, dataStream);
    }

    public MessageFileInfo Deliver(string directory, Stream dataStream)
    {
      return Deliver(directory, delegate(FileStream stream) {
        dataStream.CopyTo(stream, 1024);
      });
    }

    public MessageFileInfo Deliver(Action<FileStream> write)
    {
      return Deliver(null, write);
    }

    public MessageFileInfo Deliver(string directory, Action<FileStream> write)
    {
      if (write == null)
        throw new ArgumentNullException("write");

      using (var stream = CreateStream(directory)) {
        write(stream);

        return (stream as DeliveringFileStream).FileInfo;
      }
    }

    private string hostname;
    private MaildirInfo maildir;
  }
}
