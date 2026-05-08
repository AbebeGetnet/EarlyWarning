using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using EarlyWarning.Data;
using EarlyWarning.Models;
using EarlyWarning.Static_files;

namespace EarlyWarning.Controllers
{
    //[Authorize(Roles = "Administrator")]
    [AutoValidateAntiforgeryToken]
    public class RolesController : Controller
    {
        private readonly EarlyWarningDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(EarlyWarningDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // [Authorize(Roles = "Supper Administrator, Admin")]

        public IActionResult Index()
        {
            var roles = _context.Roles.ToList();
            return View(roles);
        }

        // [Authorize(Roles = "Administrator, Admin")]
        public IActionResult Upsert(string id)
        {

            if (string.IsNullOrEmpty(id))
            {
                //create
                return View();
            }
            else
            {
                //update
                var role = _context.Roles.FirstOrDefault(r => r.Id == id);
                TempData["success"] = "Role added successfully";

                return View(role);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(IdentityRole role)
        {
            if (await _roleManager.RoleExistsAsync(role.Name))
            {
                //allready exists
                TempData["error"] = "Role Allredy exists.";
            }
            if (string.IsNullOrEmpty(role.Id))
            {
                //create
                await _roleManager.CreateAsync(new IdentityRole() { Name = role.Name });
                TempData["success"] = "User created successfully";
            }
            else
            {
                //update
                var roleFromDb = _context.Roles.FirstOrDefault(r => r.Id == role.Id);
                if (roleFromDb == null)
                {
                    TempData[SD.Success] = "Role not found.";
                    return RedirectToAction(nameof(Index));
                }
                roleFromDb.Name = role.Name;
                roleFromDb.NormalizedName = role.Name.ToUpper();
                var result = await _roleManager.UpdateAsync(roleFromDb);
                TempData["success"] = "Updated successfully";
            }
            return RedirectToAction(nameof(Index));
        }

        // [Authorize(Roles = "Administrator, Admin")]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var roleFromDb = _context.Roles.FirstOrDefault(r => r.Id == id);
            if (roleFromDb == null)
            {
                //TesmData[SD.Error] = "Cannot find this role.";
                return RedirectToAction(nameof(Index));
            }
            var userRolesForThisRole = _context.UserRoles.Where(u => u.RoleId == id).Count();
            if (userRolesForThisRole > 0)
            {
                //TesmData[SD.Error] = "Cannot delete this role, since there are users assigned for this role";
                return RedirectToAction(nameof(Index));
            }
            await _roleManager.DeleteAsync(roleFromDb);

            return RedirectToAction(nameof(Index));
        }
    }
}
