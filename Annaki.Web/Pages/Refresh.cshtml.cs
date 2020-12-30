using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ClaimsSample.Areas.Identity.Pages.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SplatNet2.Net.Monitor.Workers;

namespace Annaki.Web.Pages
{
    [Authorize]
    public class RefreshModel : PageModel
    {
        private BattleMonitor battleMonitor;
        private ulong userId;

        private UserManager<IdentityUser> userManager;
        private SignInManager<IdentityUser> signInManager;
        private ILogger<ExternalLoginModel> logger;

        public RefreshModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<ExternalLoginModel> logger)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Authorization Information")]
            [DataType(DataType.Password)]
            public string AuthInfo { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            string idString = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            this.userId = ulong.Parse(idString);
            this.battleMonitor = Annaki.BattleMonitors[this.userId];

            if (!this.battleMonitor.NeedsAuth)
            {
                return this.Redirect("/Annaki");
            }

            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (this.battleMonitor == null)
            {
                this.battleMonitor =
                    Annaki.BattleMonitors[ulong.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier))];
            }

            if (this.ModelState.IsValid)
            {
                if (await this.battleMonitor.RefreshCookie(this.Input.AuthInfo))
                {
                    return this.LocalRedirect(returnUrl);
                }

                this.ModelState.AddModelError(string.Empty, "Unable to refresh cookie, please try again.");
            }

            return this.Page();
        }

        public string GetAuthUrl()
        {
            return this.battleMonitor.AuthUrl;
        }
    }
}
