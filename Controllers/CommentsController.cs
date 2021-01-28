using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScruMster.Areas.Identity.Data;
using ScruMster.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
namespace ScruMster.Controllers
{
    [Authorize(Roles = "Admin, Manager, User")]
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
            var currentUser = await _userManager.GetUserAsync(User);
            var Comments = new List<Comment>();

            foreach (var sprint in _context.Sprints.Where(s => s.TeamID == _context.Teams.Where(t => t.TeamID == currentUser.TeamID).FirstOrDefault().TeamID))
            {
                foreach (var comment in _context.Comments.Where(c => c.SprintID == sprint.SprintID))
                {
                    Comments.Add(comment);
                }
            }

            ViewBag.comments = Comments;
            if (currentUser.Id == "AdminID") ViewBag.comments = _context.Comments;
            return View();
        }

        // GET: Comments/Details/5
        [Authorize(Roles = "Admin, Manager, User")]
        public async Task<IActionResult> Details(int? id)
        {
            var currentUser = await _userManager.GetUserAsync(User);


            if (currentUser.Id != "AdminID")
            {
                foreach (var commentCheck in _context.Comments.Where(c => c.CommentId == id))
                {
                    foreach (var sprintCheck in _context.Sprints.Where(s => s.TeamID != currentUser.TeamID)
                        .Where(s => s.SprintID == commentCheck.SprintID))
                    {
                        throw new Exception("You can't access this comment!");
                    }
                }
            }

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
            ViewData["SprintID"] = new SelectList(_context.Sprints.Where(s => s.TeamID == _context.ScruMsterUsers
            .Where(i => i.Id == User.FindFirstValue(ClaimTypes.NameIdentifier)).FirstOrDefault().TeamID), "SprintID", "Name");
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
            comment.ScruMsterUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            comment.Author = await _userManager.GetUserAsync(User);
            comment.AuthorName = comment.Author.FirstName + " " + comment.Author.LastName;

            if (ModelState.IsValid)
            {
                _context.Add(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            foreach (var s in _context.Sprints.Where(s => s.SprintID == comment.SprintID))
            {
                _context.Comments.Add(comment);
                s.Comments.Add(comment);
            }
            ViewData["SprintID"] = new SelectList(_context.Sprints.Where(s => s.TeamID == currentUser.TeamID), "SprintID", "Name", comment.SprintID);
            return View(comment);
        }
        // GET: Comments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (_context.Sprints.Where(s => s.SprintID == _context.Comments
            .Where(c => c.CommentId == id).FirstOrDefault().SprintID).Where(s => s.TeamID != currentUser.TeamID).Any())
            {
                throw new Exception("You can't access this comment!");
            }

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
