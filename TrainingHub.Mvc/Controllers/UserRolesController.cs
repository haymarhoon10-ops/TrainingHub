using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingHub.Models;
using TrainingHub.Mvc.Models;
using TrainingHub.Security;

namespace TrainingHub.Mvc.Controllers
{
    [Authorize(Roles = RoleNames.TrainingCoordinator)]
    public class UserRolesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRolesController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .OrderBy(user => user.Email)
                .ToListAsync();

            var model = new List<(ApplicationUser User, string Role)>();

            foreach (var user in users)
            {
                var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "No role";
                model.Add((user, role));
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
            {
                return NotFound();
            }

            var selectedRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? RoleNames.Trainee;

            return View(new UserRoleEditViewModel
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                SelectedRole = selectedRole,
                AvailableRoles = RoleNames.All
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserRoleEditViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId.ToString());

            if (user == null)
            {
                return NotFound();
            }

            if (!RoleNames.All.Contains(model.SelectedRole))
            {
                ModelState.AddModelError(nameof(model.SelectedRole), "Select a valid role.");
            }

            if (!ModelState.IsValid)
            {
                model.AvailableRoles = RoleNames.All;
                return View(model);
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Count > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Unable to update the user's current role.");
                    model.AvailableRoles = RoleNames.All;
                    return View(model);
                }
            }

            var addResult = await _userManager.AddToRoleAsync(user, model.SelectedRole);
            if (!addResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Unable to assign the selected role.");
                model.AvailableRoles = RoleNames.All;
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}