using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TMSWeb.Controllers;
[Authorize]
public class HomeController : Controller
{
    private string LoggedInUser => User.Identity.Name;
    public IActionResult Index()
    {
        this.GetLoggedGroup();
        ViewBag.Group = this.GetLoggedGroup();
        return View();
    }

    private string GetLoggedGroup()
    {
        var data = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
        return data;
    }
}
