using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Capitol_Theatre.Data;
using System;

namespace Capitol_Theatre.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("/Movies/Listing/{mode}")]
        public IActionResult Listing(string mode)
        {
            if (string.IsNullOrWhiteSpace(mode))
                return NotFound();

            var today = DateTime.Today;
            DateTime? start = null;
            DateTime? end = null;
            string title;

            switch (mode.ToLowerInvariant())
            {
                case "nowshowing":
                    start = today.AddDays(-((int)today.DayOfWeek + 2) % 7); // Previous Friday
                    end = start.Value.AddDays(6); // Following Thursday
                    title = "Now Showing";
                    break;

                case "nextweek":
                    start = today.AddDays(7 - ((int)today.DayOfWeek + 2) % 7); // Next Friday
                    end = start.Value.AddDays(6);
                    title = "Next Week";
                    break;

                case "comingsoon":
                    start = today.AddDays(14 - ((int)today.DayOfWeek + 2) % 7); // Two Fridays ahead
                    title = "Coming Soon";
                    break;

                default:
                    return NotFound();
            }

            IQueryable<Movie> query = _context.Movies
                .Include(m => m.Rating)
                .Include(m => m.Showtimes);

            if (mode.Equals("comingsoon", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(m => m.StartShowingDate.HasValue && m.StartShowingDate > start);
            }
            else
            {
                query = query.Where(m =>
                    (!m.StartShowingDate.HasValue || m.StartShowingDate <= end) &&
                    (!m.EndShowingDate.HasValue || m.EndShowingDate >= start));
            }

            var movies = query.ToList();

            ViewData["Title"] = title;
            ViewData["Mode"] = mode;

            return View("Movies", movies);
        }
    }
}
