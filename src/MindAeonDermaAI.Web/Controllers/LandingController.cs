using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MindAeonDermaAI.Web.Controllers;

/// <summary>
/// Public marketing / entry site shown before authentication. Visitors land here first and proceed to
/// the platform via Sign in.
/// </summary>
[AllowAnonymous]
public class LandingController : Controller
{
    public IActionResult Index() => View();
}
