using ThirdSpace.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThirdSpace.Data;
using ThirdSpace.Models;
using ThirdSpace.Models.Marketplace;
using ThirdSpace.ViewModels;

namespace ThirdSpace.Controllers
{
    [Authorize]
    public class MarketplaceController : Controller
    {
        private readonly IListingRepository _listingRepository;
        private readonly IChatRepository _chatRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _users;
        private readonly IWebHostEnvironment _environment;


        public MarketplaceController(
            ApplicationDbContext context,
            UserManager<User> users,
            IWebHostEnvironment environment,
            IListingRepository listingRepository,
            IChatRepository chatRepository)
        {
            _context = context;
            _users = users;
            _environment = environment;
            _listingRepository = listingRepository;
            _chatRepository = chatRepository;
        }



        public async Task<IActionResult> Index()
        {
            var listings = await _listingRepository.GetAllAsync();

            return View(listings);
        }


        [HttpGet]
        public async Task<IActionResult> SearchListings(
            string? query,
            string? category,
            decimal? minPrice,
            decimal? maxPrice)
        {
            ViewBag.Query = query;
            ViewBag.Category = category;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            bool isAjax = Request.Headers["X-Requested-With"]
                .ToString() == "XMLHttpRequest";

            if (minPrice < 0 || maxPrice < 0 ||
                (minPrice.HasValue &&
                 maxPrice.HasValue &&
                 minPrice.Value > maxPrice.Value))
            {
                const string errorMessage =
                    "Please enter a valid price range.";

                if (isAjax)
                {
                    return BadRequest(errorMessage);
                }

                ModelState.AddModelError(
                    string.Empty,
                    errorMessage);

                return View(new List<Listing>());
            }

            var listings = await _listingRepository.SearchAsync(
                query,
                category,
                minPrice,
                maxPrice);

            if (isAjax)
            {
                return PartialView(
                    "_SearchListingResults",
                    listings);
            }

            return View(listings);
        }


        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var listing = await _context.Listings
                .AsNoTracking()
                .Include(l => l.Seller)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (listing == null)
                return NotFound();

            return View(listing);
        }

        [HttpGet]
        public IActionResult CreateListings()
        {
            return View(new MarketplaceListingInput());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateListings(
            MarketplaceListingInput input)
        {
            ValidatePhoto(input.Photo);

            if (!ModelState.IsValid)
                return View(input);

            var listing = new Listing
            {
                Title = input.Title.Trim(),
                Description = input.Description.Trim(),
                Price = input.Price,
                Category = input.Category.Trim(),
                Condition = input.Condition.Trim(),
                Location = input.Location.Trim(),
                InStock = true,
                SellerId = _users.GetUserId(User)!,
                CreatedAt = DateTime.UtcNow
            };

            listing.ImagePath = await SavePhoto(input.Photo);

            _context.Listings.Add(listing);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Listing published successfully.";

            return RedirectToAction(nameof(YourListings));
        }

        [HttpGet]
        public async Task<IActionResult> YourListings()
        {
            string userId = _users.GetUserId(User)!;

            var listings = await _context.Listings
                .AsNoTracking()
                .Where(l => l.SellerId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            return View(listings);
        }

        [HttpGet]
        public async Task<IActionResult> EditListing(int id)
        {
            var listing = await OwnedListing(id);

            if (listing == null)
                return NotFound();

            return View(new MarketplaceListingInput
            {
                Id = listing.Id,
                Title = listing.Title,
                Description = listing.Description,
                Price = listing.Price,
                Category = listing.Category,
                Condition = listing.Condition,
                Location = listing.Location,
                InStock = listing.InStock,
                ExistingImagePath = listing.ImagePath
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditListing(
            int id,
            MarketplaceListingInput input)
        {
            if (id != input.Id)
                return BadRequest();

            var listing = await OwnedListing(id);

            if (listing == null)
                return NotFound();

            ValidatePhoto(input.Photo);

            if (!ModelState.IsValid)
            {
                input.ExistingImagePath = listing.ImagePath;
                return View(input);
            }

            listing.Title = input.Title.Trim();
            listing.Description = input.Description.Trim();
            listing.Price = input.Price;
            listing.Category = input.Category.Trim();
            listing.Condition = input.Condition.Trim();
            listing.Location = input.Location.Trim();
            listing.InStock = input.InStock;

            if (input.Photo != null)
            {
                listing.ImagePath =
                    await SavePhoto(input.Photo);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Listing updated.";

            return RedirectToAction(nameof(YourListings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetAvailability(
            int id,
            bool inStock)
        {
            var listing = await OwnedListing(id);

            if (listing == null)
                return NotFound();

            listing.InStock = inStock;

            await _context.SaveChangesAsync();

            TempData["Success"] = inStock
                ? "Listing is available again."
                : "Listing marked as sold.";

            return RedirectToAction(nameof(YourListings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteListing(int id)
        {
            var listing = await OwnedListing(id);

            if (listing == null)
                return NotFound();

            await using var tx =
                await _context.Database.BeginTransactionAsync();

            var chats = await _context.Chats
                .Where(c => c.ListingId == id)
                .ToListAsync();

            var chatIds = chats
                .Select(c => c.Id)
                .ToList();

            var messages = await _context.ChatMessages
                .Where(m => chatIds.Contains(m.ChatId))
                .ToListAsync();

            _context.ChatMessages.RemoveRange(messages);
            _context.Chats.RemoveRange(chats);
            _context.Listings.Remove(listing);

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            TempData["Success"] =
                "Listing and its related conversations deleted.";

            return RedirectToAction(nameof(YourListings));
        }


        [HttpGet]
        public async Task<IActionResult> Chats(int? id)
        {
            var userId = _users.GetUserId(User)!;

            var chats = await _chatRepository.GetUserChatsAsync(userId);

            var selected = id.HasValue
                ? chats.FirstOrDefault(c => c.Id == id.Value)
                : chats.FirstOrDefault();

            if (id.HasValue && selected == null)
                return NotFound();

            ViewBag.SelectedChatId = selected?.Id;

            ViewBag.Messages = selected == null
                ? new List<ChatMessage>()
                : await _chatRepository.GetMessagesAsync(selected.Id);

            ViewBag.UserId = userId;

            return View(chats);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartChat(int listingId)
        {
            string userId = _users.GetUserId(User)!;

            var listing = await _context.Listings
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    l => l.Id == listingId);

            if (listing == null)
                return NotFound();

            if (listing.SellerId == userId)
            {
                return BadRequest(
                    "You cannot message yourself.");
            }

            var chat = await _context.Chats
                .FirstOrDefaultAsync(c =>
                    c.ListingId == listingId &&
                    c.BuyerId == userId &&
                    c.SellerId == listing.SellerId);

            if (chat == null)
            {
                chat = new Chat
                {
                    ListingId = listingId,
                    BuyerId = userId,
                    SellerId = listing.SellerId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Chats.Add(chat);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(
                nameof(Chats),
                new { id = chat.Id });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(
            int chatId,
            string? content)
        {
            var userId = _users.GetUserId(User)!;

            var chat = await _chatRepository.GetByIdAsync(chatId, userId);

            if (chat == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(content) ||
                content.Trim().Length > 2000)
            {
                TempData["Error"] =
                    "Message must contain 1–2000 characters.";

                return RedirectToAction(
                    nameof(Chats),
                    new { id = chatId });
            }

            var message = new ChatMessage
            {
                ChatId = chatId,
                SenderId = userId,
                Content = content.Trim(),
                SentAt = DateTime.UtcNow
            };

            await _chatRepository.AddMessageAsync(message);

            return RedirectToAction(
                nameof(Chats),
                new { id = chatId });
        }


        private Task<Listing?> OwnedListing(int id)
        {
            var userId = _users.GetUserId(User)!;

            return _context.Listings
                .FirstOrDefaultAsync(l =>
                    l.Id == id &&
                    l.SellerId == userId);
        }

        private void ValidatePhoto(IFormFile? photo)
        {
            if (photo == null)
                return;

            if (photo.Length == 0 ||
                photo.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    nameof(MarketplaceListingInput.Photo),
                    "Image must be 1 byte to 5 MB.");
            }

            var extension = Path
                .GetExtension(photo.FileName)
                .ToLowerInvariant();

            if (extension != ".jpg" &&
                extension != ".jpeg" &&
                extension != ".png" &&
                extension != ".webp")
            {
                ModelState.AddModelError(
                    nameof(MarketplaceListingInput.Photo),
                    "Use JPG, PNG, or WEBP images.");
            }

            if (photo.ContentType != "image/jpeg" &&
                photo.ContentType != "image/png" &&
                photo.ContentType != "image/webp")
            {
                ModelState.AddModelError(
                    nameof(MarketplaceListingInput.Photo),
                    "Invalid image content type.");
            }
        }

        private async Task<string?> SavePhoto(IFormFile? photo)
        {
            if (photo == null)
                return null;

            var root = _environment.WebRootPath ??
                Path.Combine(
                    _environment.ContentRootPath,
                    "wwwroot");

            var directory = Path.Combine(
                root,
                "uploads",
                "marketplace");

            Directory.CreateDirectory(directory);

            var extension = Path
                .GetExtension(photo.FileName)
                .ToLowerInvariant();

            var fileName =
                $"{Guid.NewGuid():N}{extension}";

            var fullPath = Path.Combine(
                directory,
                fileName);

            await using var stream =
                System.IO.File.Create(fullPath);

            await photo.CopyToAsync(stream);

            return $"~/uploads/marketplace/{fileName}";
        }
    }
}
