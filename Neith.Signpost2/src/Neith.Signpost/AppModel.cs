using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Signpost
{
    /// <summary>
    /// アプリケーション内でシングルトン参照されるモデル・ビューモデル
    /// </summary>
    public static class AppModel
    {
        public static readonly EorzeaClockViewModel Clock = new EorzeaClockViewModel();

    }
}
