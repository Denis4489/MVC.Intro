namespace MVC.Intro.Services
{
    public class ImageService
    {
        private readonly string _imagesPath;

        public ImageService(IWebHostEnvironment env)
        {
            _imagesPath = Path.Combine(env.WebRootPath, "images");
        }

        public List<string> GetAvailableImages()
        {
            if (!Directory.Exists(_imagesPath))
            {
                return new List<string>();
            }

            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var files = Directory.GetFiles(_imagesPath)
                .Where(f => imageExtensions.Contains(Path.GetExtension(f).ToLower()))
                .Select(f => $"/images/{Path.GetFileName(f)}")
                .OrderBy(f => f)
                .ToList();

            return files;
        }
    }
}
