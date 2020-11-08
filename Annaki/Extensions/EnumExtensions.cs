using System;
using System.Collections.Generic;
using System.Text;
using SplatNet2.Net.Api.Data;

namespace Annaki.Extensions
{
    public static class EnumExtensions
    {
        public static string ToModeName(this GameMode gameMode)
        {
            return gameMode switch
            {
                GameMode.TurfWar => "Turf War",
                GameMode.SplatZones => "Splat Zones",
                GameMode.TowerControl => "Tower Control",
                GameMode.Rainmaker => "Rainmaker",
                GameMode.ClamBlitz => "Clam Blitz",
                _ => throw new ArgumentOutOfRangeException(nameof(gameMode), gameMode, null)
            };
        }
    }
}
