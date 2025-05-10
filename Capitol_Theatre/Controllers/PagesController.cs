// File: PagesController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Capitol_Theatre.Data;
using System.Linq;

[Authorize]
public class PagesController : Controller
{
    private readonly ApplicationDbContext _context;

    public PagesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Edit(int id)
    {
        var page = _context.PageContents.FirstOrDefault(p => p.Id == id);
        if (page == null) return NotFound();
        return View(page);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, PageContent updatedPage)
    {
        if (id != updatedPage.Id) return BadRequest();

        if (ModelState.IsValid)
        {
            var page = _context.PageContents.Find(id);
            if (page == null) return NotFound();

            page.HtmlContent = updatedPage.HtmlContent;
            page.LastUpdated = DateTime.UtcNow;

            _context.SaveChanges();
            return RedirectToAction("Index", "Admin");
        }

        return View(updatedPage);
    }

    [HttpGet("/Tickets")]
    [AllowAnonymous]
    public IActionResult Tickets()
    {
        var content = _context.PageContents.FirstOrDefault(p => p.PageKey == "Tickets");
        if (content == null) return NotFound();
        ViewData["Title"] = "Tickets/Hours";
        return View("~/Views/Pages/Render.cshtml", model: content);
    }

    [HttpGet("/Gift-Certificates")]
    [AllowAnonymous]
    public IActionResult GiftCertificates()
    {
        var content = _context.PageContents.FirstOrDefault(p => p.PageKey == "Gift Certificates");
        if (content == null) return NotFound();
        return View("~/Views/Pages/Render.cshtml", model: content);
    }

    [HttpGet("/Ratings")]
    [AllowAnonymous]
    public IActionResult Ratings()
    {
        var content = _context.PageContents.FirstOrDefault(p => p.PageKey == "Ratings");
        if (content == null) return NotFound();
        return View("~/Views/Pages/Render.cshtml", model: content);
    }

    [AllowAnonymous]
    public IActionResult ContactUs()
    {
        var content = _context.PageContents.FirstOrDefault(p => p.PageKey == "Contact Us");
        if (content == null) return NotFound();
        return View("~/Views/Pages/Render.cshtml", model: content);
    }


    [HttpGet("/FAQ")]
    [AllowAnonymous]
    public IActionResult FAQ()
    {
        var content = _context.PageContents.FirstOrDefault(p => p.PageKey == "FAQ");
        if (content == null) return NotFound();
        return View("~/Views/Pages/Render.cshtml", model: content);
    }
}
