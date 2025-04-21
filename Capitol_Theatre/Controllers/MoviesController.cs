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

        public IActionResult NowShowing()
        {
            var today = DateTime.Today;
            int daysToFriday = ((int)DayOfWeek.Friday - (int)today.DayOfWeek + 7) % 7;
            var start = today.AddDays(-((int)today.DayOfWeek + 2) % 7); // Previous Friday
            var end = start.AddDays(6); // Following Thursday

            var movies = _context.Movies
                .Include(m => m.Rating)
                .Include(m => m.Showtimes)
                .Where(m =>
                    (!m.StartShowingDate.HasValue || m.StartShowingDate.Value <= end) &&
                    (!m.EndShowingDate.HasValue || m.EndShowingDate.Value >= start))
                .ToList();

            return View(movies);
        }

        public IActionResult NextWeek()
        {
            var today = DateTime.Today;
            var start = today.AddDays(7 - ((int)today.DayOfWeek + 2) % 7); // Next Friday
            var end = start.AddDays(6);

            var movies = _context.Movies
                .Include(m => m.Rating)
                .Include(m => m.Showtimes)
                .Where(m =>
                    (!m.StartShowingDate.HasValue || m.StartShowingDate <= end) &&
                    (!m.EndShowingDate.HasValue || m.EndShowingDate >= start))
                .ToList();

            return View(movies);
        }


        public IActionResult ComingSoon()
        {
            var today = DateTime.Today;
            var nextFriday = today.AddDays(14 - ((int)today.DayOfWeek + 2) % 7);

            var movies = _context.Movies
                .Include(m => m.Rating)
                .Include(m => m.Showtimes)
                .Where(m => m.StartShowingDate.HasValue && m.StartShowingDate > nextFriday)
                .ToList();

            return View(movies);
        }
    }
}
