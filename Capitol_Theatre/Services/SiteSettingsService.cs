using Capitol_Theatre.Data;

namespace Capitol_Theatre.Services
{
    public class SiteSettingsService : ISiteSettingsService
    {
        private readonly ApplicationDbContext _context;
        private SiteSettings? _cachedSettings;

        public SiteSettingsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public SiteSettings GetSettings()
        {
            return _cachedSettings ??= _context.SiteSettings.FirstOrDefault() ?? new SiteSettings();
        }

        public void InvalidateCache()
        {
            _cachedSettings = null;
        }
    }
}
