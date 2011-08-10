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

namespace Smdn.Windows.UserInterfaces.Interop {
  [CLSCompliant(false), Flags]
  public enum SLGP_FLAGS : uint {
    SHORTPATH   = 1,
    UNCPRIORITY = 2,
    RAWPATH     = 4,
  }

  /// <summary>Flags determining how the links with missing targets are resolved.</summary>
  [CLSCompliant(false), Flags]
  public enum SLR_FLAGS : uint {
    /// <summary>
    /// Do not display a dialog box if the link cannot be resolved. 
    /// When SLR_NO_UI is set, a time-out value that specifies the 
    /// maximum amount of time to be spent resolving the link can 
    /// be specified in milliseconds. The function returns if the 
    /// link cannot be resolved within the time-out duration. 
    /// If the timeout is not set, the time-out duration will be 
    /// set to the default value of 3,000 milliseconds (3 seconds). 
    /// </summary>
    NO_UI       = 1,
    /// <summary>
    /// Allow any match during resolution.  Has no effect
    /// on ME/2000 or above, use the other flags instead.
    /// </summary>
    ANY_MATCH   = 2,
    /// <summary>
    /// If the link object has changed, update its path and list 
    /// of identifiers. If SLR_UPDATE is set, you do not need to 
    /// call IPersistFile::IsDirty to determine whether or not 
    /// the link object has changed. 
    /// </summary>
    UPDATE      = 4,
    /// <summary>Do not update the link information.</summary>
    NOUPDATE    = 8,
    /// <summary>Do not execute the search heuristics.</summary>
    NOSEARCH    = 16,
    /// <summary>Do not use distributed link tracking.</summary>
    NOTRACK     = 32,
    /// <summary>
    /// Disable distributed link tracking. By default, 
    /// distributed link tracking tracks removable media 
    /// across multiple devices based on the volume name. 
    /// It also uses the UNC path to track remote file 
    /// systems whose drive letter has changed. Setting 
    /// SLR_NOLINKINFO disables both types of tracking.
    /// </summary>
    NOLINKINFO  = 64,
    /// <summary>Call the Microsoft Windows Installer.</summary>
    INVOKE_MSI  = 128,

    /// <summary>
    /// Not documented in SDK.  Assume same as SLR_NO_UI but 
    /// intended for applications without a hWnd.
    /// </summary>
    UI_WITH_MSG_PUMP = 0x101,
  }

  [CLSCompliant(false), Flags]
  public enum SHCONTF : uint {
    CHECKING_FOR_CHILDREN = 0x00010,
    FOLDERS               = 0x00020,
    NONFOLDERS            = 0x00040,
    INCLUDEHIDDEN         = 0x00080,
    INIT_ON_FIRST_NEXT    = 0x00100,
    NETPRINTERSRCH        = 0x00200,
    SHAREABLE             = 0x00400,
    STORAGE               = 0x00800,
    NAVIGATION_ENUM       = 0x01000,
    FASTITEMS             = 0x02000,
    FLATLIST              = 0x04000,
    ENABLE_ASYNC          = 0x08000,
    INCLUDESUPERHIDDEN    = 0x10000,
  }

  [CLSCompliant(false), Flags]
  public enum SFGAOF : uint {
    SFGAO_CANCOPY         = DROPEFFECT.COPY,
    SFGAO_CANMOVE         = DROPEFFECT.MOVE,
    SFGAO_CANLINK         = DROPEFFECT.LINK,
    SFGAO_CANRENAME       = 0x00000010,
    SFGAO_CANDELETE       = 0x00000020,
    SFGAO_HASPROPSHEET    = 0x00000040,
    SFGAO_DROPTARGET      = 0x00000100,
    SFGAO_CAPABILITYMASK  = 0x00000177,
    SFGAO_ISSLOW          = 0x00004000,
    SFGAO_GHOSTED         = 0x00008000,
    SFGAO_LINK            = 0x00010000,
    SFGAO_SHARE           = 0x00020000,
    SFGAO_READONLY        = 0x00040000,
    SFGAO_HIDDEN          = 0x00080000,
    SFGAO_DISPLAYATTRMASK =
      SFGAOF.SFGAO_ISSLOW |
      SFGAOF.SFGAO_GHOSTED |
      SFGAOF.SFGAO_LINK |
      SFGAOF.SFGAO_SHARE |
      SFGAOF.SFGAO_READONLY |
      SFGAOF.SFGAO_HIDDEN,
    SFGAO_FILESYSANCESTOR = 0x10000000,
    SFGAO_FOLDER          = 0x20000000,
    SFGAO_FILESYSTEM      = 0x40000000,
    SFGAO_HASSUBFOLDER    = 0x80000000,
    SFGAO_CONTENTSMASK    = 0x80000000,
    SFGAO_VALIDATE        = 0x01000000,
    SFGAO_REMOVABLE       = 0x02000000,
    SFGAO_COMPRESSED      = 0x04000000,
  }

  [CLSCompliant(false), Flags]
  public enum SHGDNF : uint {
    SHGDN_NORMAL              = 0x00000000,
    SHGDN_INFOLDER            = 0x00000001,
    SHGDN_FOREDITING          = 0x00001000,
    SHGDN_FORADDRESSBAR       = 0x00004000,
    SHGDN_FORPARSING          = 0x00008000,
  }

  [CLSCompliant(false), Flags]
  public enum GIL : uint {
    OPENICON        = 0x00000001,
    FORSHELL        = 0x00000002,
    ASYNC           = 0x00000020,
    DEFAULTICON     = 0x00000040,
    FORSHORTCUT     = 0x00000080,
    CHECKSHIELD     = 0x00000200,

    SIMULATEDOC     = 0x00000001,
    PERINSTANCE     = 0x00000002,
    PERCLASS        = 0x00000004,
    NOTFILENAME     = 0x00000008,
    DONTCACHE       = 0x00000010,
    SHIELD          = 0x00000200,
    FORCENOSHIELD   = 0x00000400,
  }
}
