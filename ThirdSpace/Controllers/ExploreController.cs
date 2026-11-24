
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThirdSpace.Data;
using ThirdSpace.Models;
using HomePost = ThirdSpace.Models.Home.Post;

namespace ThirdSpace.Controllers
{
    [Authorize]
    public class ExploreController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ExploreController(
            ApplicationDbContext context,
            UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Set<HomePost>()
                .Include(p => p.User)
                .Where(p => p.ImagePath != null);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();

                query = query.Where(p =>
                    p.Content.Contains(term));
            }

            var posts = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        [HttpGet]
        public async Task<IActionResult> DiscoverPeople()
        {
            var userId = _userManager.GetUserId(User);

            var users = await _userManager.Users
                .Where(u => u.Id != userId)
                .OrderBy(u => u.UserName)
                .ToListAsync();

            var requests = await _context.Set<FriendRequest>()
                .Where(r =>
                    r.SenderId == userId ||
                    r.ReceiverId == userId)
                .ToListAsync();

            ViewBag.FriendRequests = requests;

            return View(users);
        }

        // AJAX: SEND FRIEND REQUEST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendRequest(
            string receiverId)
        {
            var userId = _userManager.GetUserId(User);

            bool isAjax =
                Request.Headers["X-Requested-With"].ToString()
                == "XMLHttpRequest";

            if (string.IsNullOrWhiteSpace(receiverId) ||
                userId == null ||
                userId == receiverId)
            {
                if (isAjax)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid friend request."
                    });
                }

                return BadRequest();
            }

            var receiver = await _userManager
                .FindByIdAsync(receiverId);

            if (receiver == null)
            {
                if (isAjax)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "User not found."
                    });
                }

                return NotFound();
            }

            var existingRequest =
                await _context.Set<FriendRequest>()
                    .AnyAsync(r =>
                        (r.SenderId == userId &&
                         r.ReceiverId == receiverId) ||
                        (r.SenderId == receiverId &&
                         r.ReceiverId == userId));

            if (existingRequest)
            {
                const string message =
                    "A friend request or connection already exists.";

                if (isAjax)
                {
                    return Conflict(new
                    {
                        success = false,
                        message = message
                    });
                }

                TempData["Error"] = message;

                return RedirectToAction(
                    nameof(DiscoverPeople));
            }

            var friendRequest = new FriendRequest
            {
                SenderId = userId,
                ReceiverId = receiverId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Set<FriendRequest>().Add(friendRequest);

            await _context.SaveChangesAsync();

            if (isAjax)
            {
                return Json(new
                {
                    success = true,
                    status = "Pending",
                    message = "Friend request sent successfully."
                });
            }

            TempData["Success"] =
                "Friend request sent successfully.";

            return RedirectToAction(
                nameof(DiscoverPeople));
        }

        // INCOMING FRIEND REQUESTS
        [HttpGet]
        public async Task<IActionResult> FriendRequests()
        {
            var userId = _userManager.GetUserId(User);

            var requests = await _context.Set<FriendRequest>()
                .Include(r => r.Sender)
                .Where(r =>
                    r.ReceiverId == userId &&
                    r.Status == "Pending")
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(requests);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptRequest(int id)
        {
            var userId = _userManager.GetUserId(User);

            bool isAjax = Request.Headers["X-Requested-With"]
                .ToString() == "XMLHttpRequest";

            var request = await _context.Set<FriendRequest>()
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    r.ReceiverId == userId &&
                    r.Status == "Pending");

            if (request == null)
            {
                if (isAjax)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Friend request not found."
                    });
                }

                return NotFound();
            }

            request.Status = "Accepted";

            await _context.SaveChangesAsync();

            if (isAjax)
            {
                return Json(new
                {
                    success = true,
                    status = "Accepted",
                    message = "Friend request accepted."
                });
            }

            return RedirectToAction(nameof(FriendRequests));
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectRequest(int id)
        {
            var userId = _userManager.GetUserId(User);

            bool isAjax = Request.Headers["X-Requested-With"]
                .ToString() == "XMLHttpRequest";

            var request = await _context.Set<FriendRequest>()
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    r.ReceiverId == userId &&
                    r.Status == "Pending");

            if (request == null)
            {
                if (isAjax)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Friend request not found."
                    });
                }

                return NotFound();
            }

            request.Status = "Rejected";

            await _context.SaveChangesAsync();

            if (isAjax)
            {
                return Json(new
                {
                    success = true,
                    status = "Rejected",
                    message = "Friend request rejected."
                });
            }

            return RedirectToAction(nameof(FriendRequests));
        }

    }
}
