
using ThirdSpace.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThirdSpace.Data;
using ThirdSpace.Models;
using ThirdSpace.Models.Communities;

namespace ThirdSpace.Controllers
{
    [Authorize]
    public class CommunitiesController : Controller
    {
        private readonly ICommunityRepository _communityRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public CommunitiesController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            ICommunityRepository communityRepository)
        {
            _context = context;
            _userManager = userManager;
            _communityRepository = communityRepository;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var joinedIds = await _context.Set<CommunityMember>()
                .Where(m => m.UserId == userId)
                .Select(m => m.CommunityId)
                .ToListAsync();

            var posts = await _context.Set<Post>()
                .Include(p => p.User)
                .Include(p => p.Community)
                .Where(p => joinedIds.Contains(p.CommunityId))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            await LoadSidebar(userId!);

            return View(posts);
        }

        public async Task<IActionResult> Details(int id)
        {
            var community = await _context.Set<Community>()
                .Include(c => c.Creator)
                .Include(c => c.Members)
                .Include(c => c.Posts)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (community == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            ViewBag.IsMember = community.Members
                .Any(m => m.UserId == userId);

            await LoadSidebar(userId!);

            return View(community);
        }

        [HttpGet]
        public async Task<IActionResult> CreatePost()
        {
            var userId = _userManager.GetUserId(User);

            var joinedCommunities = await _context.CommunityMembers
                .Where(m => m.UserId == userId)
                .Select(m => m.Community)
                .OrderBy(c => c.Name)
                .ToListAsync();

            await LoadSidebar(userId!);

            ViewBag.JoinedCommunities = joinedCommunities;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(
            int communityId,
            string title,
            string? body)
        {
            var userId = _userManager.GetUserId(User);

            var isMember = await _context.CommunityMembers
                .AnyAsync(m =>
                    m.CommunityId == communityId &&
                    m.UserId == userId);

            if (!isMember)
                return Forbid();

            if (string.IsNullOrWhiteSpace(title) ||
                title.Trim().Length > 200)
            {
                ModelState.AddModelError(
                    "title",
                    "Title must contain between 1 and 200 characters.");

                ViewBag.JoinedCommunities = await _context.CommunityMembers
                    .Where(m => m.UserId == userId)
                    .Select(m => m.Community)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                await LoadSidebar(userId!);

                return View();
            }

            var post = new Post
            {
                Title = title.Trim(),
                Body = body,
                CommunityId = communityId,
                UserId = userId!,
                CreatedAt = DateTime.UtcNow
            };

            _context.CommunityPosts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                new { id = communityId });
        }

        public async Task<IActionResult> MyPosts()
        {
            var userId = _userManager.GetUserId(User);

            var posts = await _context.Set<Post>()
                .Include(p => p.User)
                .Include(p => p.Community)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            await LoadSidebar(userId!);

            return View(posts);
        }

        // AJAX JOIN COMMUNITY
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int id)
        {
            var userId = _userManager.GetUserId(User);

            bool isAjax = Request.Headers["X-Requested-With"]
                .ToString() == "XMLHttpRequest";

            var communityExists = await _context.Set<Community>()
                .AnyAsync(c => c.Id == id);

            if (!communityExists)
            {
                if (isAjax)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Community not found."
                    });
                }

                return NotFound();
            }

            var alreadyJoined = await _context.Set<CommunityMember>()
                .AnyAsync(m =>
                    m.CommunityId == id &&
                    m.UserId == userId);

            if (!alreadyJoined)
            {
                var member = new CommunityMember
                {
                    CommunityId = id,
                    UserId = userId!,
                    JoinedAt = DateTime.UtcNow
                };

                _context.Set<CommunityMember>().Add(member);
                await _context.SaveChangesAsync();
            }

            if (isAjax)
            {
                var memberCount = await _context
                    .Set<CommunityMember>()
                    .CountAsync(m => m.CommunityId == id);

                return Json(new
                {
                    success = true,
                    isMember = true,
                    memberCount = memberCount,
                    message = "Successfully joined the community."
                });
            }

            return RedirectToAction("Details", new { id });
        }

        // AJAX LEAVE COMMUNITY
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Leave(int id)
        {
            var userId = _userManager.GetUserId(User);

            bool isAjax = Request.Headers["X-Requested-With"]
                .ToString() == "XMLHttpRequest";

            var communityExists = await _context.Set<Community>()
                .AnyAsync(c => c.Id == id);

            if (!communityExists)
            {
                if (isAjax)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Community not found."
                    });
                }

                return NotFound();
            }

            var member = await _context.Set<CommunityMember>()
                .FirstOrDefaultAsync(m =>
                    m.CommunityId == id &&
                    m.UserId == userId);

            if (member != null)
            {
                _context.Set<CommunityMember>().Remove(member);
                await _context.SaveChangesAsync();
            }

            if (isAjax)
            {
                var memberCount = await _context
                    .Set<CommunityMember>()
                    .CountAsync(m => m.CommunityId == id);

                return Json(new
                {
                    success = true,
                    isMember = false,
                    memberCount = memberCount,
                    message = "Successfully left the community."
                });
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCommunity(string name)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(name) ||
                name.Trim().Length > 100)
            {
                TempData["Error"] =
                    "Enter a community name (up to 100 characters).";

                return RedirectToAction("Index");
            }

            var trimmedName = name.Trim();

            var exists = await _context.Set<Community>()
                .AnyAsync(c => c.Name == trimmedName);

            if (exists)
            {
                TempData["Error"] =
                    "A community with this name already exists.";

                return RedirectToAction("Index");
            }

            var community = new Community
            {
                Name = trimmedName,
                CreatorId = userId!,
                CreatedAt = DateTime.UtcNow
            };

            _context.Set<Community>().Add(community);

            community.Members.Add(new CommunityMember
            {
                UserId = userId!,
                JoinedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                new { id = community.Id });
        }

        private async Task LoadSidebar(string userId)
        {
            ViewBag.MyCommunities =
                await _communityRepository
                    .GetJoinedCommunitiesAsync(userId);

            ViewBag.PopularCommunities =
                await _communityRepository
                    .GetPopularCommunitiesAsync();
        }
    }
}
