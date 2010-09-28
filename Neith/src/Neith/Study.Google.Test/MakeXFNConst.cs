using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Study.Google.Test
{
    internal class MakeXFNConst
    {
        internal static readonly Uri XfnURI = new Uri("http://xfn.vbel.net/");

        internal const string JsonDoc =
@"{                 $:{ja:""NeithXFN"", },
  types:{         $:{ja:""分類"", },
    space:{       $:{ja:""時空"", },
      game:{      $:{ja:""ゲーム""        ,level:  0}},
      world:{     $:{ja:""世界""          ,level:  1}},
      era:{       $:{ja:""時代""          ,level:  2}},
      country:{   $:{ja:""国""            ,level:  3}},
      region:{    $:{ja:""リージョン""    ,level:  4}},
      area:{      $:{ja:""エリア""        ,level:  5}},
      map:{       $:{ja:""マップ""        ,level:  6}},
      floor:{     $:{ja:""階層""          ,level:  7}},
      district:{  $:{ja:""地区""          ,level:  8}},
      landmark:{  $:{ja:""ランドマーク""  ,level:  9}},
      position:{  $:{ja:""座標""          ,level: 10}},
    },
    organism:{    $:{ja:""生物"", },
      pc:{        $:{ja:""プレイヤー"", }},
      npc:{       $:{ja:""NPC"", }},
      enemy:{     $:{ja:""敵"", }},
      race:{      $:{ja:""種族"", }},
    },
    production:{  $:{ja:""生産"", },
      pc:{        $:{ja:""プレイヤー"", }},
      npc:{       $:{ja:""NPC"", }},
      enemy:{     $:{ja:""敵"", }},
      race:{      $:{ja:""種族"", }},
    },
    adventure:{   $:{ja:""冒険"", },
      legend:{    $:{ja:""伝説""          ,level: 0 }},
      story:{     $:{ja:""物語""          ,level: 1 }},
      quest:{     $:{ja:""クエスト""      ,level: 2 }},
      misson:{    $:{ja:""作戦""          ,level: 2 }},
      mark:{      $:{ja:""小目標""        ,level: 9 }},
    },
    life:{        $:{ja:""生活"", },
      work:{      $:{ja:""仕事"" }},
      toy:{       $:{ja:""遊び"" }},
    },
  },
}
";

    }
}
