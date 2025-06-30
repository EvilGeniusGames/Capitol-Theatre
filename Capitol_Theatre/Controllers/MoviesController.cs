using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Capitol_Theatre.Data;
using System;
using System.Linq;

namespace Capitol_Theatre.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("/Movies/Listing")]
        public IActionResult Listing(string mode)
        {
            if (string.IsNullOrWhiteSpace(mode)) return NotFound();

            var today = DateTime.Today;
            DateOnly startDate, endDate = DateOnly.MaxValue;
            string title;

            switch (mode.ToLowerInvariant())
            {
                case "nowshowing":
                    startDate = DateOnly.FromDateTime(today.AddDays(-((int)today.DayOfWeek + 2) % 7));
                    endDate = startDate.AddDays(6);
                    title = "Now Showing";
                    break;

                case "nextweek":
                    startDate = DateOnly.FromDateTime(today.AddDays(7 - ((int)today.DayOfWeek + 2) % 7));
                    endDate = startDate.AddDays(6);
                    title = "Next Week";
                    break;

                case "comingsoon":
                    startDate = DateOnly.FromDateTime(today.AddDays(14 - ((int)today.DayOfWeek + 2) % 7));
                    title = "Coming Soon";
                    break;

                default:
                    return NotFound();
            }

            IQueryable<Movie> query = _context.Movies
                .Include(m => m.Rating)
                .Include(m => m.MovieShowDates)
                    .ThenInclude(d => d.Showtimes);

            if (mode.Equals("comingsoon", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(m =>
                    m.MovieShowDates.Any(d => d.ShowDate >= startDate));
            }
            else
            {
                query = query.Where(m =>
                    m.MovieShowDates.Any(d =>
                        d.ShowDate >= startDate && d.ShowDate <= endDate));
            }

            var movies = query.ToList();

            ViewData["Title"] = title;
            ViewData["Mode"] = mode;

            return View("Movies", movies);
        }
    }
}
