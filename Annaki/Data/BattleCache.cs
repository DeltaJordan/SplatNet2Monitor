using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Annaki.Data
{
    public class BattleCache
    {
        public string[] ClamsFormattedArray { get; set; }
        public string[] RainFormattedArray { get; set; }
        public string[] ZonesFormattedArray { get; set; }
        public string[] TowerFormattedArray { get; set; }

        public int[] ReadBattleNumbers { get; set; }
    }
}
