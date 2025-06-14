# 🎬 Capitol Theatre Website (Alpha)

A custom-built movie poster CMS and public website for the Capitol Theatre.

> 🚧 This project is currently in **Beta**. Features are actively evolving.

📖 **[Installation & Recovery Manual →](./RECOVERY.md)**

---

## ✨ Features

### 🎟️ Public Site
- **Now Showing, Next Week, Coming Soon**: Displays movies dynamically based on date range.
- **Trailer Integration**: Clickable posters open movie trailers in a new tab (if available).
- **Showtimes Rendering**: Intelligently formats evening and matinee showtimes per day.
- **Notice System**: Displays time-limited notices with Bootstrap alert styling.
- **Responsive Design**: Clean layout with Bootstrap 5 and mobile-first considerations.
- **Improved Poster Scaling**: Card and text scaling optimized for desktop and mobile.
- **Single-Click Navigation**: Poster reveals description; second click opens trailer.

### 🔐 Admin Interface
- **Movie Management**: Add, edit, delete movies with poster upload, showtime scheduling, and ratings.
- **Poster Handling**: Upload and select images with optional future folder support.
- **Notice Editor**: Manage public alerts with date range and color-coding.
- **Page Content Editing**: Markdown/HTML editor (TinyMCE) for static page content.
- **Authentication**: Admin access protected with ASP.NET Identity.
- **Site Settings**: Update site visuals like logo, background, and color.
- **Social Media Management**: Admin interface for editing and displaying icon links.
- **Backup Generator**: Create downloadable ZIP containing DB and image assets.

---

## ⚙️ Tech Stack

- **ASP.NET Core MVC** (.NET 8)
- **SQLite** database (Entity Framework Core)
- **Bootstrap 5** for layout and styling
- **TinyMCE** for content editing
- **Docker-ready** with containerization support
- **GitHub Container Registry** (GHCR) for production deployment

---

## 📦 Deployment (Alpha)

This image is available on GitHub Container Registry:

# License

This project is licensed under the [CC BY-NC 4.0 License](https://creativecommons.org/licenses/by-nc/4.0/).
