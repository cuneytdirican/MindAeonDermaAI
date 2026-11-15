using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace MindAeonDermaAI.Web.Controllers;

/// <summary>Switches the UI language by setting the culture cookie.</summary>
[AllowAnonymous]
public class CultureController : Controller
{
    [HttpGet]
    public IActionResult Set(string culture, string? returnUrl = null)
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });
        return LocalRedirect(string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl) ? "/" : returnUrl);
    }
}
