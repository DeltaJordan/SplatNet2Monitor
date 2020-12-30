using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Annaki.Data.Processors;
using Annaki.Events.Workers;
using ClaimsSample.Areas.Identity.Pages.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SplatNet2.Net.Api.Data;
using SplatNet2.Net.Api.Data.Battles;
using SplatNet2.Net.Monitor.Workers;

namespace Annaki.Web.Pages
{
    public class AnnakiModel : PageModel
    {
        private BattleMonitor battleMonitor;
        private ulong userId;

        private SignInManager<IdentityUser> signInManager;
        private UserManager<IdentityUser> userManager;
        private ILogger<ExternalLoginModel> logger;

        public AnnakiModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<ExternalLoginModel> logger)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            string idString = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            this.userId = ulong.Parse(idString);

            if (!Annaki.BattleMonitors.ContainsKey(this.userId))
            {
                await Annaki.InitializeBattleMonitor(this.userId);
            }

            this.battleMonitor = Annaki.BattleMonitors[this.userId];

            if (this.battleMonitor.NeedsAuth)
            {
                return this.LocalRedirect("/Refresh");
            }

            return this.Page();
        }

        public string FormatBattleData(GameMode gameMode)
        {
            Regex regex = new Regex(@"Date\(([0-9]+)\)");

            string[] modeArray = BattleCacheProcessor.GetModeArray(gameMode, this.userId);

            return string.Join(',', modeArray.OrderByDescending(x => ulong.Parse(regex.Match(x).Groups[1].Value)));
        }
    }
}
