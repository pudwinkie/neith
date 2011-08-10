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
using System.Runtime.InteropServices;

namespace Smdn.Windows.UserInterfaces.Interop {
  [CLSCompliant(false)]
  public static partial class Consts {
    public const uint INPUT_MOUSE     = 0;
    public const uint INPUT_KEYBOARD  = 1;
    public const uint INPUT_HARDWARE  = 2;

    public const uint XBUTTON1 = 0x0001;
    public const uint XBUTTON2 = 0x0002;

    public static readonly IntPtr RT_CURSOR         = new IntPtr(1);
    public static readonly IntPtr RT_BITMAP         = new IntPtr(2);
    public static readonly IntPtr RT_ICON           = new IntPtr(3);
    public static readonly IntPtr RT_MENU           = new IntPtr(4);
    public static readonly IntPtr RT_DIALOG         = new IntPtr(5);
    public static readonly IntPtr RT_STRING         = new IntPtr(6);
    public static readonly IntPtr RT_FONTDIR        = new IntPtr(7);
    public static readonly IntPtr RT_FONT           = new IntPtr(8);
    public static readonly IntPtr RT_ACCELERATOR    = new IntPtr(9);
    public static readonly IntPtr RT_RCDATA         = new IntPtr(10);
    public static readonly IntPtr RT_MESSAGETABLE   = new IntPtr(11);
    public static readonly IntPtr RT_GROUP_CURSOR   = new IntPtr(/*RT_CURSOR*/1 + 11);
    public static readonly IntPtr RT_GROUP_ICON     = new IntPtr(/*RT_ICON*/3 + 11);
    public static readonly IntPtr RT_VERSION        = new IntPtr(16);
    public static readonly IntPtr RT_DLGINCLUDE     = new IntPtr(17);
    public static readonly IntPtr RT_PLUGPLAY       = new IntPtr(19);
    public static readonly IntPtr RT_VXD            = new IntPtr(20);
    public static readonly IntPtr RT_ANICURSOR      = new IntPtr(21);
    public static readonly IntPtr RT_ANIICON        = new IntPtr(22);
    public static readonly IntPtr RT_HTML           = new IntPtr(23);
  }

  /*
   * http://www.pinvoke.net/default.aspx/user32.sendinput
   */
  [CLSCompliant(false)]
  public enum VK : ushort {
    //
    // Virtual Keys, Standard Set
    //
    LBUTTON = 0x01,
    RBUTTON = 0x02,
    CANCEL =  0x03,
    MBUTTON = 0x04,    // NOT contiguous with L & RBUTTON

    XBUTTON1 = 0x05,    // NOT contiguous with L & RBUTTON
    XBUTTON2 = 0x06,    // NOT contiguous with L & RBUTTON

    // 0x07 : unassigned

    BACK =    0x08,
    TAB =     0x09,

    // 0x0A - 0x0B : reserved

    CLEAR =   0x0C,
    RETURN =  0x0D,

    SHIFT =   0x10,
    CONTROL = 0x11,
    MENU =    0x12,
    PAUSE =   0x13,
    CAPITAL = 0x14,

    KANA =    0x15,
    HANGEUL = 0x15,  // old name - should be here for compatibility
    HANGUL =  0x15,
    JUNJA =   0x17,
    FINAL =   0x18,
    HANJA =   0x19,
    KANJI =   0x19,

    ESCAPE =  0x1B,

    CONVERT =    0x1C,
    NONCONVERT = 0x1D,
    ACCEPT =     0x1E,
    MODECHANGE = 0x1F,

    SPACE =   0x20,
    PRIOR =   0x21,
    NEXT =    0x22,
    END =     0x23,
    HOME =    0x24,
    LEFT =    0x25,
    UP =      0x26,
    RIGHT =   0x27,
    DOWN =    0x28,
    SELECT =  0x29,
    PRINT =   0x2A,
    EXECUTE = 0x2B,
    SNAPSHOT = 0x2C,
    INSERT =  0x2D,
    DELETE =  0x2E,
    HELP =    0x2F,

    //
    // VK_0 - VK_9 are the same as ASCII '0' - '9' (0x30 - 0x39)
    // 0x40 : unassigned
    // VK_A - VK_Z are the same as ASCII 'A' - 'Z' (0x41 - 0x5A)
    //

    LWIN =    0x5B,
    RWIN =    0x5C,
    APPS =    0x5D,

    //
    // 0x5E : reserved
    //

    SLEEP =   0x5F,

    NUMPAD0 = 0x60,
    NUMPAD1 = 0x61,
    NUMPAD2 = 0x62,
    NUMPAD3 = 0x63,
    NUMPAD4 = 0x64,
    NUMPAD5 = 0x65,
    NUMPAD6 = 0x66,
    NUMPAD7 = 0x67,
    NUMPAD8 = 0x68,
    NUMPAD9 = 0x69,
    MULTIPLY = 0x6A,
    ADD = 0x6B,
    SEPARATOR = 0x6C,
    SUBTRACT  = 0x6D,
    DECIMAL = 0x6E,
    DIVIDE =  0x6F,
    F1 =      0x70,
    F2 =      0x71,
    F3 =      0x72,
    F4 =      0x73,
    F5 =      0x74,
    F6 =      0x75,
    F7 =      0x76,
    F8 =      0x77,
    F9 =      0x78,
    F10 =     0x79,
    F11 =     0x7A,
    F12 =     0x7B,
    F13 =     0x7C,
    F14 =     0x7D,
    F15 =     0x7E,
    F16 =     0x7F,
    F17 =     0x80,
    F18 =     0x81,
    F19 =     0x82,
    F20 =     0x83,
    F21 =     0x84,
    F22 =     0x85,
    F23 =     0x86,
    F24 =     0x87,

    //
    // 0x88 - 0x8F : unassigned
    //

    NUMLOCK = 0x90,
    SCROLL =  0x91,

    //
    // VK_L* & VK_R* - left and right Alt, Ctrl and Shift virtual keys.
    // Used only as parameters to GetAsyncKeyState() and GetKeyState().
    // No other API or message will distinguish left and right keys in this way.
    //
    LSHIFT   = 0xA0,
    RSHIFT   = 0xA1,
    LCONTROL = 0xA2,
    RCONTROL = 0xA3,
    LMENU    = 0xA4,
    RMENU    = 0xA5,

    BROWSER_BACK    = 0xA6,
    BROWSER_FORWARD = 0xA7,
    BROWSER_REFRESH = 0xA8,
    BROWSER_STOP    = 0xA9,
    BROWSER_SEARCH    = 0xAA,
    BROWSER_FAVORITES = 0xAB,
    BROWSER_HOME    = 0xAC,

    VOLUME_MUTE =  0xAD,
    VOLUME_DOWN =  0xAE,
    VOLUME_UP =    0xAF,
    MEDIA_NEXT_TRACK = 0xB0,
    MEDIA_PREV_TRACK = 0xB1,
    MEDIA_STOP =   0xB2,
    MEDIA_PLAY_PAUSE = 0xB3,
    LAUNCH_MAIL =  0xB4,
    LAUNCH_MEDIA_SELECT = 0xB5,
    LAUNCH_APP1 =  0xB6,
    LAUNCH_APP2 =  0xB7,

    //
    // 0xB8 - 0xB9 : reserved
    //

    OEM_1      = 0xBA,   // ';:' for US
    OEM_PLUS   = 0xBB,   // '+' any country
    OEM_COMMA  = 0xBC,   // ',' any country
    OEM_MINUS  = 0xBD,   // '-' any country
    OEM_PERIOD = 0xBE,   // '.' any country
    OEM_2      = 0xBF,   // '/?' for US
    OEM_3      = 0xC0,   // '`~' for US

    //
    // 0xC1 - 0xD7 : reserved
    //

    //
    // 0xD8 - 0xDA : unassigned
    //

    OEM_4 =   0xDB,  //  '[{' for US
    OEM_5 =   0xDC,  //  '\|' for US
    OEM_6 =   0xDD,  //  ']}' for US
    OEM_7 =   0xDE,  //  ''"' for US
    OEM_8 =   0xDF

    //
    // 0xE0 : reserved
    //
  }

  [CLSCompliant(false), Flags]
  public enum MOUSEEVENTF : uint {
    None          = 0,
    MOVE          = 0x00000001,
    LEFTDOWN      = 0x00000002,
    LEFTUP        = 0x00000004,
    RIGHTDOWN     = 0x00000008,
    RIGHTUP       = 0x00000010,
    MIDDLEDOWN    = 0x00000020,
    MIDDLEUP      = 0x00000040,
    ABSOLUTE      = 0x00008000,
    WHEEL         = 0x00000800,
    XDOWN         = 0x00000080,
    XUP           = 0x00000100,
  }

  [CLSCompliant(false), Flags]
  public enum KEYEVENTF : uint {
    None          = 0,
    EXTENDEDKEY   = 0x00000001,
    KEYUP         = 0x00000002,
    UNICODE       = 0x00000004,
    SCANCODE      = 0x00000008,
  }

  [CLSCompliant(false), StructLayout(LayoutKind.Sequential, Pack = 4)]
  public struct MOUSEINPUT {
    public int dx;
    public int dy;
    public uint mouseData;
    public MOUSEEVENTF dwFlags;
    public uint time;
    public IntPtr dwExtraInfo;
  }

  [CLSCompliant(false), StructLayout(LayoutKind.Sequential, Pack = 4)]
  public struct KEYBDINPUT {
    public VK wVk;
    public ushort wScan;
    public KEYEVENTF dwFlags;
    public uint time; //
    public IntPtr dwExtraInfo;
  }

  [CLSCompliant(false), StructLayout(LayoutKind.Sequential, Pack = 4)]
  public struct HARDWAREINPUT {
    public uint uMsg;
    public ushort wParamL;
    public ushort wParamH;
  }

  [CLSCompliant(false), StructLayout(LayoutKind.Explicit, Pack = 4)]
  public struct INPUT {
    [FieldOffset(0)] public uint type;
    [FieldOffset(4)] public MOUSEINPUT mi;
    [FieldOffset(4)] public KEYBDINPUT ki;
    [FieldOffset(4)] public HARDWAREINPUT hi;

    public static INPUT CreateMouseInput()
    {
      return CreateInput(Consts.INPUT_MOUSE);
    }

    public static INPUT CreateKeyboardInput()
    {
      return CreateInput(Consts.INPUT_KEYBOARD);
    }

    public static INPUT CreateHardwareInput()
    {
      return CreateInput(Consts.INPUT_HARDWARE);
    }

    private static INPUT CreateInput(uint type)
    {
      var input = new INPUT();

      input.type = type;

      return input;
    }

    public static readonly int Size = Marshal.SizeOf(typeof(INPUT));
  }

  [Flags]
  public enum MOD : int {
    ALT       = 1,
    CONTROL   = 2,
    SHIFT     = 4,
    WIN       = 8,
    IGNORE_ALL_MODIFIER = 1024,
    ON_KEYUP  = 2048,
    RIGHT     = 16384,
    LEFT      = 32768,
  }

  public enum SW : int {
    HIDE            = 0,
    NORMAL          = 1,
    SHOWNORMAL      = 1,
    SHOWMINIMIZED   = 2,
    MAXIMIZE        = 3,
    SHOWMAXIMIZED   = 3,
    SHOWNOACTIVATE  = 4,
    SHOW            = 5,
    MINIMIZE        = 6,
    SHOWMINNOACTIVE = 7,
    SHOWNA          = 8,
    RESTORE         = 9,
    SHOWDEFAULT     = 10,
    FORCEMINIMIZE   = 11,
  }

  public enum SPI : int {
    SETDESKWALLPAPER        = 0x0014,
  }

  [Flags]
  public enum SPIF : int {
    UPDATEINIFILE           = 0x0001,
    SENDWININICHANGE        = 0x0002,
  }

  [Flags]
  public enum WM : int {
    PAINT                   = 0xf,
    CLOSE                   = 0x10,
    ENDSESSION              = 0x16,
    GETICON                 = 0x7f,
    SETICON                 = 0x80,
    KEYDOWN                 = 0x100,
    COMMAND                 = 0x111,
    HSCROLL                 = 0x114,
    VSCROLL                 = 0x115,
    SYSCOMMAND              = 0x112,
    MOUSEWHEEL              = 0x20a,
    HOTKEY                  = 0x312,
    DWMCOMPOSITIONCHANGED       = 0x031e,
    DWMNCRENDERINGCHANGED       = 0x031f,
    DWMCOLORIZATIONCOLORCHANGED = 0x0320,
    DWMWINDOWMAXIMIZEDCHANGE    = 0x0321,
  }
}
