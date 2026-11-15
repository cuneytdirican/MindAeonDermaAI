using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MindAeonDermaAI.Infrastructure.Identity;

namespace MindAeonDermaAI.Web.Controllers;

/// <summary>Admin-only user management: list, create, lock/unlock and role assignment.</summary>
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly RoleManager<IdentityRole> _roles;

    public UsersController(UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles)
    {
        _users = users;
        _roles = roles;
    }

    public async Task<IActionResult> Index()
    {
        var list = _users.Users.OrderBy(u => u.Email).ToList();
        var rows = new List<UserRow>(list.Count);
        foreach (var u in list)
        {
            var roles = await _users.GetRolesAsync(u);
            rows.Add(new UserRow
            {
                Id = u.Id,
                Email = u.Email ?? "",
                FullName = u.FullName,
                Roles = roles.ToArray(),
                // A user is "locked" when lockout is enabled and the end date is in the future.
                LockedOut = u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow,
                CreatedAtUtc = u.CreatedAtUtc
            });
        }
        return View(rows);
    }

    [HttpGet]
    public IActionResult Create() =>
        View(new CreateUserViewModel { Role = AppRoles.Clinician });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(vm.Email))
            ModelState.AddModelError(nameof(vm.Email), "Email is required.");
        if (string.IsNullOrWhiteSpace(vm.Password))
            ModelState.AddModelError(nameof(vm.Password), "Password is required.");
        if (!AppRoles.All.Contains(vm.Role))
            ModelState.AddModelError(nameof(vm.Role), "Select a valid role.");

        if (!ModelState.IsValid) return View(vm);

        var email = vm.Email!.Trim();
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = string.IsNullOrWhiteSpace(vm.FullName) ? null : vm.FullName!.Trim(),
            EmailConfirmed = true
        };

        var result = await _users.CreateAsync(user, vm.Password!);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
            return View(vm);
        }

        await EnsureRoleAsync(vm.Role);
        await _users.AddToRoleAsync(user, vm.Role);

        TempData["Ok"] = $"Created user “{email}” as {vm.Role}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock(string id)
    {
        var user = await _users.FindByIdAsync(id);
        if (user is null) return NotFound();

        var locked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;
        if (locked)
        {
            await _users.SetLockoutEndDateAsync(user, null);
            TempData["Ok"] = $"Enabled “{user.Email}”.";
        }
        else
        {
            // Lockout requires the flag to be enabled for the user.
            await _users.SetLockoutEnabledAsync(user, true);
            await _users.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            TempData["Ok"] = $"Disabled “{user.Email}”.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRole(string id, string role)
    {
        if (!AppRoles.All.Contains(role))
        {
            TempData["Err"] = "Invalid role.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _users.FindByIdAsync(id);
        if (user is null) return NotFound();

        await EnsureRoleAsync(role);
        var current = await _users.GetRolesAsync(user);
        if (current.Count > 0) await _users.RemoveFromRolesAsync(user, current);
        await _users.AddToRoleAsync(user, role);

        TempData["Ok"] = $"“{user.Email}” is now {role}.";
        return RedirectToAction(nameof(Index));
    }

    private async Task EnsureRoleAsync(string role)
    {
        if (!await _roles.RoleExistsAsync(role))
            await _roles.CreateAsync(new IdentityRole(role));
    }
}

/// <summary>Row projection for the user list.</summary>
public class UserRow
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public string? FullName { get; set; }
    public string[] Roles { get; set; } = Array.Empty<string>();
    public bool LockedOut { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

/// <summary>Create-user form.</summary>
public class CreateUserViewModel
{
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? Password { get; set; }
    public string Role { get; set; } = AppRoles.Clinician;
}
