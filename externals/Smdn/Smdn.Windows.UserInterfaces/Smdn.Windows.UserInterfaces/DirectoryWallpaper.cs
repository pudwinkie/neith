// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2009-2011 smdn
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif
using Smdn.Imaging;
using Smdn.Mathematics;

namespace Smdn.Windows.UserInterfaces {
  public class DirectoryWallpaper : FileWallpaper {
    public enum SelectionOrder {
      ByFileName,
      ByCreationTime,
      ByRandom,
    }

    public string Directory {
      get { return directory; }
      set
      {
        if (directory == value)
          return;

        directory = value;

        selectedFiles.Clear();
      }
    }

    public Random Random {
      get; set;
    }

    public SelectionOrder FileSelectionOrder {
      get { return fileSelectionOrder; }
      set
      {
        if (fileSelectionOrder == value)
          return;

        fileSelectionOrder = value;

        selectedFiles.Clear();
      }
    }

    public DirectoryWallpaper()
      : base()
    {
      this.FileSelectionOrder = SelectionOrder.ByFileName;
      this.Directory = null;
    }

    public DirectoryWallpaper(string directory, SelectionOrder fileSelectionOrder, Color backgroundColor, ImageFillStyle drawStyle)
      : this(directory, fileSelectionOrder, backgroundColor, backgroundColor, Radian.Zero, drawStyle)
    {
    }

    public DirectoryWallpaper(string directory, SelectionOrder fileSelectionOrder, Color backgroundColorNear, Color backgroundColorFar, Radian gradientDirection, ImageFillStyle drawStyle)
      : base(null, backgroundColorNear, backgroundColorFar, gradientDirection, drawStyle)
    {
      this.FileSelectionOrder = fileSelectionOrder;
      this.Directory = directory;
    }

    public void SelectNextFile()
    {
      if (!System.IO.Directory.Exists(directory))
        return;

      var extensions = ImageCodecs.GetDecoderExtensions();
      var files = Smdn.IO.DirectoryUtils.GetFiles(directory, delegate(string file) {
        foreach (var extension in extensions) {
          if (Smdn.IO.PathUtils.AreExtensionEqual(file, extension))
            return true;
        }

        return false;
      });
      var candidateFiles = new List<string>(files);

      foreach (var selectedFile in selectedFiles) {
        var selectedCandidateFile = candidateFiles.Find(delegate(string candidateFile) {
          return Smdn.IO.PathUtils.ArePathEqual(candidateFile, selectedFile);
        });

        if (selectedCandidateFile != null)
          candidateFiles.Remove(selectedCandidateFile);
      }

      if (candidateFiles.Count == 0) {
        if (files.Count() == 0) {
          File = null;
          return;
        }

        selectedFiles.Clear();
        candidateFiles = new List<string>(files);
      }

      switch (fileSelectionOrder) {
        case SelectionOrder.ByFileName:
          candidateFiles.Sort();
          File = candidateFiles[0];
          break;

        case SelectionOrder.ByCreationTime:
          candidateFiles.Sort(delegate(string x, string y) {
            return DateTime.Compare(System.IO.File.GetCreationTimeUtc(x), System.IO.File.GetCreationTimeUtc(y));
          });
          File = candidateFiles[0];
          break;

        case SelectionOrder.ByRandom:
          File = candidateFiles[(Random ?? new Random()).Next(0, candidateFiles.Count)];
          break;

        default:
          throw ExceptionUtils.CreateNotSupportedEnumValue(fileSelectionOrder);
      }

      selectedFiles.Add(File);
    }

    private string directory;
    private List<string> selectedFiles = new List<string>();
    private SelectionOrder fileSelectionOrder;
  }
}
