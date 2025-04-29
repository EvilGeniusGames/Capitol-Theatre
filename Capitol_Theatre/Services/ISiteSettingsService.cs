using Capitol_Theatre.Data;

namespace Capitol_Theatre.Services
{
    public interface ISiteSettingsService
    {
        SiteSettings GetSettings();
        void InvalidateCache();
    }
}