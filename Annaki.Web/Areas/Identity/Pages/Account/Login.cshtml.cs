using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Annaki.Web.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        public IActionResult OnGetAsync()
        {
            return this.Redirect("/404");
        }
    }
}
