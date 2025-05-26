using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Capitol_Theatre.Data;
using Capitol_Theatre.Utilities;
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

        [HttpGet("admin/CreateEditMovie/DeleteMovie")]
        public IActionResult DeleteMovie(int id)
        {
            var movie = _context.Movies
                .Include(m => m.MovieShowDates)
                .ThenInclude(d => d.Showtimes)
                .FirstOrDefault(m => m.Id == id);

            if (movie == null) return NotFound();

            _context.Movies.Remove(movie);
            _context.SaveChanges();

            return RedirectToAction("ManageMovies", "Admin");
        }

        [HttpPost("admin/CreateEditMovie/DeleteMovieConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMovieConfirmed(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.MovieShowDates)
                .ThenInclude(d => d.Showtimes)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            return RedirectToAction("ManageMovies", "Admin");
        }

        [HttpGet]
        public IActionResult Index(string mode, int? id)
        {
            if (string.IsNullOrEmpty(mode) || !(mode.Equals("Create", StringComparison.OrdinalIgnoreCase) || mode.Equals("Edit", StringComparison.OrdinalIgnoreCase)))
                return BadRequest("Invalid mode.");

            Movie model = new Movie();

            if (mode.Equals("Edit", StringComparison.OrdinalIgnoreCase))
            {
                if (!id.HasValue) return BadRequest("Movie ID is required for edit mode.");

                model = _context.Movies
                    .Include(m => m.MovieShowDates)
                        .ThenInclude(d => d.Showtimes)
                    .FirstOrDefault(m => m.Id == id.Value);

                if (model == null) return NotFound();
            }

            ViewBag.Mode = mode;
            ViewBag.Ratings = new SelectList(_context.Ratings, "Id", "Code", model.RatingId);

            var posterDir = Path.Combine("wwwroot", "Images", "posters");
            ViewBag.Posters = Directory.Exists(posterDir)
                ? Directory.GetFiles(posterDir).Select(p => "/Images/posters/" + Path.GetFileName(p)).ToList()
                : new List<string>();

            return View("CreateEditMovie", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string mode, Movie model, [FromForm] string ShowDateTimeEntries)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Mode = mode;
                ViewBag.Ratings = new SelectList(_context.Ratings, "Id", "Code", model.RatingId);

                var posterDir = Path.Combine("wwwroot", "Images", "posters");
                ViewBag.Posters = Directory.Exists(posterDir)
                    ? Directory.GetFiles(posterDir).Select(p => "/Images/posters/" + Path.GetFileName(p)).ToList()
                    : new List<string>();

                return View("CreateEditMovie", model);
            }

            var parsedShowDates = ParseShowDateTimeEntries(ShowDateTimeEntries);

            // Calculate current Friday–Thursday range
            var today = DateOnly.FromDateTime(DateTime.Today);
            int daysToFriday = ((int)DayOfWeek.Friday - (int)today.DayOfWeek + 7) % 7;
            var thisFriday = today.AddDays(-((int)today.DayOfWeek + 2) % 7); // Friday of this week
            var nextThursday = thisFriday.AddDays(6);

            bool hasAnyDatesThisWeek = parsedShowDates.Any(d =>
                d.ShowDate >= thisFriday && d.ShowDate <= nextThursday);

            bool anyThisWeekDatesMissingShowtimes = parsedShowDates
            .Where(d => d.ShowDate >= thisFriday && d.ShowDate <= nextThursday)
            .Any(d => d.Showtimes == null || !d.Showtimes.Any());


            if (hasAnyDatesThisWeek && anyThisWeekDatesMissingShowtimes)
            {
                ModelState.AddModelError("", "Movies scheduled for this week must include at least one showtime.");

                model.MovieShowDates = parsedShowDates; // ✅ Restore parsed dates and times

                ViewBag.Mode = mode;
                ViewBag.Ratings = new SelectList(_context.Ratings, "Id", "Code", model.RatingId);

                var posterDir = Path.Combine("wwwroot", "Images", "posters");
                ViewBag.Posters = Directory.Exists(posterDir)
                    ? Directory.GetFiles(posterDir).Select(p => "/Images/posters/" + Path.GetFileName(p)).ToList()
                    : new List<string>();

                return View("CreateEditMovie", model); // 🔁 Send full model back to repopulate UI
            }



            if (mode.Equals("Create", StringComparison.OrdinalIgnoreCase))
            {
                model.MovieShowDates = parsedShowDates;
                _context.Movies.Add(model);
            }
            else if (mode.Equals("Edit", StringComparison.OrdinalIgnoreCase))
            {
                var movie = _context.Movies
                    .Include(m => m.MovieShowDates)
                        .ThenInclude(msd => msd.Showtimes)
                    .FirstOrDefault(m => m.Id == model.Id);

                if (movie == null) return NotFound();

                movie.Title = model.Title;
                movie.Description = model.Description;
                movie.RatingId = model.RatingId;
                movie.runtime = model.runtime;
                movie.Warning = model.Warning;
                movie.WarningColor = model.WarningColor;
                movie.TrailerUrl = model.TrailerUrl;
                if (!string.IsNullOrEmpty(model.PosterPath)) movie.PosterPath = model.PosterPath;

                _context.MovieShowDates.RemoveRange(movie.MovieShowDates);
                movie.MovieShowDates = parsedShowDates;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("ManageMovies", "Admin");
        }



        private List<MovieShowDate> ParseShowDateTimeEntries(string entries)
        {
            var result = new Dictionary<DateOnly, MovieShowDate>();

            if (string.IsNullOrWhiteSpace(entries)) return result.Values.ToList();

            var parts = entries.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var tokens = part.Split('|');
                if (tokens.Length == 0) continue;

                if (!DateOnly.TryParse(tokens[0], out var date)) continue;

                if (!result.TryGetValue(date, out var msd))
                {
                    msd = new MovieShowDate { ShowDate = date };
                    result[date] = msd;
                }

                if (tokens.Length == 2 && TimeOnly.TryParse(tokens[1], out var time))
                {
                    msd.Showtimes.Add(new Showtime { StartTime = time });
                }
            }

            return result.Values.ToList();
        }



    }
}
