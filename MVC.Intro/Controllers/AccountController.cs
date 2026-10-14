using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC.Intro.Models;

namespace MVC.Intro.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;

        // Emails, which grant admin role — extendable via config
        private static readonly HashSet<string> AdminEmails = new(StringComparer.OrdinalIgnoreCase)
        {
            "admin@tickets.com",
            "administrator@tickets.com"
        };

        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Derives a display name from the input:
        /// 1. If the user typed a name → use it as-is
        /// 2. Otherwise take the part before '@' in the email and title-case it
        ///    e.g. "ivan.petrov@mail.com" → "Ivan Petrov"
        /// </summary>
        private static string ResolveDisplayName(string email, string? typedName)
        {
            if (!string.IsNullOrWhiteSpace(typedName))
                return typedName.Trim();

            var local = email.Split('@')[0];                         // "ivan.petrov"
            var parts = local.Split(new[] { '.', '_', '-' },        // split on separators
                StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", parts.Select(p =>
                char.ToUpper(p[0]) + p[1..]));                      // "Ivan Petrov"
        }

        /// <summary>
        /// Derives the user's role:
        /// - "Admin" for known admin emails
        /// - "User" for everyone else
        /// </summary>
        private static string ResolveRole(string email) =>
            AdminEmails.Contains(email) ? "Admin" : "User";

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var demoEmail = _configuration["Demo:Email"] ?? "tickets@fan.com";
            var demoPassword = _configuration["Demo:Password"] ?? "Tickets123!";

            if (!string.Equals(model.Email, demoEmail, StringComparison.OrdinalIgnoreCase)
                || model.Password != demoPassword)
            {
                ModelState.AddModelError(string.Empty, "Невалиден имейл или парола.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name,  ResolveDisplayName(demoEmail, model.DisplayName)),
                new(ClaimTypes.Email, demoEmail),
                new(ClaimTypes.Role,  ResolveRole(demoEmail))
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var properties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(4)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                properties);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
