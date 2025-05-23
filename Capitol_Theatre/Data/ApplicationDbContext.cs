﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Capitol_Theatre.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Showtime> Showtimes { get; set; }
        public DbSet<RecurringShowtimeRule> RecurringShowtimeRules { get; set; }
        public DbSet<DayOfWeekRule> DayOfWeekRules { get; set; }
        public DbSet<Notice> Notices { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<PageContent> PageContents { get; set; }
        public DbSet<SiteSettings> SiteSettings { get; set; }
        public DbSet<SocialMediaLink> SocialMediaLinks { get; set; }
        public DbSet<SocialMediaType> SocialMediaTypes { get; set; } = null!;



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new IdentityRoleConfiguration());

            modelBuilder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable("AspNetRoles");
                entity.Property(r => r.Id).HasColumnType("TEXT");
                entity.Property(r => r.Name).HasColumnType("TEXT").HasMaxLength(256);
                entity.Property(r => r.NormalizedName).HasColumnType("TEXT").HasMaxLength(256);
                entity.Property(r => r.ConcurrencyStamp).HasColumnType("TEXT");
            });

            modelBuilder.Entity<IdentityUser>(entity =>
            {
                entity.ToTable("AspNetUsers");
                entity.Property(u => u.Id).HasColumnType("TEXT");
                entity.Property(u => u.UserName).HasColumnType("TEXT").HasMaxLength(256);
                entity.Property(u => u.NormalizedUserName).HasColumnType("TEXT").HasMaxLength(256);
                entity.Property(u => u.Email).HasColumnType("TEXT").HasMaxLength(256);
                entity.Property(u => u.NormalizedEmail).HasColumnType("TEXT").HasMaxLength(256);
                entity.Property(u => u.ConcurrencyStamp).HasColumnType("TEXT");
                entity.Property(u => u.SecurityStamp).HasColumnType("TEXT");
            });

            modelBuilder.Entity<PageContent>()
                .Property(p => p.HtmlContent)
                .HasColumnType("TEXT");

            modelBuilder.Entity<Movie>()
                .HasMany(m => m.Showtimes)
                .WithOne()
                .HasForeignKey(s => s.MovieId);

            modelBuilder.Entity<Movie>()
                .HasOne(m => m.Rating)
                .WithMany()
                .HasForeignKey(m => m.RatingId);

            modelBuilder.Entity<RecurringShowtimeRule>()
                .HasMany(r => r.Days)
                .WithOne()
                .HasForeignKey(d => d.RecurringShowtimeRuleId);

            modelBuilder.Entity<PageContent>().HasData(
                new PageContent { Id = 1, PageKey = "Gift Certificates", HtmlContent = "Coming soon...", LastUpdated = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc) },
                new PageContent { Id = 2, PageKey = "Tickets", HtmlContent = "Coming soon...", LastUpdated = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc) },
                new PageContent { Id = 3, PageKey = "Ratings", HtmlContent = "Coming soon...", LastUpdated = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc) },
                new PageContent { Id = 4, PageKey = "Contact Us", HtmlContent = "Coming soon...", LastUpdated = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc) },
                new PageContent { Id = 5, PageKey = "FAQ", HtmlContent = "Coming soon...", LastUpdated = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc) }
            );

            modelBuilder.Entity<Rating>().HasData(
                new Rating { Id = 1, Code = "G", Description = "Suitable viewing for all ages." },
                new Rating { Id = 2, Code = "PG", Description = "Parental guidance is advised. Theme or content may not be suitable for all children." },
                new Rating { Id = 3, Code = "14A", Description = "Suitable for viewing by persons 14 years of age or older. Persons under 14 must be accompanied by an adult. May contain violence, coarse language, and/or sexually suggestive scenes." },
                new Rating { Id = 4, Code = "18A", Description = "Suitable for viewing by persons 18 years of age or older. Persons 14 - 17 must be accompanied by an adult. No Admittance to persons under 14. May contain explicit violence, frequent coarse language, sexual activity and/or horror." },
                new Rating { Id = 5, Code = "R", Description = "Admittance restricted to persons 18 and older. Content not suitable for minors. Contains frequent sexual activity, brutal/graphic violence, intense horror and/or disturbing content." }
            );

            modelBuilder.Entity<SocialMediaType>().HasData(
                new SocialMediaType { Id = 1, Name = "Facebook", FontAwesomeClass = "fab fa-facebook-square" }, // ✅ has square
                new SocialMediaType { Id = 2, Name = "Instagram", FontAwesomeClass = "fab fa-instagram" },       // ❌ no square
                new SocialMediaType { Id = 3, Name = "YouTube", FontAwesomeClass = "fab fa-youtube-square" },    // ✅ has square
                new SocialMediaType { Id = 4, Name = "Twitter/X", FontAwesomeClass = "fab fa-twitter-square" },    // ✅ has square (replace x-twitter)
                new SocialMediaType { Id = 5, Name = "LinkedIn", FontAwesomeClass = "fab fa-linkedin" },          // ❌ square deprecated
                new SocialMediaType { Id = 6, Name = "TikTok", FontAwesomeClass = "fab fa-tiktok" },              // ❌ no square in FA6
                new SocialMediaType { Id = 7, Name = "Pinterest", FontAwesomeClass = "fab fa-pinterest-square" },// ✅ has square
                new SocialMediaType { Id = 8, Name = "Bluesky", FontAwesomeClass = "fas fa-globe" }               // 🌐 generic, no square
            );


            modelBuilder.Entity<SiteSettings>().HasData(
                new SiteSettings
                {
                    Id = 1,
                    IconUrl = "", // Empty at start
                    BackgroundImageUrl = "", // No background image initially
                    BackgroundImageAlignment = "left", // Default to left (matches your config)
                    BackgroundImageTiled = false, // Default to no tiling
                    BackgroundColor = "#ffffff", // White background
                    FontColor = "#000000", // Black text
                    CardBackgroundColor = "#ffffff", // White card background
                    LastUpdated = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc) // Default timestamp
                }
            );
        }
    }

    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        [ValidateNever]
        [DataType(DataType.Text)]
        public string? PosterPath { get; set; }
        public string Description { get; set; } = string.Empty;
        public int RatingId { get; set; }
        [BindNever]
        public Rating? Rating { get; set; } = null!;

        public string? Warning { get; set; } = string.Empty;
        public string? WarningColor { get; set; } = "warning";
        public ICollection<Showtime>? Showtimes { get; set; } = new List<Showtime>();
        public string? TrailerUrl { get; set; }
        public int? runtime { get; set; }
        public DateTime? StartShowingDate { get; set; }
        public DateTime? EndShowingDate { get; set; }
        public int? RunLength { get; set; }


        public string GetFormattedShowtimes()
        {
            if (Showtimes == null || !Showtimes.Any())
                return "No showtimes available.";

            var groups = Showtimes
                .GroupBy(s => s.StartTime.TimeOfDay)
                .OrderBy(g => g.Key)
                .ToList();

            List<string> eveningTimes = new();
            List<string> matineeTimes = new();

            foreach (var group in groups)
            {
                var days = group
                    .Select(s => s.StartTime.DayOfWeek)
                    .Distinct()
                    .OrderBy(d => ((int)d + 2) % 7) // Friday = 0, Thursday = 6
                    .ToList();

                string dayRange = FormatDayRange(days);
                string time = DateTime.Today.Add(group.Key).ToString("h:mm tt");

                if (group.Key < new TimeSpan(17, 0, 0))
                    matineeTimes.Add($"{dayRange} {time}");
                else
                    eveningTimes.Add($"{dayRange} {time}");
            }

            string result = "";
            if (eveningTimes.Any())
                result += "<strong>Showtimes:</strong> " + JoinTimes(eveningTimes) + "\n";
            if (matineeTimes.Any())
                result += "<strong>Matinées:</strong> " + JoinTimes(matineeTimes);

            return result.Trim();
        }

        private static string JoinTimes(List<string> times)
        {
            return times.Count switch
            {
                0 => "",
                1 => times[0],
                2 => string.Join(" and ", times),
                _ => string.Join(", ", times.Take(times.Count - 1)) + " and " + times.Last()
            };
        }

        private static readonly Dictionary<DayOfWeek, string> DayShortNames = new()
        {
            [DayOfWeek.Sunday] = "Sun",
            [DayOfWeek.Monday] = "Mon",
            [DayOfWeek.Tuesday] = "Tue",
            [DayOfWeek.Wednesday] = "Wed",
            [DayOfWeek.Thursday] = "Thu",
            [DayOfWeek.Friday] = "Fri",
            [DayOfWeek.Saturday] = "Sat"
        };

        private string FormatDayRange(List<DayOfWeek> days)
        {
            if (!days.Any()) return "";

            // Reorder so Friday is 0, Thursday is 6
            days = days.OrderBy(d => ((int)d + 2) % 7).ToList();

            List<string> ranges = new();
            int i = 0;

            while (i < days.Count)
            {
                var start = days[i];
                var end = start;

                while (i + 1 < days.Count &&
                       (((int)days[i + 1] - (int)end == 1) || ((int)end == 6 && (int)days[i + 1] == 0)))
                {
                    end = days[++i];
                }

                if (start == end)
                    ranges.Add(DayShortNames[start]);
                else
                    ranges.Add($"{DayShortNames[start]}–{DayShortNames[end]}");

                i++;
            }

            return string.Join(", ", ranges);
        }


        private static string FormatRange(DayOfWeek start, DayOfWeek end)
        {
            string FormatDay(DayOfWeek day) => day.ToString().Substring(0, 3);
            return start == end ? FormatDay(start) : $"{FormatDay(start)} – {FormatDay(end)}";
        }
    }

    public class Rating
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class Showtime
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public int MovieId { get; set; }
    }

    public class RecurringShowtimeRule
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;
        public TimeSpan TimeOfDay { get; set; }
        public ICollection<DayOfWeekRule> Days { get; set; } = new List<DayOfWeekRule>();
    }

    public class DayOfWeekRule
    {
        public int Id { get; set; }
        public DayOfWeek Day { get; set; }
        public int RecurringShowtimeRuleId { get; set; }
    }

    public class Notice
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime PostedAt { get; set; }
        public DateTime Expires { get; set; }
        public string Color { get; set; } = "info";

    }

    public class PageContent
    {
        public int Id { get; set; }
        public string PageKey { get; set; } = string.Empty;
        public string HtmlContent { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }

    public class SocialMediaType
    {
        public int Id { get; set; } // Primary Key
        public string Name { get; set; } = "";
        // Example: "Facebook", "Instagram", "YouTube"
        public string FontAwesomeClass { get; set; } = "";
        // Example: "fab fa-facebook", "fab fa-instagram"
    }

    public class SiteSettings
    {
        public int Id { get; set; }
        public string IconUrl { get; set; } = "";
        public string BackgroundImageUrl { get; set; } = "";
        public string BackgroundImageAlignment { get; set; } = "left";
        public bool BackgroundImageTiled { get; set; } = false;
        public string BackgroundColor { get; set; } = "#ffffff";
        public string FontColor { get; set; } = "#000000";
        public string CardBackgroundColor { get; set; } = "#ffffff";
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // ✅ Add this:
        public ICollection<SocialMediaLink> SocialMediaLinks { get; set; } = new List<SocialMediaLink>();
    }

    public class SocialMediaLink
    {
        public int Id { get; set; }
        public int SocialMediaTypeId { get; set; } // FK to lookup table
        public SocialMediaType? SocialMediaType { get; set; }
        public string Url { get; set; } = "";       // User-supplied link
        public string IconColor { get; set; } = ""; // Optional, e.g., "#4285F4", "red"
    }
}
