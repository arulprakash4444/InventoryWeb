using Inventory.DataAccess.Repository.IRepository;
using Inventory.Models;
using Inventory.Models.ViewModels;
using Inventory.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryWeb.Controllers
{

    [Authorize(Roles = SD.Role_Admin)]
    public class ApplicationUserController : Controller
    {
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ApplicationUserController(IApplicationUserRepository applicationUserRepository, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _applicationUserRepository = applicationUserRepository;
            _userManager = userManager;
            _roleManager = roleManager;
        }


        //public IActionResult Index()
        //{
        //    var users = _applicationUserRepository.GetAll();
        //    return View(users);
        //}

        public async Task<IActionResult> Index()
        {
            List<ApplicationUserWithRole> users = await _applicationUserRepository.GetAllUsersWithRolesAsync();
            return View(users);
        }


        //// -------------------- EDIT (GET) --------------------
        //public async Task<IActionResult> Edit(string id)
        //{
        //    ApplicationUser user = await _applicationUserRepository.GetUserAsync(id);

        //    if (user == null)
        //        return NotFound();

        //    IEnumerable<SelectListItem> roles = _roleManager.Roles.Select(r => new SelectListItem
        //    {
        //        Text = r.Name,
        //        Value = r.Name
        //    });

        //    string currentRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

        //    ApplicationUserEditVM applicationUserEditVM = new ApplicationUserEditVM
        //    {
        //        User = user,
        //        RoleList = roles,
        //        SelectedRole = currentRole ?? ""
        //    };

        //    return View(applicationUserEditVM);
        //}


        //// -------------------- EDIT (POST) --------------------
        //[HttpPost]
        //public async Task<IActionResult> Edit(ApplicationUserEditVM applicationUserEditVM)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        applicationUserEditVM.RoleList = _roleManager.Roles.Select(r => new SelectListItem
        //        {
        //            Text = r.Name,
        //            Value = r.Name
        //        });
        //        return View(applicationUserEditVM);
        //    }

        //    ApplicationUser user = await _applicationUserRepository.GetUserAsync(applicationUserEditVM.User.Id);
        //    if (user == null)
        //        return NotFound();

        //    // Update fields
        //    user.UserName = applicationUserEditVM.User.UserName;
        //    user.Name = applicationUserEditVM.User.Name;
        //    user.PhoneNumber = applicationUserEditVM.User.PhoneNumber;
        //    user.Address = applicationUserEditVM.User.Address;

        //    await _applicationUserRepository.UpdateUserAsync(user);

        //    // Get current role
        //    IList<string> currentRoles = await _userManager.GetRolesAsync(user);

        //    string currentRole = currentRoles.FirstOrDefault() ?? string.Empty;
        //    string selectedRole = applicationUserEditVM.SelectedRole;

        //    // Update only if the role changed
        //    if (!string.Equals(currentRole, selectedRole, StringComparison.OrdinalIgnoreCase))
        //    {
        //        // Remove old roles
        //        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        //        // Assign new role
        //        await _userManager.AddToRoleAsync(user, selectedRole);

        //        // IMPORTANT if the user is made employee from admin, they may still have 
        //        // admin rights, till they logout, below line invalidates, when admin revokes rights
        //        await _userManager.UpdateSecurityStampAsync(user);
        //    }

        //    return RedirectToAction("Index");
        //}



        public async Task<IActionResult> EditRole(string id)
        {
            ApplicationUser? user = await _applicationUserRepository.GetUserAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            IEnumerable<IdentityRole> rolesFromDb = _roleManager.Roles;
            IEnumerable<SelectListItem> roles = rolesFromDb.Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Name
            });

            IList<string> userRoles = await _userManager.GetRolesAsync(user);
            string currentRole = userRoles.FirstOrDefault() ?? string.Empty;

            ApplicationUserRoleEditVM applicationUserRoleEditVM = new ApplicationUserRoleEditVM
            {
                UserId = user.Id,
                UserName = user.UserName,
                RoleList = roles,
                SelectedRole = currentRole
            };

            return View(applicationUserRoleEditVM);
        }





        [HttpPost]
        public async Task<IActionResult> EditRole(ApplicationUserRoleEditVM applicationUserRoleEditVM)
        {
            if (!ModelState.IsValid)
            {
                IEnumerable<SelectListItem> roles = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                });

                applicationUserRoleEditVM.RoleList = roles;
                return View(applicationUserRoleEditVM);
            }

            ApplicationUser? user = await _applicationUserRepository.GetUserAsync(applicationUserRoleEditVM.UserId);

            if (user == null)
            {
                return NotFound();
            }

            // username is only for display, not updated
            string selectedRole = applicationUserRoleEditVM.SelectedRole;

            IList<string> currentRoles = await _userManager.GetRolesAsync(user);
            string currentRole = currentRoles.FirstOrDefault() ?? string.Empty;

            // Only change role if different
            if (!string.Equals(currentRole, selectedRole, StringComparison.OrdinalIgnoreCase))
            {
                // Remove existing roles
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                // Add new role
                await _userManager.AddToRoleAsync(user, selectedRole);

                // Updates security stamp, for next request to logout
                await _userManager.UpdateSecurityStampAsync(user);
            }

            return RedirectToAction("Index");
        }





        // -------------------- DELETE (GET)
        public async Task<IActionResult> Delete(string id)
        {
            ApplicationUser? user = await _applicationUserRepository.GetUserAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            IList<string> roles = await _userManager.GetRolesAsync(user);
            string role = roles.FirstOrDefault() ?? string.Empty;

            ApplicationUserWithRole applicationUserEditVM = new ApplicationUserWithRole
            {
                UserId = user.Id,
                UserName = user.UserName,
                Name = user.Name ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Address = user.Address ?? string.Empty,
                Role = role
            };

            return View(applicationUserEditVM);
        }



        //// -------------------- DELETE (POST)
        //[HttpPost, ActionName("Delete")]
        //public async Task<IActionResult> DeletePOST(string id)
        //{
        //    var user = await _applicationUserRepository.GetUserAsync(id);
        //    if (user == null)
        //        return NotFound();

        //    await _applicationUserRepository.DeleteUserAsync(user);

        //    return RedirectToAction("Index");
        //}


        // -------------------- DELETE (POST)
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeletePOST(string id)
        {
            var user = await _applicationUserRepository.GetUserAsync(id);
            if (user == null)
                return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);

            // -------------------- CHECK 1: Prevent deleting self --------------------
            if (currentUser != null && currentUser.Id == user.Id)
            {
                TempData["error"] = "You cannot delete your own Admin account.";
                return RedirectToAction("Index");
            }

            // -------------------- CHECK 2: Prevent deleting last admin --------------------
            if (await _userManager.IsInRoleAsync(user, SD.Role_Admin))
            {
                var allAdmins = await _userManager.GetUsersInRoleAsync(SD.Role_Admin);

                // If this user is the ONLY admin → block deletion
                if (allAdmins.Count == 1)
                {
                    TempData["error"] = "At least one Admin user is required. You cannot delete the only Admin.";
                    return RedirectToAction("Index");
                }
            }

            // -------------------- DELETE USER --------------------
            await _applicationUserRepository.DeleteUserAsync(user);

            TempData["success"] = "User deleted successfully.";
            return RedirectToAction("Index");
        }



        //-----------------------EditPassword
        public async Task<IActionResult> EditPassword(string id)
        {
            ApplicationUser user = await _applicationUserRepository.GetUserAsync(id);

            if (user == null)
                return NotFound();

            var applicationUserPasswordEditVM = new ApplicationUserPasswordEditVM
            {
                UserId = user.Id,
                UserName = user.UserName
            };

            return View(applicationUserPasswordEditVM);
        }



        //----------------------------Edit Password (POST)

        [HttpPost]
        public async Task<IActionResult> EditPassword(ApplicationUserPasswordEditVM applicationUserPasswordEditVM)
        {
            if (!ModelState.IsValid)
            {
                return View(applicationUserPasswordEditVM);
            }

            ApplicationUser? user = await _applicationUserRepository.GetUserAsync(applicationUserPasswordEditVM.UserId);
            if (user == null)
            {
                return NotFound();
            }

            // -------------------- Validate password using Identity validators --------------------
            foreach (IPasswordValidator<ApplicationUser> validator in _userManager.PasswordValidators)
            {
                IdentityResult result = await validator.ValidateAsync(_userManager, user, applicationUserPasswordEditVM.NewPassword);

                if (!result.Succeeded)
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("NewPassword", error.Description);
                    }
                    return View(applicationUserPasswordEditVM);
                }
            }

            // -------------------- Remove old password --------------------
            IdentityResult removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError("", "Unable to remove old password.");
                return View(applicationUserPasswordEditVM);
            }

            // -------------------- Add new password --------------------
            IdentityResult addResult = await _userManager.AddPasswordAsync(user, applicationUserPasswordEditVM.NewPassword);
            if (!addResult.Succeeded)
            {
                foreach (IdentityError error in addResult.Errors)
                {
                    ModelState.AddModelError("NewPassword", error.Description);
                }
                return View(applicationUserPasswordEditVM);
            }

            // -------------------- Force logout from all sessions --------------------
            await _userManager.UpdateSecurityStampAsync(user);

            return RedirectToAction("Index");
        }


    }

}

