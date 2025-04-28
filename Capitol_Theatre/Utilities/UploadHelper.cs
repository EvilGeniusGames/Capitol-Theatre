using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Capitol_Theatre.Utilities
{
    public static class UploadHelper
    {
        private static readonly string TempUploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "TempUploads");

        static UploadHelper()
        {
            // Ensure TempUploads exists
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

            // Save to temp first
            var tempPath = Path.Combine(TempUploadFolder, fileName);
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Now move to final folder
            var finalFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", finalSubfolder);
            if (!Directory.Exists(finalFolderPath))
            {
                Directory.CreateDirectory(finalFolderPath);
            }

            var finalFilePath = Path.Combine(finalFolderPath, fileName);
            File.Move(tempPath, finalFilePath, overwrite: true);

            // Return relative path for database
            return $"/Images/{finalSubfolder}/{fileName}";
        }
    }
}
