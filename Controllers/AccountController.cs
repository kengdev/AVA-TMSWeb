using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TMSWeb.Controllers;

[Route("account")]
public sealed class AccountController : Controller
{
    [Authorize]
    [HttpGet("~/connect/logout")]
    public IActionResult Logout()
    {
        return SignOut(
            new AuthenticationProperties
            {
                RedirectUri = "/"
            },

            // ลบ Cookie ของ TMSWeb
            CookieAuthenticationDefaults.AuthenticationScheme,

            // ส่งไป Logout ที่ SSO Server
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    [AllowAnonymous]
    [HttpGet("access-denied")]
    public IActionResult AccessDenied()
    {
        return View();
    }
}