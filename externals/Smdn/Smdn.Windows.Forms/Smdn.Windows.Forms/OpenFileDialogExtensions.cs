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
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Smdn.Imaging;
using Smdn.IO;

namespace Smdn.Windows.Forms {
  public static class OpenFileDialogExtensions {
    public static OpenFileDialog CreateOpenImageDialog()
    {
      return CreateOpenImageDialog(null);
    }

    public static OpenFileDialog CreateOpenImageDialog(string initialFile)
    {
      var dialog = new OpenFileDialog();

      dialog.Filter = FileDialogFilter.CreateFilterString(new[] {
        new[] {"画像ファイル", ImageCodecs.GetDecoderExtensionPattern()},
        new[] {"すべてのファイル", "*.*"},
      });
      dialog.FilterIndex = 1;
      dialog.CheckFileExists = true;

      if (File.Exists(initialFile)) {
        dialog.FileName = initialFile;
        dialog.InitialDirectory = Path.GetDirectoryName(initialFile);
      }
      else {
        dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
      }

      return dialog;
    }

    public static Bitmap OpenImage(this OpenFileDialog dialog)
    {
      if (string.IsNullOrEmpty(dialog.FileName))
        throw new InvalidOperationException("FileName is null or empty");

      return BitmapExtensions.LoadFrom(dialog.FileName);
    }

    public static Bitmap[] OpenImages(this OpenFileDialog dialog)
    {
      return Array.ConvertAll(dialog.FileNames, delegate(string filename) {
        return BitmapExtensions.LoadFrom(filename);
      });
    }
  }
}
