using System;
using System.IO;
using System.Runtime.Serialization;

namespace Smdn.IO.Maildir {
  public class MessageFileInfo : FileSystemInfo {
    public override bool Exists {
      get { return file.Exists; }
    }

    public override string FullName {
      get { return file.FullName; }
    }

    public override string Name {
      get { return file.Name; }
    }

    public DirectoryInfo Directory {
      get { return file.Directory; }
    }

    public string DirectoryName {
      get { return file.DirectoryName; }
    }

    public long Length {
      get { return file.Length; }
    }

    public MaildirInfo Maildir {
      get { return new MaildirInfo(file.Directory.Parent); }
    }

    public MessageFlags Flags {
      get { return ParseMessageFlags(); }
      set { SetMessageFlags(value); }
    }

    internal MessageFileInfo(string fileName)
      : this(new FileInfo(fileName))
    {
    }

    internal MessageFileInfo(FileInfo file)
    {
      this.file = file;
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      file.GetObjectData(info, context);
    }

    public FileInfo GetFileInfo()
    {
      return file;
    }

    public FileStream OpenRead()
    {
      return file.OpenRead();
    }

    public override void Delete()
    {
      file.Delete();
    }

    public void MoveToCur()
    {
      file.MoveTo(Path.Combine(Maildir.CurDirectory.FullName, Name));
    }

    public void MoveToNew()
    {
      file.MoveTo(Path.Combine(Maildir.NewDirectory.FullName, Name));
    }

    /*
     * managing info methods
     */
    private MessageFlags ParseMessageFlags()
    {
      return MessageFlags.None;
    }

    private void SetMessageFlags(MessageFlags flags)
    {
    }

    public override string ToString()
    {
      return file.ToString();
    }

    private FileInfo file;
  }
}
