using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScruMster.Areas.Identity.Data;
using ScruMster.Data;
using System;
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
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;
            var sprints = from s in _context.Sprints
                          select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                sprints = sprints.Where(s => s.Name.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    sprints = sprints.OrderByDescending(s => s.Name);
                    break;
                case "Date":
                    sprints = sprints.OrderBy(s => s.Deadline);
                    break;
                case "date_desc":
                    sprints = sprints.OrderByDescending(s => s.Deadline);
                    break;
                default:
                    sprints = sprints.OrderBy(s => s.Name);
                    break;
            }


            var currentUser = await _userManager.GetUserAsync(User);
            if (User.IsInRole("User"))
            {
                sprints = sprints.Include(s => s.Team).Where(s => s.TeamID == currentUser.TeamID);
                //return View(await sprints.AsNoTracking().ToListAsync());
            }
            else
            {
                sprints = sprints.Include(s => s.Team);
                //return View(await sprints.AsNoTracking().ToListAsync());
            }
            int pageSize = 5;
            return View(await PaginatedList<Sprint>.CreateAsync(sprints.AsNoTracking(), pageNumber ?? 1, pageSize));

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
            ViewBag.allUsers = _context.ScruMsterUsers;

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
            /*            foreach (var item in _context.Sprints)
                        {
                            if (sprint.Name == item.Name)
                            {
                                throw new Exception("Sprint with that name already exist!");
                            }
                        }*/
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
            var currentUser = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin"))
            {
                sprint.TeamID = currentUser.TeamID;
            }

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
