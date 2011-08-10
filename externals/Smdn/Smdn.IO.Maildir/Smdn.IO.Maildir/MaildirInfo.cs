using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

using Smdn.Formats;

namespace Smdn.IO.Maildir {
  public class MaildirInfo : FileSystemInfo {
    /*
     * class members
     */
    public static MaildirInfo Create(string path)
    {
      return Create(path, true);
    }

    public static MaildirInfo Create(string path, bool createDirectory)
    {
      if (System.IO.Directory.Exists(path))
        return new MaildirInfo(path);
      else if (createDirectory)
        return new MaildirInfo(System.IO.Directory.CreateDirectory(path));
      else
        throw new DirectoryNotFoundException(string.Format("{0} not exist", path));
    }

    /*
     * instance members
     */
    public override string Name {
      get { return directory.Name; }
    }

    public string DecodedName {
      get { return ModifiedUTF7.Decode(directory.Name); }
    }

    public override string FullName {
      get { return directory.FullName; }
    }

    public override bool Exists {
      get { return directory.Exists; }
    }

    public DirectoryInfo Root {
      get { return directory.Root; }
    }

    public DirectoryInfo Parent {
      get { return directory.Parent; }
    }

    public DirectoryInfo TmpDirectory {
      get { return tmpDirectory; }
    }

    public DirectoryInfo NewDirectory {
      get { return newDirectory; }
    }

    public DirectoryInfo CurDirectory {
      get { return curDirectory; }
    }

    public MaildirInfo(string path)
      : this(new DirectoryInfo(path))
    {
    }

    public MaildirInfo(DirectoryInfo directory)
    {
      this.directory = directory;

      UpdateSubdirectoryInfo();
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      directory.GetObjectData(info, context);
    }

    public DirectoryInfo GetDirectoryInfo()
    {
      return directory;
    }

    public MaildirInfo CreateSubdirectory(string path)
    {
      var name = Path.GetDirectoryName(path);

      if (name == "tmp" || name == "new" || name == "cur")
        throw new MaildirException(string.Format("{0} is invalid path", path));

      var subMaildir = new MaildirInfo(Path.Combine(directory.FullName, path));

      subMaildir.EnsureDirectoriesExist();

      return subMaildir;
    }

    public override void Delete()
    {
      directory.Delete();
    }

    public void Delete(bool recursive)
    {
      directory.Delete(recursive);
    }

    public void MoveTo(string destDirName)
    {
      directory.MoveTo(destDirName);

      UpdateSubdirectoryInfo();
    }

    public void Create()
    {
      Create(true);
    }

    public void EnsureDirectoriesExist()
    {
      Create(false);
    }

    private void Create(bool nocheck)
    {
      if (nocheck || !directory.Exists)
        directory.Create();

      if (nocheck || !tmpDirectory.Exists)
        tmpDirectory = directory.CreateSubdirectory("tmp/");

      if (nocheck || !newDirectory.Exists)
        newDirectory = directory.CreateSubdirectory("new/");

      if (nocheck || !curDirectory.Exists)
        curDirectory = directory.CreateSubdirectory("cur/");
    }

    private void UpdateSubdirectoryInfo()
    {
      tmpDirectory = new DirectoryInfo(Path.Combine(directory.FullName, "tmp/"));
      newDirectory = new DirectoryInfo(Path.Combine(directory.FullName, "new/"));
      curDirectory = new DirectoryInfo(Path.Combine(directory.FullName, "cur/"));
    }

    public IEnumerable<MaildirInfo> GetDirectories()
    {
      return GetDirectories("*", SearchOption.TopDirectoryOnly);
    }

    public IEnumerable<MaildirInfo> GetDirectories(string searchPattern)
    {
      return GetDirectories(searchPattern, SearchOption.TopDirectoryOnly);
    }

    public IEnumerable<MaildirInfo> GetDirectories(string searchPattern, SearchOption searchOption)
    {
      switch (searchOption) {
        case SearchOption.TopDirectoryOnly: {
          var result = new List<MaildirInfo>();

          foreach (var dir in directory.GetDirectories(searchPattern, searchOption)) {
            if (dir.Name == "tmp" || dir.Name == "new" || dir.Name == "cur")
              continue;
            else
              result.Add(new MaildirInfo(dir));
          }

          return result;
        }

        case SearchOption.AllDirectories: {
          var queue = new Queue<MaildirInfo>(GetDirectories(searchPattern, SearchOption.TopDirectoryOnly));
          var result = new List<MaildirInfo>();

          while (0 < queue.Count) {
            var dir = queue.Dequeue();

            foreach (var subdir in dir.GetDirectories(searchPattern, SearchOption.TopDirectoryOnly)) {
              queue.Enqueue(subdir);
            }

            result.Add(dir);
          }

          return result;
        }

        default:
          throw ExceptionUtils.CreateArgumentMustBeValidEnumValue("searchOption", searchOption);
      }
    }

    public override string ToString()
    {
      return directory.ToString();
    }

    private DirectoryInfo directory;
    private DirectoryInfo tmpDirectory;
    private DirectoryInfo newDirectory;
    private DirectoryInfo curDirectory;
  }
}
