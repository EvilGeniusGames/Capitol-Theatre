using Capitol_Theatre.Data;
using Capitol_Theatre.Models;
using Capitol_Theatre.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;

[Authorize]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [Authorize(Roles = "Administrator")]
    public IActionResult Index()
    {
        var pages = _context.PageContents.ToList();
        return View(pages);
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Users()
    {
        var users = _userManager.Users.ToList();
        return View(users);
    }

    // Updated UploadImage to support universal upload
    // POST: Admin/UploadImage
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> UploadImage([FromForm] IFormFile image, [FromForm] string folder)
    {
        try
        {
            if (image == null || image.Length == 0) return BadRequest("No file uploaded.");

            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", folder);
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

            var filePath = Path.Combine(uploads, Path.GetFileName(image.FileName));

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            var relativePath = $"/images/{folder}/{image.FileName}";
            return Json(new { location = relativePath });
        }
        catch (Exception ex)
        {
            Console.WriteLine("UploadImage Exception: " + ex.Message);
            return StatusCode(500, "Internal server error.");
        }
    }

    // Updated BrowseImages to accept any folder
    // GET: Admin/BrowseImages
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public IActionResult BrowseImages(string folder, string target)
    {
        if (string.IsNullOrWhiteSpace(folder))
            return BadRequest("Folder not specified.");

        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", folder);

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var images = Directory.GetFiles(folderPath)
            .Select(file => $"/Images/{folder}/" + Path.GetFileName(file))
            .ToList();

        ViewBag.TargetInput = target;
        ViewBag.Folder = folder;
        ViewBag.Images = images;

        return View();
    }
    //  GET: Admin/EditPage
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public IActionResult Edit(int id)
    {
        var page = _context.PageContents.FirstOrDefault(p => p.Id == id);
        if (page == null) return NotFound();
        return View("EditPage", page);
    }
    //  POST: Admin/EditPage
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public IActionResult Edit(PageContent model)
    {
        if (!ModelState.IsValid) return View("EditPage", model);

        var page = _context.PageContents.Find(model.Id);
        if (page == null) return NotFound();

        page.HtmlContent = model.HtmlContent ?? "";
        page.LastUpdated = DateTime.UtcNow;
        _context.SaveChanges();

        return RedirectToAction("Index");
    }
    // GET: Admin/UploadImages
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public IActionResult GetUploadedImages()
    {
        var folderPath = Path.Combine("wwwroot", "images", "upload");
        if (!Directory.Exists(folderPath))
            return Json(Array.Empty<string>());

        var files = Directory.GetFiles(folderPath)
            .Select(f => "/images/upload/" + Path.GetFileName(f))
            .ToArray();

        return Json(files);
    }
    // GET: Admin/ManageMovies
    [Authorize(Roles = "Administrator")]
    [HttpGet]
    public IActionResult ManageMovies(string sort = "Status", string dir = "asc")
    {
        var movies = _context.Movies.Include(m => m.Rating).Include(m => m.Showtimes).ToList();

        DateTime today = DateTime.Today;
        DateTime thisFriday = today.AddDays(-((int)today.DayOfWeek + 2) % 7);
        DateTime twoFridays = thisFriday.AddDays(14);

        string GetMovieStatus(Movie movie)
        {
            if (movie.EndShowingDate.HasValue && movie.EndShowingDate.Value < today)
                return "Expired";
            if (movie.StartShowingDate.HasValue)
            {
                if (movie.StartShowingDate.Value <= thisFriday && (!movie.EndShowingDate.HasValue || movie.EndShowingDate >= today))
                    return "Now Showing";
                if (movie.StartShowingDate.Value >= thisFriday && movie.StartShowingDate.Value < twoFridays)
                    return "Next Week";
                if (movie.StartShowingDate.Value >= twoFridays)
                    return "Coming Soon";
            }
            return "Unscheduled";
        }

        var sortedMovies = movies.Select(m => new MovieListing
        {
            Movie = m,
            Status = GetMovieStatus(m)
        });

        sortedMovies = (sort, dir) switch
        {
            ("Title", "asc") => sortedMovies.OrderBy(m => m.Movie.Title),
            ("Title", "desc") => sortedMovies.OrderByDescending(m => m.Movie.Title),
            ("Rating", "asc") => sortedMovies.OrderBy(m => m.Movie.Rating?.Code),
            ("Rating", "desc") => sortedMovies.OrderByDescending(m => m.Movie.Rating?.Code),
            ("Runtime", "asc") => sortedMovies.OrderBy(m => m.Movie.runtime),
            ("Runtime", "desc") => sortedMovies.OrderByDescending(m => m.Movie.runtime),
            ("Dates", "asc") => sortedMovies.OrderBy(m => m.Movie.StartShowingDate),
            ("Dates", "desc") => sortedMovies.OrderByDescending(m => m.Movie.StartShowingDate),
            ("Status", "desc") => sortedMovies.OrderByDescending(m => m.Status),
            _ => sortedMovies.OrderBy(m => m.Status)
        };

        ViewBag.Sort = sort;
        ViewBag.Dir = dir;

        return View(sortedMovies.ToList());
    }
    // GET: Admin/EditMovie
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Notices()
    {
        var notices = await _context.Notices.OrderByDescending(n => n.PostedAt).ToListAsync();
        return View(notices);
    }
    // GET: Admin/CreateNotice
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public IActionResult CreateNotice()
    {
        return View();
    }
    // POST: Admin/CreateNotice
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> CreateNotice(Notice notice)
    {
        if (ModelState.IsValid)
        {
            _context.Notices.Add(notice);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Notices));
        }
        return View(notice);
    }
    //  GET: Admin/EditNotice
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> EditNotice(int id)
    {
        var notice = await _context.Notices.FindAsync(id);
        if (notice == null) return NotFound();
        return View(notice);
    }
    // POST: Admin/EditNotice
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> EditNotice(int id, Notice notice)
    {
        if (id != notice.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(notice);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Notices.Any(e => e.Id == id)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Notices));
        }
        return View(notice);
    }
    // GET: Admin/DeleteNotice
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteNotice(int id)
    {
        var notice = await _context.Notices.FindAsync(id);
        if (notice == null) return NotFound();

        _context.Notices.Remove(notice);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Notices));
    }

    // GET: Admin/DeleteUser
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        return View(user);
    }
    // POST: Admin/DeleteUser
    [HttpPost, ActionName("DeleteUser")]
    [Authorize(Roles = "Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUserConfirmed(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(user);
        }

        return RedirectToAction("Users");
    }
    // GET: Admin/ManageUser
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> ManageUser(string? id)
    {
        if (string.IsNullOrEmpty(id))
        {
            // Create mode
            return View(new EditUserViewModel());
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        return View(new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email
        });
    }
    // POST: Admin/ManageUser
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> ManageUser(EditUserViewModel model)
    {
        if (string.IsNullOrEmpty(model.Id) && string.IsNullOrWhiteSpace(model.NewPassword))
        {
            ModelState.AddModelError("NewPassword", "Password is required when creating a user.");
        }

        if (!ModelState.IsValid)
            return View(model);

        if (string.IsNullOrEmpty(model.Id))
        {
            // Create
            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.NewPassword!);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Administrator");
                return RedirectToAction("Users");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }
        else
        {
            // Edit
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.Email = model.Email;
            user.UserName = model.Email;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.NewPassword!);

                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    return View(model);
                }
            }

            return RedirectToAction("Users");
        }

        return View(model); // In case of creation failure
    }
    // GET: Admin/ManageSiteSettings
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> ManageSiteSettings()
    {
        var settings = await _context.SiteSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new SiteSettings();
        }

        ViewBag.SocialMediaLinks = await _context.SocialMediaLinks
            .Include(x => x.SocialMediaType)
            .OrderBy(x => x.SocialMediaType.Name)
            .ToListAsync();

        return View(settings);
    }
    // GET: Admin/EditSocialLink
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> EditSocialLink(int id)
    {
        var link = await _context.SocialMediaLinks
            .Include(x => x.SocialMediaType)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (link == null)
        {
            return NotFound();
        }

        ViewBag.SocialMediaTypes = new SelectList(_context.SocialMediaTypes.OrderBy(x => x.Name), "Id", "Name", link.SocialMediaTypeId);
        return View("AddSocialLink", link); // Reuse the AddSocialLink.cshtml view
    }
    // POST: Admin/EditSocialLink
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> EditSocialLink(SocialMediaLink link)
    {
        if (ModelState.IsValid)
        {
            _context.SocialMediaLinks.Update(link);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageSiteSettings));
        }

        ViewBag.SocialMediaTypes = new SelectList(_context.SocialMediaTypes.OrderBy(x => x.Name), "Id", "Name", link.SocialMediaTypeId);
        return View("AddSocialLink", link);
    }

    // GET: Admin/DeleteSocialLink
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteSocialLink(int id)
    {
        var link = await _context.SocialMediaLinks.FindAsync(id);
        if (link == null)
        {
            return NotFound();
        }

        _context.SocialMediaLinks.Remove(link);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(ManageSiteSettings));
    }


    // POST: Admin/ManageSiteSettings
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManageSiteSettings(
    IFormFile? IconFile,
    IFormFile? BackgroundFile,
    string? IconUrl,
    string? BackgroundImageUrl,
    string BackgroundImageAlignment,
    bool BackgroundImageTiled,
    string BackgroundColor,
    string FontColor,
    string CardBackgroundColor) // 👈 add this
    {
        var existing = await _context.SiteSettings.FirstOrDefaultAsync();
        if (existing == null)
        {
            return NotFound();
        }

        // Upload new Icon if provided
        if (IconFile != null && IconFile.Length > 0)
        {
            var uploadedIconPath = await UploadHelper.UploadAndMoveAsync(IconFile, "icons");
            if (!string.IsNullOrEmpty(uploadedIconPath))
            {
                existing.IconUrl = uploadedIconPath;
            }
        }
        else
        {
            existing.IconUrl = IconUrl ?? "";
        }

        // Upload new Background if provided
        if (BackgroundFile != null && BackgroundFile.Length > 0)
        {
            var uploadedBackgroundPath = await UploadHelper.UploadAndMoveAsync(BackgroundFile, "backgrounds");
            if (!string.IsNullOrEmpty(uploadedBackgroundPath))
            {
                existing.BackgroundImageUrl = uploadedBackgroundPath;
            }
        }
        else
        {
            existing.BackgroundImageUrl = BackgroundImageUrl ?? "";
        }

        // Update other settings
        existing.BackgroundImageAlignment = BackgroundImageAlignment;
        existing.BackgroundImageTiled = BackgroundImageTiled;
        existing.BackgroundColor = BackgroundColor;
        existing.FontColor = FontColor;
        existing.LastUpdated = DateTime.UtcNow;
        existing.CardBackgroundColor = CardBackgroundColor;
        // Save changes to the database
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(ManageSiteSettings));
    }




    // GET: Admin/AddSocialLink
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public IActionResult AddSocialLink()
    {
        ViewBag.SocialMediaTypes = new SelectList(_context.SocialMediaTypes.OrderBy(x => x.Name), "Id", "Name");
        return View(new SocialMediaLink());
    }

    // POST: Admin/AddSocialLink
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> AddSocialLink(SocialMediaLink link)
    {
        if (ModelState.IsValid)
        {
            _context.SocialMediaLinks.Add(link);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageSiteSettings));
        }

        // If validation fails, reload social media types and redisplay form
        ViewBag.SocialMediaTypes = new SelectList(_context.SocialMediaTypes.OrderBy(x => x.Name), "Id", "Name");
        return View(link);
    }


}
