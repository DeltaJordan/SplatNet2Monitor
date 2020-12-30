using System.Security.Claims;
using Annaki.Web.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Annaki.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDefaultIdentity<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<IdentityContext>();

            services.AddDbContext<IdentityContext>(options =>
                options.UseMySql(this.Configuration.GetConnectionString("MySqlConnection"),
                    ServerVersion.AutoDetect(this.Configuration.GetConnectionString("MySqlConnection"))));

            services.AddAuthentication(options =>
                {
                    /*options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;*/
                })
                .AddCookie()
                .AddDiscord(options =>
                {
                    options.ClientId = this.Configuration["Auth:Discord:Id"];
                    options.ClientSecret = this.Configuration["Auth:Discord:Secret"];

                    options.SaveTokens = true;
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Friend", policy => policy.RequireRole("Friend"));
            });

            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizePage("/Annaki", "Friend");
                options.Conventions.AuthorizePage("/Refresh", "Friend");
                options.Conventions.AuthorizeFolder("/Account/Manage");
                options.Conventions.AuthorizePage("/Account/Logout");
                options.Conventions.AuthorizePage("/MyClaims");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });

            app.Use(async (context, next) =>
            {
                await next();
                if (context.Response.StatusCode == 404)
                {
                    {
                        context.Request.Path = "/404";
                        await next();
                    }
                }
            });
        }
    }
}
