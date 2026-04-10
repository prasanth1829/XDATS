using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ResumeApp.Data;
using ResumeApp.Models;
using ResumeApp.ViewModels;


namespace ResumeApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        private readonly ApplicationDbContext _context;


        public AccountController(SignInManager<Users> signInManager, UserManager<Users> userManager, ApplicationDbContext context)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            _context = context;

        }
        [AllowAnonymous]
        public IActionResult AccessDenied(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
                }

                // Get roles before checking IsApproved
                var roles = await userManager.GetRolesAsync(user);

                // Block login if not approved AND not an Admin
                if (!roles.Contains("Admin") && !user.IsApproved)
                {
                    ModelState.AddModelError("", "Your account is pending approval by Admin.");
                    return View(model);
                }
                if (user.IsDenied)
                {
                    ModelState.AddModelError("", "Your account request was denied by Admin.");
                    return View(model);
                }
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {

                    var userId = user.Id;
                    var openSession = _context.UserSessionLogs
                        .Where(x => x.UserId == userId && x.LogoutTime == null)
                        .OrderByDescending(x => x.LoginTime)
                        .FirstOrDefault();

                    if (openSession != null)
                    {
                        openSession.LogoutTime = DateTime.UtcNow;
                        openSession.SessionMinutes =
                            (int)(openSession.LogoutTime.Value - openSession.LoginTime).TotalMinutes;
                        openSession.IsAutoLogout = true;
                    }

                    // Create new login session
                    var newSession = new UserSessionLog
                    {
                        UserId = userId,
                        LoginTime = DateTime.UtcNow,
                        IsAutoLogout = false
                    };

                    _context.UserSessionLogs.Add(newSession);
                    await _context.SaveChangesAsync();

                

                    if (roles.Contains("Admin"))
                        return RedirectToAction("AdminDashboard", "Home");

                    if (roles.Contains("Reviewer"))
                        return RedirectToAction("ReviewerDashboard", "Home");

                    if (roles.Contains("Team Lead"))
                        return RedirectToAction("TeamLead", "Dashboard");

                    if (roles.Contains("Manager"))
                        return RedirectToAction("Manager", "Dashboard");

                    if (roles.Contains("Vendor"))
                        return RedirectToAction("Vendor", "Dashboard");

                    if (roles.Contains("Panel"))
                        return RedirectToAction("Panel", "Dashboard");


                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Invalid login attempt.");
            }

            return View(model);
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                Users users = new Users
                {
                    FullName = model.Name,
                    Email = model.Email,
                    UserName = model.Email,
                };
                var result = await userManager.CreateAsync(users, model.Password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(users, "Reviewer");

                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
            }
            return View(model);
        }
        public IActionResult VerifyEmail()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "Something is wrong!");
                    return View(model);
                }
                else
                {
                    return RedirectToAction("ChangePassword", "Account", new { username = user.UserName });
                }
            }
            return View(model);
        }
        public IActionResult ChangePassword(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("VerifyEmail", "Account");
            }
            return View(new ChangePasswordViewModel { Email = username });
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(model.Email);
                if (user != null)
                {
                    var result = await userManager.RemovePasswordAsync(user);
                    if (result.Succeeded)
                    {
                        result = await userManager.AddPasswordAsync(user, model.NewPassword);
                        return RedirectToAction("Login", "Account");
                    }
                    else
                    {

                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Email not found!");
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("", "Something went Wrong");
                return View(model);
            }
        }
        public async Task<IActionResult> Logout()
        {
            var userId = userManager.GetUserId(User);

            if (!string.IsNullOrEmpty(userId))
            {
                // Find active session
                var openSession = _context.UserSessionLogs
                    .Where(x => x.UserId == userId && x.LogoutTime == null)
                    .OrderByDescending(x => x.LoginTime)
                    .FirstOrDefault();

                if (openSession != null)
                {
                    openSession.LogoutTime = DateTime.UtcNow;
                    openSession.SessionMinutes =
                        (int)(openSession.LogoutTime.Value - openSession.LoginTime).TotalMinutes;
                    openSession.IsAutoLogout = false;

                    await _context.SaveChangesAsync();
                }
            }

            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

    }
}
