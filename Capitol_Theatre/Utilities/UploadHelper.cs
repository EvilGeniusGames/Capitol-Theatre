using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Capitol_Theatre.Utilities
{
    public static class UploadHelper
    {
        private static readonly string TempUploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "temp");

        static UploadHelper()
        {
            // Ensure temp folder exists at startup
            if (!Directory.Exists(TempUploadFolder))
            {
                Directory.CreateDirectory(TempUploadFolder);
            }
        }

        public static async Task<string?> UploadAndMoveAsync(IFormFile file, string finalSubfolder)
        {
            if (file == null || file.Length == 0)
                return null;

            var fileName = Path.GetFileName(file.FileName);

            // Ensure temp folder exists
            if (!Directory.Exists(TempUploadFolder))
                Directory.CreateDirectory(TempUploadFolder);

            var tempPath = Path.Combine(TempUploadFolder, fileName);
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Ensure final folder exists
            var finalFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", finalSubfolder);
            if (!Directory.Exists(finalFolderPath))
            {
                Directory.CreateDirectory(finalFolderPath);
            }

            var finalFilePath = Path.Combine(finalFolderPath, fileName);

            // Move file to final location
            File.Move(tempPath, finalFilePath, overwrite: true);

            // Return relative path with lowercase "images" to match URL convention
            return $"/images/{finalSubfolder}/{fileName}";
        }
    }
}
