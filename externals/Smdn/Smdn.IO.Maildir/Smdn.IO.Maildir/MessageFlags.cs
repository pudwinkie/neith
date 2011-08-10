using System;

namespace Smdn.IO.Maildir {
  [Flags()]
  public enum MessageFlags : int {
    None      = 0,
    Passed    = 1 << 0,
    Replied   = 1 << 1,
    Seen      = 1 << 2,
    Trashed   = 1 << 3,
    Draft     = 1 << 4,
    Flagged   = 1 << 5,
  }
}
