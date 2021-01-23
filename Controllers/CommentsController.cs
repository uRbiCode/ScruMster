using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScruMster.Areas.Identity.Data;
using ScruMster.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ScruMster.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ScruMsterContext _context;
        private UserManager<ScruMsterUser> _userManager;

        public CommentsController(ScruMsterContext context, UserManager<ScruMsterUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Comments
        public async Task<IActionResult> Index()
        {
            var scruMsterContext = _context.Comments.Include(c => c.Sprint);
            return View(await scruMsterContext.ToListAsync());
        }

        // GET: Comments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .Include(c => c.Sprint)
                .FirstOrDefaultAsync(m => m.CommentId == id);
            if (comment == null)
            {
                return NotFound();
            }
            ViewBag.tempUser = _userManager.Users.FirstOrDefault(s => s.Id == comment.ScruMsterUserId);
            return View(comment);
        }

        // GET: Comments/Create
        public IActionResult Create()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = new ScruMsterUser();
            foreach (var item in _context.ScruMsterUsers)
            {
                if (item.Id == currentUserId)
                {
                    user = item;
                }
            }
            ViewData["SprintID"] = new SelectList(_context.Sprints.Where(s => s.TeamID == user.TeamID), "SprintID", "SprintID");
            return View();
        }

        // POST: Comments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CommentId,AddTime,Text,SprintID")] Comment comment)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            /*            if (User.IsInRole("Admin"))
                        {
                            return View(await _context.Teams.ToListAsync());
                        }
                        foreach (var user in _context.ScruMsterUsers)
                        {
                            if (user == currentUser)
                            {
                                return View(await _context.Sprints.Where(s => s.TeamID == user.TeamID).ToListAsync());
                            }
                        }*/

            comment.ScruMsterUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            comment.Author = await _userManager.GetUserAsync(User);

            if (ModelState.IsValid)
            {
                _context.Add(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            foreach (var s in _context.Sprints)
            {
                if (s.SprintID == comment.SprintID)
                {
                    _context.Comments.Add(comment);
                    s.Comments.Add(comment);
                }
            }
            ViewData["SprintID"] = new SelectList(_context.Sprints.Where(s => s.TeamID == currentUser.TeamID), "SprintID", "Description", comment.SprintID);
            return View(comment);
        }

        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            ViewData["SprintID"] = new SelectList(_context.Sprints, "SprintID", "Description", comment.SprintID);
            return View(comment);
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CommentId,AddTime,Text,SprintID")] Comment comment)
        {
            if (id != comment.CommentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(comment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.CommentId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["SprintID"] = new SelectList(_context.Sprints, "SprintID", "Description", comment.SprintID);
            return View(comment);
        }

        // GET: Comments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .Include(c => c.Sprint)
                .FirstOrDefaultAsync(m => m.CommentId == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.CommentId == id);
        }
    }
}
