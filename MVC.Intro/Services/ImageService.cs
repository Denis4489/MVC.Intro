namespace MVC.Intro.Services
{
    public class ImageService
    {
        private readonly string _imagesPath;

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        public ImageService(IWebHostEnvironment env)
        {
            _imagesPath = Path.Combine(env.WebRootPath, "images");
        }

        public List<string> GetAvailableImages()
        {
            if (!Directory.Exists(_imagesPath))
                return new List<string>();

            return Directory.GetFiles(_imagesPath)
                .Where(f => AllowedExtensions.Contains(Path.GetExtension(f).ToLower()))
                .Select(f => $"/images/{Path.GetFileName(f)}")
                .OrderBy(f => f)
                .ToList();
        }

        /// <summary>
        /// Saves an uploaded image to wwwroot/images.
        /// Returns the relative URL (/images/filename.ext) on success,
        /// or throws InvalidOperationException with a user-friendly message on failure.
        /// </summary>
        public async Task<string> SaveUploadedImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("Файлът е празен.");

            if (file.Length > MaxFileSizeBytes)
                throw new InvalidOperationException("Файлът е твърде голям. Максималният размер е 5 MB.");

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!AllowedExtensions.Contains(ext))
                throw new InvalidOperationException($"Неподдържан формат. Разрешени: {string.Join(", ", AllowedExtensions)}.");

            // Ensure the images folder exists
            Directory.CreateDirectory(_imagesPath);

            // Build a safe, unique filename to avoid collisions and path traversal
            var safeName = Path.GetFileNameWithoutExtension(file.FileName);
            safeName = string.Concat(safeName.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_'));
            if (string.IsNullOrEmpty(safeName)) safeName = "image";

            var uniqueName = $"{safeName}_{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(_imagesPath, uniqueName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/images/{uniqueName}";
        }
    }
}
