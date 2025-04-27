using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Capitol_Theatre.Data;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Capitol_Theatre.Controllers
{
    [Authorize(Roles = "Administrator")]
    [Route("admin/[controller]")]
    public class CreateEditMovieController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CreateEditMovieController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(string mode, int? id)
        {
            if (string.IsNullOrEmpty(mode) || !(mode.Equals("Create", StringComparison.OrdinalIgnoreCase) || mode.Equals("Edit", StringComparison.OrdinalIgnoreCase)))
                return BadRequest("Invalid mode.");

            Movie model = new Movie();
            if (mode.Equals("Edit", StringComparison.OrdinalIgnoreCase))
            {
                if (!id.HasValue)
                    return BadRequest("Movie ID is required for edit mode.");

                model = _context.Movies.Include(m => m.Showtimes).FirstOrDefault(m => m.Id == id.Value);
                if (model == null)
                    return NotFound();
            }

            ViewBag.Mode = mode;
            ViewBag.Ratings = new SelectList(_context.Ratings, "Id", "Code", model.RatingId);
            var posterDir = Path.Combine("wwwroot", "images", "posters");
            ViewBag.Posters = Directory.Exists(posterDir)
                ? Directory.GetFiles(posterDir).Select(p => "/images/posters/" + Path.GetFileName(p)).ToList()
                : new List<string>();

            return View("CreateEditMovie", model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(string mode, Movie model, IFormFile posterImage, string? ShowtimeEntries)
        {
            if (string.IsNullOrEmpty(mode) || !(mode.Equals("Create", StringComparison.OrdinalIgnoreCase) || mode.Equals("Edit", StringComparison.OrdinalIgnoreCase)))
                return BadRequest("Invalid mode.");

            ModelState.Remove(nameof(posterImage));
            ModelState.Remove(nameof(ShowtimeEntries));

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
                ModelState.AddModelError("PosterPath", "Please upload or select a poster image.");

            if (!ModelState.IsValid)
            {
                ViewBag.Mode = mode;
                ViewBag.Ratings = new SelectList(_context.Ratings, "Id", "Code", model.RatingId);
                var posterDir = Path.Combine("wwwroot", "images", "posters");
                ViewBag.Posters = Directory.Exists(posterDir)
                    ? Directory.GetFiles(posterDir).Select(p => "/images/posters/" + Path.GetFileName(p)).ToList()
                    : new List<string>();

                return View("CreateEditMovie", model);
            }

            if (mode.Equals("Create", StringComparison.OrdinalIgnoreCase))
            {
                model.Showtimes ??= new List<Showtime>();

                if (model.StartShowingDate.HasValue && model.EndShowingDate.HasValue)
                    model.RunLength = (model.EndShowingDate.Value - model.StartShowingDate.Value).Days;

                _context.Movies.Add(model);
            }
            else if (mode.Equals("Edit", StringComparison.OrdinalIgnoreCase))
            {
                var movie = _context.Movies.Include(m => m.Showtimes).FirstOrDefault(m => m.Id == model.Id);
                if (movie == null) return NotFound();

                movie.Title = model.Title;
                movie.Description = model.Description;
                movie.RatingId = model.RatingId;
                movie.TrailerUrl = model.TrailerUrl;
                movie.runtime = model.runtime;
                movie.StartShowingDate = model.StartShowingDate;
                movie.EndShowingDate = model.EndShowingDate;
                movie.Warning = model.Warning;
                movie.WarningColor = model.WarningColor;

                if (model.StartShowingDate.HasValue && model.EndShowingDate.HasValue)
                    movie.RunLength = (model.EndShowingDate.Value - model.StartShowingDate.Value).Days;

                if (!string.IsNullOrEmpty(model.PosterPath))
                    movie.PosterPath = model.PosterPath;

                if (movie.Showtimes != null && movie.Showtimes.Any())
                    _context.Showtimes.RemoveRange(movie.Showtimes);

                var parsedShowtimes = new List<Showtime>();
                if (!string.IsNullOrEmpty(ShowtimeEntries) && movie.StartShowingDate.HasValue && movie.EndShowingDate.HasValue)
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

                movie.Showtimes = parsedShowtimes;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("ManageMovies", "Admin");
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("admin/CreateEditMovie/DeleteMovie")]
        public IActionResult DeleteMovie(int id)
        {
            var movie = _context.Movies.FirstOrDefault(m => m.Id == id);
            if (movie == null) return NotFound();

            _context.Movies.Remove(movie);
            _context.SaveChanges();

            return RedirectToAction("ManageMovies", "Admin");
        }


        [Authorize(Roles = "Administrator")]
        [HttpPost("admin/CreateEditMovie/DeleteMovieConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMovieConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            return RedirectToAction("ManageMovies", "Admin");
        }


    }
}
