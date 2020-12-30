using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace ClaimsSample.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private static readonly ulong[] AuthorizedAnnakiUsers = {324602032147267584, 200066011616116737, 228019100008316948 };

        private readonly SignInManager<IdentityUser> signInManager;
        private readonly UserManager<IdentityUser> userManager;
        private readonly ILogger<ExternalLoginModel> logger;
        private readonly RoleManager<IdentityRole> roleManager;

        public ExternalLoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<ExternalLoginModel> logger,
            RoleManager<IdentityRole> roleManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.logger = logger;
            this.roleManager = roleManager;
        }

        public string LoginProvider { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public IActionResult OnGetAsync()
        {
            return this.RedirectToPage("./Login");
        }

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            string redirectUrl = this.Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            AuthenticationProperties properties = this.signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, 
            string remoteError = null)
        {
            returnUrl ??= this.Url.Content("~/");

            if (remoteError != null)
            {
                this.ErrorMessage = $"Error from external provider: {remoteError}";
                return this.RedirectToPage("~/", new {ReturnUrl = returnUrl });
            }

            ExternalLoginInfo info = await this.signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                this.ErrorMessage = "Error loading external login information.";
                return this.RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already 
            // has a login.
            SignInResult result = await this.signInManager.ExternalLoginSignInAsync(info.LoginProvider, 
                info.ProviderKey, true, true);

            bool friendRoleExists = await this.roleManager.RoleExistsAsync("Friend");

            if (!friendRoleExists)
            {
                IdentityRole role = new IdentityRole("Friend");
                await this.roleManager.CreateAsync(role);
            }

            if (result.Succeeded)
            {
                // Store the access token and resign in so the token is included in
                // in the cookie
                IdentityUser user = await this.userManager.FindByLoginAsync(info.LoginProvider, 
                    info.ProviderKey);

                AuthenticationProperties props = new AuthenticationProperties();
                props.StoreTokens(info.AuthenticationTokens);

                await this.signInManager.SignInAsync(user, props, info.LoginProvider);

                this.logger.LogInformation("{Name} logged in with {LoginProvider} provider.", 
                    info.Principal.Identity.Name, info.LoginProvider);

                if (AuthorizedAnnakiUsers.Contains(ulong.Parse(info.Principal.FindFirstValue(ClaimTypes.NameIdentifier))) &&
                    !await this.userManager.IsInRoleAsync(user, "Friend"))
                {
                    await this.userManager.AddToRoleAsync(user, "Friend");
                }

                return this.LocalRedirect(returnUrl);
            }

            if (result.IsLockedOut)
            {
                return this.RedirectToPage("./Lockout");
            }
            else
            {
                IdentityUser user = new IdentityUser
                {
                    Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                    UserName = info.Principal.FindFirstValue(ClaimTypes.Name),
                    Id = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier)
                };

                IdentityResult identityResult = await this.userManager.CreateAsync(user);

                if (identityResult.Succeeded)
                {
                    identityResult = await this.userManager.AddLoginAsync(user, info);

                    if (identityResult.Succeeded)
                    {
                        // If they exist, add claims to the user.

                        if (info.Principal.HasClaim(c => c.Type == DiscordAuthenticationConstants.Claims.AvatarUrl))
                        {
                            await this.userManager.AddClaimAsync(user,
                                info.Principal.FindFirst(DiscordAuthenticationConstants.Claims.AvatarUrl));
                        }

                        if (info.Principal.HasClaim(c => c.Type == DiscordAuthenticationConstants.Claims.Discriminator))
                        {
                            await this.userManager.AddClaimAsync(user,
                                info.Principal.FindFirst(DiscordAuthenticationConstants.Claims.Discriminator));
                        }

                        // Include the access token in the properties
                        AuthenticationProperties props = new AuthenticationProperties();
                        props.StoreTokens(info.AuthenticationTokens);
                        props.IsPersistent = true;

                        await this.signInManager.SignInAsync(user, props);

                        this.logger.LogInformation(
                            "User created an account using {Name} provider.",
                            info.LoginProvider);

                        if (AuthorizedAnnakiUsers.Contains(ulong.Parse(info.Principal.FindFirstValue(ClaimTypes.NameIdentifier))) &&
                            !await this.userManager.IsInRoleAsync(user, "Friend"))
                        {
                            await this.userManager.AddToRoleAsync(user, "Friend");
                        }

                        return this.LocalRedirect(returnUrl);
                    }
                }

                foreach (IdentityError error in identityResult.Errors)
                {
                    this.ModelState.AddModelError(string.Empty, error.Description);
                }

                this.LoginProvider = info.LoginProvider;
                this.ReturnUrl = returnUrl;

                return this.Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            return this.Page();
        }
    }
}
