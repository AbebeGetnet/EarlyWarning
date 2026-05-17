using EarlyWarning.ViewModel;
using EarlyWarning.Data;
using EarlyWarning.Enums;
using EarlyWarning.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Collections.Specialized.BitVector32;
using EarlyWarning.ViewModels.AddressViewModel;

namespace EarlyWarning.Controllers
{
    //[Authorize(Roles = "Supper Administrator")]
    public class LocationsController : Controller
    {
        private readonly EarlyWarningDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly IEmailSender _emailSender;

        public LocationsController(EarlyWarningDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender)

        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _userStore = userStore;
        }
        public async Task<JsonResult> GetKebelesByWoreda()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var kebeles = _context.Locations
                .Where(x => x.ParentId == currentUser.LocationId && x.Level == LocationLevel.ቀበሌ)
                .Select(x => new { id = x.Id, name = x.LocationName })
                .ToList();

            return Json(kebeles);
        }
        // GET: Locations
        //[Authorize(Roles = "Supper Administrator, Administrator")]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            if (currentUser != null)
            {
                if (HttpContext.User.IsInRole("Supper Administrator"))
                {
                    var applicationDbContext = _context.Locations.Include(l => l.Parent).Where(l => l.Level != LocationLevel.ሀገር);
                    return View(await applicationDbContext.ToListAsync());
                }
                else if (HttpContext.User.IsInRole("Administrator"))
                {
                    var currentUserLocation = await _context.Locations.Include(l => l.Parent).Where(l => l.Id == currentUser.LocationId).FirstOrDefaultAsync();
                    var currentUserChildLocation = await _context.Locations.Include(l => l.Parent).Where(l => l.ParentId == currentUserLocation.Id).FirstOrDefaultAsync();
                    var locations = await _context.Locations.Include(l => l.Parent).Where(l => l.Id == currentUserLocation.Id || l.Id == currentUserLocation.ParentId || l.Id == currentUserChildLocation.Id).ToListAsync();
                    return View(locations);
                }
                else
                {
                    var locations = _context.Locations.Include(l => l.Parent).Where(l => l.Level != LocationLevel.ሀገር);
                    return View(locations);
                }
            }
            TempData["error"] = "መረጃ የለም።";
            return View();
        }
        public async Task<IActionResult> ZoneIndex()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            if (currentUser != null)
            {
                if (HttpContext.User.IsInRole("Supper Administrator"))
                {
                    var applicationDbContext = _context.Locations.Include(l => l.Parent).Where(l => l.Level == LocationLevel.ዞን);
                    return View(await applicationDbContext.ToListAsync());
                }
                else if (HttpContext.User.IsInRole("Administrator"))
                {
                    var locations = await _context.Locations.Include(l => l.Parent).Where(l => l.Id == currentUser.LocationId || l.ParentId == currentUser.LocationId).ToListAsync();
                    return View(locations);
                }
            }
            TempData["error"] = "ያልተፈቀደ።";
            return View();
        }
        public async Task<IActionResult> WoredaIndex()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            if (currentUser != null)
            {
                if (HttpContext.User.IsInRole("Supper Administrator"))
                {
                    var applicationDbContext = _context.Locations.Include(l => l.Parent).Where(l => l.Level == LocationLevel.ወረዳ);
                    return View(await applicationDbContext.ToListAsync());
                }
                else if (HttpContext.User.IsInRole("Administrator"))
                {
                    var locations = await _context.Locations.Include(l => l.Parent).Where(l => l.Id == currentUser.LocationId || l.ParentId == currentUser.LocationId).ToListAsync();
                    return View(locations);
                }
            }
            TempData["error"] = "ያልተፈቀደ።";
            return View();
        }
        public async Task<IActionResult> KebelieIndex()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            if (currentUser != null)
            {
                if (HttpContext.User.IsInRole("Supper Administrator"))
                {
                    var applicationDbContext = _context.Locations.Include(l => l.Parent).Where(l => l.Level == LocationLevel.ቀበሌ);
                    return View(await applicationDbContext.ToListAsync());
                }
                else if (HttpContext.User.IsInRole("Administrator"))
                {
                    var locations = await _context.Locations.Include(l => l.Parent).Where(l => l.Id == currentUser.LocationId || l.ParentId == currentUser.LocationId).ToListAsync();
                    return View(locations);
                }
            }
            TempData["error"] = "ያልተፈቀደ።";
            return View();
        }


        // GET: Locations
        [AllowAnonymous]
        public async Task<IActionResult> LocationsIndex()
        {
            var applicationDbContext = _context.Locations.Include(l => l.Parent);
            return View(await applicationDbContext.ToListAsync());
        }
        // GET: Locations
        public async Task<IActionResult> LocationIndex(Guid id)
        {
            if (id != Guid.Empty)
            {
                var locations = await _context.Locations.Include(l => l.Parent).Where(l => l.ParentId == id).ToListAsync(); ;
                return View(locations);
            }
            TempData["error"] = "Select Parent Location First";
            return RedirectToAction(nameof(Index));
        }
        // GET: Locations
        public async Task<IActionResult> CategorizedLocationList()
        {
            var zones = await _context.Locations.Include(l => l.Parent).Where(l => l.Level == LocationLevel.ዞን).ToListAsync(); ;
            var woredas = await _context.Locations.Include(l => l.Parent).Where(l => l.Level == LocationLevel.ወረዳ).ToListAsync(); ;
            var keblies = await _context.Locations.Include(l => l.Parent).Where(l => l.Level == LocationLevel.ቀበሌ).ToListAsync(); ;
            var locationViewModel = new LocationViewModel()
            {
                ZoneList = zones,
                WoredaList = woredas,
                KebelieList = keblies
            };
            return View(locationViewModel);
        }

        // GET: Locations/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var location = await _context.Locations
                .Include(l => l.Parent)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (location == null)
            {
                return NotFound();
            }

            return View(location);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Levels = new SelectList(Enum.GetValues(typeof(LocationLevel)));
            ViewData["ParentId"] = new SelectList(await _context.Locations.ToListAsync(), "Id", "LocationName");
            return View();
        }

        // POST: Locations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Locations location)
        {
            if (!ModelState.IsValid)
            {
                if (!await LocationCodeExistsAsync(location.Level, location.LocationCode))
                {
                    location.Id = Guid.NewGuid();
                    _context.Add(location);
                                        
                    await _context.SaveChangesAsync();
                    TempData["success"] = "በትክክል ተመዝግቧል።";
                    return RedirectToAction(nameof(Index));
                }
                TempData["error"] = "ያስገቡት የአድራሻ መለያ ኮድ ተይዟል።";
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParentId"] = new SelectList(await _context.Locations.ToListAsync(), "Id", "LocationName", location.ParentId);
            TempData["error"] = "ያስገቡትን መረጃ ይፈትሹ።";
            return View(location);
        }

        // GET: Locations/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id != Guid.Empty)
            {
                var location = await _context.Locations.FindAsync(id);
                if (location != null)
                {
                    var model = new AddressViewModel()
                    {
                        Id = location.Id,
                        ParentId = location.ParentId,
                        LocationAmharicName = location.LocationAmharicName,
                        LocationName = location.LocationName,
                        LocationCode = location.LocationCode,
                        PhoneNumber = location.PhoneNumber,
                    };
                    var locations = await _context.Locations.ToListAsync();
                    ViewBag.Locations = locations;
                    return View(model);
                }
                TempData["error"] = "No address selected to update";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "No address selected to update";
            return RedirectToAction(nameof(Index));
        }

        // POST: Locations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, AddressViewModel model)
        {
            if (id == Guid.Empty)
            {
                TempData["error"] = "No data available";
                return RedirectToAction(nameof(Index));
            }
            var location = await _context.Locations.Include(l => l.Parent).FirstOrDefaultAsync(l => l.Id == id);
            if (location != null)
            {
                location.LocationAmharicName = model.LocationAmharicName;
                location.LocationName = model.LocationName;
                location.LocationCode = model.LocationCode;
                location.Level = model.Level;
                location.PhoneNumber = model.PhoneNumber;
                location.CardHeaderTitle = model.CardHeaderTitle;
                location.ParentId = model.ParentId;
                _context.Update(location);
                
                await _context.SaveChangesAsync();
                TempData["success"] = "በትክክል ተስተካክሏል";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Nothing changed";
            return RedirectToAction(nameof(Index));
        }
        private Task<bool> LocationExistsAsync(Guid id)
        {
            return _context.Locations.AnyAsync(e => e.Id == id);
        }
        private Task<bool> LocationCodeExistsAsync(LocationLevel? level, string code)
        {
            return _context.Locations.AnyAsync(e => e.Level == level && e.LocationCode == code);
        }
    }
}
