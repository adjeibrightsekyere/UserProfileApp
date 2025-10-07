using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UserProfileApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace UserProfileApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        // ---------------- CREATE ROLE ----------------
        [HttpGet]
        public IActionResult CreateRole() => View();

        [HttpPost]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                ModelState.AddModelError("", "Role name is required");
                return View();
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                if (result.Succeeded)
                {
                    TempData["Success"] = "Role created successfully!";
                    return RedirectToAction("CreateRole");
                }
                foreach (var err in result.Errors)
                    ModelState.AddModelError("", err.Description);
            }
            else
            {
                ModelState.AddModelError("", "Role already exists");
            }

            return View();
        }

        // ---------------- ASSIGN ROLE ----------------
        [HttpGet]
        public IActionResult AssignRole()
        {
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            var users = _userManager.Users.Select(u => u.Email).ToList();
            ViewBag.Roles = new SelectList(roles);
            ViewBag.Users = new SelectList(users);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(string email, string role)
        {
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            var users = _userManager.Users.Select(u => u.Email).ToList();
            ViewBag.Roles = new SelectList(roles);
            ViewBag.Users = new SelectList(users);

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null && await _roleManager.RoleExistsAsync(role))
            {
                var result = await _userManager.AddToRoleAsync(user, role);
                if (result.Succeeded)
                {
                    TempData["Success"] = "Role assigned successfully!";
                    return RedirectToAction("AssignRole");
                }
                foreach (var err in result.Errors)
                    ModelState.AddModelError("", err.Description);
            }
            else
            {
                ModelState.AddModelError("", "User or Role not found");
            }

            return View();
        }

        // ---------------- CREATE USER ----------------
        [HttpGet]
        public IActionResult CreateUser()
        {
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            ViewBag.Roles = new SelectList(roles);
            return View(new CreateUserViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            ViewBag.Roles = new SelectList(roles);

            if (!ModelState.IsValid)
                return View(model);

            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.Role) && await _roleManager.RoleExistsAsync(model.Role))
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                }
                TempData["Success"] = "User created successfully!";
                return RedirectToAction("CreateUser");
            }

            foreach (var err in result.Errors)
                ModelState.AddModelError("", err.Description);

            return View(model);
        }

        // ---------------- LIST USERS AND ROLES ----------------
        [HttpGet]
        public async Task<IActionResult> ListUsersAndRoles()
        {
            var users = _userManager.Users.ToList();
            var roles = _roleManager.Roles.ToList();


            var userRoles = new Dictionary<string, IList<string>>();
            foreach (var user in users)
            {
                var rolesForUser = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = rolesForUser;
            }

            ViewBag.Users = users;
            ViewBag.Roles = roles;
            ViewBag.UserRoles = userRoles;
            return View();
        }
    }
}
