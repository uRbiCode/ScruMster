using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScruMster.Areas.Identity.Data;
using ScruMster.Data;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ScruMster.Controllers
{
    [Authorize(Roles = "Admin, Manager, User")]
    public class SprintsController : Controller
    {
        private readonly ScruMsterContext _context;
        private UserManager<ScruMsterUser> _userManager;

        public SprintsController(ScruMsterContext context, UserManager<ScruMsterUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Sprints
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (User.IsInRole("User"))
            {
                var scruMsterContext = _context.Sprints.Include(s => s.Team).Where(s => s.TeamID == currentUser.TeamID);
                return View(await scruMsterContext.ToListAsync());
            }
            else
            {
                var scruMsterContext = _context.Sprints.Include(s => s.Team);
                return View(await scruMsterContext.ToListAsync());
            }

        }

        // GET: Sprints/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // ViewBag.vbComment = _context.Comments.ToList();
            //ViewBag.vbSprint = _context.Sprints.ToList();

            /*        dynamic mymodel = new ExpandoObject();  
                    mymodel.Sprints = await _context.Sprints.Include(s => s.Team)
                            .FirstOrDefaultAsync(m => m.SprintID == id);
                    mymodel.Comments = await _context.Comments
                            .FirstOrDefaultAsync(m => m.SprintId == id);
            */
            var sprint = await _context.Sprints
                .Include(s => s.Team)
                .FirstOrDefaultAsync(m => m.SprintID == id);

            if (sprint == null)
            {
                return NotFound();
            }

            // DODANO
            ViewBag.allComments = _context.Comments.Where(m => m.SprintID == id);
            var allAuthors = new List<ScruMsterUser>();
            foreach (var comment in _context.Comments.Where(m => m.SprintID == id))
            {
                foreach (var user in _context.ScruMsterUsers)
                {
                    if (user.Id == comment.ScruMsterUserId)
                    {
                        allAuthors.Add(user);
                    }
                }

            }
            ViewBag.allAuthors = allAuthors;


            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser.TeamID == sprint.TeamID || User.IsInRole("Admin") || User.IsInRole("Manager"))
            {
                return View(sprint);
            }
            else
            {
                return NotFound();
            }

        }



        // GET: Sprints/Comments/5
        public async Task<IActionResult> Comments(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            /*
                        ViewBag.vbComment = _context.Comments.ToList();
                        ViewBag.vbSprint = _context.Sprints.ToList();

                               dynamic mymodel = new ExpandoObject();  
                                mymodel.Sprints = await _context.Sprints.Include(s => s.Team)
                                        .FirstOrDefaultAsync(m => m.SprintID == id);
                                mymodel.Comments = await _context.Comments
                                        .FirstOrDefaultAsync(m => m.SprintId == id);*/

            var sprint = await _context.Sprints
                .Include(s => s.Team)
                .FirstOrDefaultAsync(m => m.SprintID == id);

            var comment = await _context.Comments
               .Include(s => s.Sprint)
               .FirstOrDefaultAsync(m => m.SprintID == id);


            if (sprint == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // GET: Sprints/Create
        [Authorize(Roles = "Admin, Manager")]
        public IActionResult Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewData["TeamID"] = new SelectList(_context.Teams.Where(s => s.ownerID == userId), "TeamID", "Name");
            return View();
        }

        // POST: Sprints/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Create([Bind("SprintID,Name,Description,Deadline,IsDone,TeamID")] Sprint sprint)
        {
            var owner = await _userManager.GetUserAsync(User);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ModelState.IsValid)
            {
                sprint.TeamID = owner.TeamID;
                _context.Add(sprint);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TeamID"] = new SelectList(_context.Teams.Where(s => s.ownerID == userId), "TeamID", "Name");
            return View(sprint);
        }

        // GET: Sprints/Edit/5
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id == null)
            {
                return NotFound();
            }

            var sprint = await _context.Sprints.FindAsync(id);
            if (sprint == null)
            {
                return NotFound();
            }
            ViewData["TeamID"] = new SelectList(_context.Teams.Where(s => s.ownerID == userId), "TeamID", "Name");
            return View(sprint);
        }

        // POST: Sprints/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("SprintID,Name,Description,Deadline,IsDone,TeamID")] Sprint sprint)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id != sprint.SprintID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sprint);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SprintExists(sprint.SprintID))
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
            ViewData["TeamID"] = new SelectList(_context.Teams.Where(s => s.ownerID == userId), "TeamID", "Name"); ;
            return View(sprint);
        }

        // GET: Sprints/Delete/5
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sprint = await _context.Sprints
                .Include(s => s.Team)
                .FirstOrDefaultAsync(m => m.SprintID == id);
            if (sprint == null)
            {
                return NotFound();
            }

            return View(sprint);
        }

        // POST: Sprints/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sprint = await _context.Sprints.FindAsync(id);
            _context.Sprints.Remove(sprint);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SprintExists(int id)
        {
            return _context.Sprints.Any(e => e.SprintID == id);
        }
    }
}
