using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserProfileApp.Models;
using System.Threading.Tasks;

namespace UserProfileApp.Controllers
{
    public class AccountsController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountsController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // ========================
        // REGISTER (GET)
        // ========================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // ========================
        // REGISTER (POST)
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    TempData["SuccessMessage"] = "Registration successful!";
                    return RedirectToAction("Index", "Home"); // Redirect to dashboard (Index)
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        // ========================
        // LOGIN (GET)
        // ========================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // ========================
        // LOGIN (POST)
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false
                );

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Login successful!";
                    return RedirectToAction("Index", "Home"); // Redirect to dashboard (Index)
                }

                ModelState.AddModelError("", "Invalid login attempt. Please try again.");
            }

            return View(model);
        }

        // ========================
        // LOGOUT
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["SuccessMessage"] = "You have been logged out.";
            return RedirectToAction("Index", "Home"); // Return to home page
        }
    }
}
