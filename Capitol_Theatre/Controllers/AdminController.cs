using Capitol_Theatre.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;

[Authorize]
public class AdminController : Controller
{

    [BindProperty(SupportsGet = false)]
    [ValidateNever]
    public string? ShowtimeEntries { get; set; }

    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Administrator")]
    public IActionResult Index()
    {
        var pages = _context.PageContents.ToList();
        return View(pages);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> UploadImage(IFormFile image)
    {
        try
        {
            if (image == null || image.Length == 0)
                return BadRequest("No image uploaded.");

            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "upload");
            Directory.CreateDirectory(uploadFolder);

            var fileName = Path.GetFileName(image.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(uploadFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            var imageUrl = $"/images/upload/{uniqueFileName}";
            Console.WriteLine("Returning JSON for image: " + imageUrl);

            return Json(new { location = imageUrl });
        }
        catch (Exception ex)
        {
            Console.WriteLine("UploadImage error: " + ex.Message);
            return StatusCode(500, "Internal error: " + ex.Message);
        }
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public IActionResult Edit(int id)
    {
        var page = _context.PageContents.FirstOrDefault(p => p.Id == id);
        if (page == null) return NotFound();
        return View("Edit", page);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public IActionResult Edit(PageContent model)
    {
        if (!ModelState.IsValid) return View("Edit", model);

        var page = _context.PageContents.Find(model.Id);
        if (page == null) return NotFound();

        page.HtmlContent = model.HtmlContent ?? "";
        page.LastUpdated = DateTime.UtcNow;
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

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

    [Authorize(Roles = "Administrator")]
    [HttpGet]
    public IActionResult ManageMovies()
    {
        var movies = _context.Movies.Include(m => m.Rating).ToList();
        return View(movies);
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public IActionResult CreateMovie()
    {
        ViewBag.Ratings = new SelectList(_context.Ratings, "Id", "Code");

        var posterDir = Path.Combine("wwwroot", "images", "posters");
        ViewBag.Posters = Directory.Exists(posterDir)
            ? Directory.GetFiles(posterDir).Select(p => "/images/posters/" + Path.GetFileName(p)).ToList()
            : new List<string>();

        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> CreateMovie(Movie model, IFormFile posterImage)
    {
        try
        {
            ModelState.Remove(nameof(posterImage));
            model.Showtimes ??= new List<Showtime>();

            if (posterImage != null && posterImage.Length > 0)
            {
                var folderPath = Path.Combine("wwwroot", "images", "posters");
                Directory.CreateDirectory(folderPath);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(posterImage.FileName)}";
                var filePath = Path.Combine(folderPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await posterImage.CopyToAsync(stream);

                model.PosterPath = "/images/posters/" + fileName;
            }

            if (string.IsNullOrWhiteSpace(model.PosterPath))
            {
                ModelState.AddModelError("PosterPath", "Please upload or select a poster image.");
            }

            if (model.StartShowingDate.HasValue && model.EndShowingDate.HasValue)
            {
                model.RunLength = (model.EndShowingDate.Value - model.StartShowingDate.Value).Days;
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Ratings = new SelectList(_context.Ratings, "Id", "Code", model.RatingId);

                var posterDir = Path.Combine("wwwroot", "images", "posters");
                ViewBag.Posters = Directory.Exists(posterDir)
                    ? Directory.GetFiles(posterDir).Select(p => "/images/posters/" + Path.GetFileName(p)).ToList()
                    : new List<string>();

                return View(model);
            }

            _context.Movies.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("ManageMovies");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🔥 CreateMovie crashed: {ex.Message}");
            System.IO.File.AppendAllText("/app/log_crash.txt", $"{DateTime.UtcNow:u} - {ex}\n");

            throw; // Optional: rethrow for normal error propagation
        }
    }




    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public IActionResult EditMovie(int id)
    {
        var movie = _context.Movies
            .Include(m => m.Showtimes)
            .FirstOrDefault(m => m.Id == id);

        if (movie == null)
            return NotFound();

        ViewBag.Ratings = new SelectList(_context.Ratings, "Id", "Code", movie.RatingId);

        var posterDir = Path.Combine("wwwroot", "images", "posters");
        ViewBag.Posters = Directory.Exists(posterDir)
            ? Directory.GetFiles(posterDir).Select(p => "/images/posters/" + Path.GetFileName(p)).ToList()
            : new List<string>();

        return View(movie);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> EditMovie(Movie model, IFormFile posterImage)
    {
        ModelState.Remove("posterImage");
        ModelState.Remove(nameof(ShowtimeEntries)); // Optional: defensive

        var movie = _context.Movies.Include(m => m.Showtimes).FirstOrDefault(m => m.Id == model.Id);
        if (movie == null) return NotFound();

        if (posterImage != null && posterImage.Length > 0)
        {
            var folderPath = Path.Combine("wwwroot", "images", "posters");
            Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(posterImage.FileName)}";
            var filePath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await posterImage.CopyToAsync(stream);

            movie.PosterPath = "/images/posters/" + fileName;
        }
        else if (!string.IsNullOrEmpty(model.PosterPath))
        {
            movie.PosterPath = model.PosterPath;
        }

        if (string.IsNullOrWhiteSpace(movie.PosterPath))
        {
            ModelState.AddModelError("PosterPath", "Please upload or select a poster image.");
        }

        movie.Title = model.Title;
        movie.Description = model.Description;
        movie.RatingId = model.RatingId;
        movie.TrailerUrl = model.TrailerUrl;
        movie.runtime = model.runtime;
        movie.StartShowingDate = model.StartShowingDate;
        movie.EndShowingDate = model.EndShowingDate;

        if (model.StartShowingDate.HasValue && model.EndShowingDate.HasValue)
        {
            movie.RunLength = (model.EndShowingDate.Value - model.StartShowingDate.Value).Days;
        }

        // Parse showtimes from bound property
        var parsedShowtimes = new List<Showtime>();
        if (!string.IsNullOrEmpty(ShowtimeEntries) &&
            movie.StartShowingDate.HasValue && movie.EndShowingDate.HasValue)
        {
            var showtimeParts = ShowtimeEntries.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (var entry in showtimeParts)
            {
                var split = entry.Split('|');
                if (split.Length != 2) continue;

                if (Enum.TryParse<DayOfWeek>(split[0], out var day) &&
                    TimeSpan.TryParse(split[1], out var time))
                {
                    for (var d = movie.StartShowingDate.Value; d <= movie.EndShowingDate.Value; d = d.AddDays(1))
                    {
                        if (d.DayOfWeek == day)
                        {
                            parsedShowtimes.Add(new Showtime
                            {
                                MovieId = movie.Id,
                                StartTime = d.Date + time
                            });
                        }
                    }
                }
            }
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Ratings = new SelectList(_context.Ratings, "Id", "Code", model.RatingId);

            var posterDir = Path.Combine("wwwroot", "images", "posters");
            ViewBag.Posters = Directory.Exists(posterDir)
                ? Directory.GetFiles(posterDir).Select(p => "/images/posters/" + Path.GetFileName(p)).ToList()
                : new List<string>();

            return View(model);
        }

        if (movie.Showtimes != null && movie.Showtimes.Any())
        {
            _context.Showtimes.RemoveRange(movie.Showtimes);
        }

        movie.Showtimes = parsedShowtimes;

        await _context.SaveChangesAsync();
        return RedirectToAction("ManageMovies");
    }




    [Authorize(Roles = "Administrator")]
    [HttpGet]
    public IActionResult DeleteMovie(int id)
    {
        var movie = _context.Movies.FirstOrDefault(m => m.Id == id);
        if (movie == null) return NotFound();

        _context.Movies.Remove(movie);
        _context.SaveChanges();

        return RedirectToAction("ManageMovies");
    }

    // --- Notices Management ---

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Notices()
    {
        var notices = await _context.Notices.OrderByDescending(n => n.PostedAt).ToListAsync();
        return View(notices);
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public IActionResult CreateNotice()
    {
        return View();
    }

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

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> EditNotice(int id)
    {
        var notice = await _context.Notices.FindAsync(id);
        if (notice == null) return NotFound();
        return View(notice);
    }

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
}