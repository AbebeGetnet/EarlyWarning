using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Policy;
using EarlyWarning.Data;
using EarlyWarning.Models;
using EarlyWarning.Static_files;
using EarlyWarning.ViewModels.UserManagementViewModel;

namespace EarlyWarning.Controllers
{
    // User management is for the Supper Administrator only; ChangePassword below stays open to every signed-in user.
    [Authorize(Roles = SD.SuperAdmin)]
    [AutoValidateAntiforgeryToken]
    public class UserRolesController : Controller
    {
        /// <summary>The password an administrator's reset gives a user; the user should change it after logging in.</summary>
        public const string DefaultPassword = "SaleGod@1";

        private readonly EarlyWarningDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly PasswordHasher<PasswordResetKey> _keydHasher;

        public UserRolesController(EarlyWarningDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IUserStore<ApplicationUser> userStore, SignInManager<ApplicationUser> signInManager, ILogger<RegisterModel> logger, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _userStore = userStore;
            _keydHasher = new PasswordHasher<PasswordResetKey>();
        }
        public IActionResult RedirectToLogin()
        {
            string loginUrl = Url.Action("Login", "Account", new { area = "Identity" });
            return Redirect(loginUrl);
        }
        public IActionResult RedirectToRegister()
        {
            string registrarUrl = Url.Action("Register", "Account", new { area = "Identity" });
            return Redirect(registrarUrl);
        }
        public IActionResult RedirectToRegisterLecturer()
        {
            string registrarUrl = Url.Action("RegisterLecture", "Account", new { area = "Identity" });
            return Redirect(registrarUrl);
        }
        public async Task<IActionResult> UserRoleIndex()
        {
            var users = await _userManager.Users.Include(d => d.Location).ToListAsync();
            var userRolesViewModel = new List<UserRolesViewModel>();
            foreach (ApplicationUser user in users)
            {
                var thisViewModel = new UserRolesViewModel();
                thisViewModel.UserId = user.Id;
                thisViewModel.FirstName = user.FirstName;
                thisViewModel.LastName = user.LastName;
                thisViewModel.LocationName = user.Location.LocationName;
                thisViewModel.Email = user.Email;
                thisViewModel.Roles = await GetUserRoles(user);
                userRolesViewModel.Add(thisViewModel);
            }
            return View(userRolesViewModel);
        }
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.Include(d => d.Location).ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> PasswordResetKeyIndex()
        {
            var passwordResetKeys = await _context.PasswordResetKeys.ToListAsync();
            return View(passwordResetKeys);
        }
        public async Task<IActionResult> AddPasswordResetKey()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddPasswordResetKey(string PasswordResetValue, string KeyNote)
        {
            var existingPasswordResetKey = await _context.PasswordResetKeys.FirstOrDefaultAsync();
            if (existingPasswordResetKey == null)
            {
                if (ModelState.IsValid)
                {
                    var key = new PasswordResetKey
                    {
                        PasswordResetValue = PasswordResetValue,
                        KeyNote = KeyNote
                    };
                    key.PasswordResetValue = _keydHasher.HashPassword(key, PasswordResetValue);
                    _context.Add(key);
                    _context.SaveChanges();
                    TempData["success"] = "Successful";
                    return RedirectToAction(nameof(PasswordResetKeyIndex));
                }
                TempData["error"] = "Checks your inputs";
                return View(nameof(PasswordResetKeyIndex));
            }
            else
            {
                _context.Remove(existingPasswordResetKey);
                await _context.SaveChangesAsync();

                if (ModelState.IsValid)
                {
                    var key = new PasswordResetKey
                    {
                        PasswordResetValue = PasswordResetValue,
                        KeyNote = KeyNote,
                    };
                    key.PasswordResetValue = _keydHasher.HashPassword(key, PasswordResetValue);
                    _context.Add(key);
                    _context.SaveChanges();
                    TempData["success"] = "Successful";
                    return RedirectToAction(nameof(PasswordResetKeyIndex));
                }
                TempData["error"] = "Checks your inputs";
                return View(nameof(PasswordResetKeyIndex));
            }
        }

        public async Task<IActionResult> Manage(string userId)
        {
            ViewBag.userId = userId;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("NotFound");
            }
            ViewBag.UserName = user.UserName;
            var model = new List<ManageUserRolesViewModel>();
            foreach (var role in _roleManager.Roles)
            {
                var userRolesViewModel = new ManageUserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.Selected = true;
                }
                else
                {
                    userRolesViewModel.Selected = false;
                }
                model.Add(userRolesViewModel);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Manage(List<ManageUserRolesViewModel> model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View();
            }
            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(model);
            }
            result = await _userManager.AddToRolesAsync(user, model.Where(x => x.Selected).Select(y => y.RoleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected roles to user");
                return View(model);
            }
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> EditUser(string userId)
        {
            ViewBag.userId = userId;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("NotFound");
            }
            ViewBag.UserName = user.UserName;
            var model = new List<ManageUserRolesViewModel>();
            foreach (var role in _roleManager.Roles)
            {
                var userRolesViewModel = new ManageUserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.Selected = true;
                }
                else
                {
                    userRolesViewModel.Selected = false;
                }
                model.Add(userRolesViewModel);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(List<ManageUserRolesViewModel> model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View();
            }
            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(model);
            }
            result = await _userManager.AddToRolesAsync(user, model.Where(x => x.Selected).Select(y => y.RoleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected roles to user");
                return View(model);
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ManageRole(string userId)
        {
            ViewBag.userId = userId;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("NotFound");
            }
            ViewBag.UserName = user.UserName;
            var model = new List<ManageUserRolesViewModel>();
            foreach (var role in _roleManager.Roles)
            {
                var userRolesViewModel = new ManageUserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.Selected = true;
                }
                else
                {
                    userRolesViewModel.Selected = false;
                }
                model.Add(userRolesViewModel);
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ManageRole(List<ManageUserRolesViewModel> model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View();
            }
            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(model);
            }
            result = await _userManager.AddToRolesAsync(user, model.Where(x => x.Selected).Select(y => y.RoleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected roles to user");
                return View(model);
            }
            return RedirectToAction("UserRoleIndex");
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var objfromDb = _context.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            if (objfromDb == null)
            {
                return NotFound();
            }
            var userRole = _context.UserRoles.ToList();
            var roles = _context.Roles.ToList();
            var role = userRole.FirstOrDefault(r => r.UserId == objfromDb.Id);
            if (role != null)
            {
                objfromDb.RoleId = roles.FirstOrDefault(u => u.Id == role.RoleId).Id;
            }
            objfromDb.RoleList = _context.Roles.Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Text = u.Name,
                Value = u.Id
            });
            return View(objfromDb);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ApplicationUser user)
        {
            if (ModelState.IsValid)
            {
                var objfromDb = _context.ApplicationUsers.FirstOrDefault(u => u.Id == user.Id);
                if (objfromDb == null)
                {
                    return NotFound();
                }
                var userRole = _context.UserRoles.FirstOrDefault(u => u.UserId == objfromDb.Id);


                if (userRole != null)
                {
                    var previousRoleName = _context.Roles.Where(u => u.Id == userRole.RoleId).Select(e => e.Name).FirstOrDefault();
                    //remove previous role
                    await _userManager.RemoveFromRoleAsync(objfromDb, previousRoleName);

                }
                //add new role
                //await _userManager.AddToRoleAsync(objfromDb, _context.Roles.FirstOrDefault(u => u.Id == user.RoleId).Name);
                //objfromDb.FullName = user.FullName;
                _context.SaveChanges();
                //TempData[SD.Success] = "User has been edited successfully.";
                return RedirectToAction(nameof(Index));
            }

            user.RoleList = _context.Roles.Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Text = u.Name,
                Value = u.Id
            });
            return View(user);
        }
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            if (id == null)
            {
                return BadRequest("User ID is required.");
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmDelete(string id)
        {
            if (id == null)
            {
                return BadRequest("User ID is required.");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index)); // Redirect after successful deletion
            }

            // AddErrors(result); 
            return View("DeleteUser", user); // Redisplay deletion view with errors
        }

        private async Task<List<string>> GetUserRoles(ApplicationUser user)
        {
            return new List<string>(await _userManager.GetRolesAsync(user));
        }

        // Administrator: set a user's password back to DefaultPassword (and lift a lockout from failed logins).
        // POST only, so a link or a crawler can never reset a password.
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData[SD.Error] = "ተጠቃሚው አልተገኘም።";
                return RedirectToAction(nameof(Index));
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, DefaultPassword);
            if (!result.Succeeded)
            {
                TempData[SD.Error] = "የይለፍ ቃሉን መመለስ አልተቻለም፡ " + string.Join(" ", result.Errors.Select(PasswordError));
                return RedirectToAction(nameof(Index));
            }
            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);
            _logger.LogInformation("Password of user {UserId} reset to the default by {Admin}.", user.Id, _userManager.GetUserId(User));
            TempData[SD.Success] = $"የ{user.FirstName} {user.LastName} የይለፍ ቃል ወደ መነሻው ({DefaultPassword}) ተመልሷል። ተጠቃሚው ከገባ በኋላ እንዲቀይረው ያሳውቁ።";
            return RedirectToAction(nameof(Index));
        }

        // Every signed-in user: change their own password (never anyone else's).
        // AllowAnonymous lifts the controller's Supper Administrator rule; the signed-in check is done here instead.
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ChangePassword() =>
            User.Identity?.IsAuthenticated == true ? View(new ChangePasswordViewModel()) : Challenge();

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (User.Identity?.IsAuthenticated != true) return Challenge();
            if (!ModelState.IsValid) return View(model);
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(error.Code == "PasswordMismatch" ? nameof(model.CurrentPassword) : nameof(model.NewPassword), PasswordError(error));
                return View(model);
            }
            await _signInManager.RefreshSignInAsync(user);
            TempData[SD.Success] = "የይለፍ ቃልዎ በተሳካ ሁኔታ ተቀይሯል።";
            return RedirectToAction("Index", "Home");
        }

        /// <summary>Identity's password errors in Amharic.</summary>
        internal static string PasswordError(IdentityError error) => error.Code switch
        {
            "PasswordMismatch" => "የአሁኑ የይለፍ ቃል ትክክል አይደለም።",
            "PasswordTooShort" => "የይለፍ ቃሉ በጣም አጭር ነው (ቢያንስ 6 ፊደላት)።",
            "PasswordRequiresDigit" => "የይለፍ ቃሉ ቢያንስ አንድ ቁጥር (0-9) መያዝ አለበት።",
            "PasswordRequiresLower" => "የይለፍ ቃሉ ቢያንስ አንድ ትንሽ የእንግሊዝኛ ፊደል (a-z) መያዝ አለበት።",
            "PasswordRequiresUpper" => "የይለፍ ቃሉ ቢያንስ አንድ ትልቅ የእንግሊዝኛ ፊደል (A-Z) መያዝ አለበት።",
            "PasswordRequiresNonAlphanumeric" => "የይለፍ ቃሉ ቢያንስ አንድ ምልክት (ለምሳሌ @ # !) መያዝ አለበት።",
            "PasswordRequiresUniqueChars" => "የይለፍ ቃሉ የተለያዩ ፊደላትን መያዝ አለበት።",
            _ => error.Description
        };
    }
}
