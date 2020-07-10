using System;
using System.Collections.Generic;
using System.Text;
using SplatNet2.Net.Api.Data.Battles.Gears;

namespace SplatNet2.Net.Api.Data.Battles
{
    public class Gear
    {
        public Headgear Headgear { get; set; }
        public Clothing Clothing { get; set; }
        public Shoes Shoes { get; set; }
    }
}
