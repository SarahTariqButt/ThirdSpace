
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThirdSpace.Data;
using ThirdSpace.Models;

namespace ThirdSpace.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public HomeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(
            string content,
            IFormFile? image)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Post content is required.";
                return RedirectToAction("Index");
            }

            if (content.Length > 2000)
            {
                TempData["Error"] = "Post cannot exceed 2000 characters.";
                return RedirectToAction("Index");
            }

            string? imagePath = null;

            if (image != null && image.Length > 0)
            {
                const long maxImageSize = 5 * 1024 * 1024;

                if (image.Length > maxImageSize)
                {
                    TempData["Error"] = "Image must be smaller than 5 MB.";
                    return RedirectToAction("Index");
                }

                var extension = Path.GetExtension(image.FileName)
                    .ToLowerInvariant();

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

                if (!allowedExtensions.Contains(extension))
                {
                    TempData["Error"] = "Only JPG and PNG images are allowed.";
                    return RedirectToAction("Index");
                }

                var uploadsFolder = Path.Combine(
                    _environment.WebRootPath, "uploads");

                Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString("N") + extension;
                var filePath = Path.Combine(uploadsFolder, fileName);

                await using (var stream = System.IO.File.Create(filePath))
                {
                    await image.CopyToAsync(stream);
                }

                imagePath = "/uploads/" + fileName;
            }

            var userId = _userManager.GetUserId(User);

            if (userId == null)
                return Challenge();

            var post = new Post
            {
                Content = content.Trim(),
                ImagePath = imagePath,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
