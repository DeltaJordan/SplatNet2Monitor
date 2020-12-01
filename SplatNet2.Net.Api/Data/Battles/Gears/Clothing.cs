using System;
using System.Collections.Generic;
using System.Text;

namespace SplatNet2.Net.Api.Data.Battles.Gears
{
    public class Clothing
    {
        public int ClothingId { get; set; }

        public string BrandName { get; set; }

        public string Name { get; set; }

        public string ImageUrl { get; set; }

        public GearEnums.Ability MainAbility { get; set; }

        public GearEnums.Ability[] SecondaryAbilities { get; set; }
    }
}
