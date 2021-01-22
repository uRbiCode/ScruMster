using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScruMster.Areas.Identity.Data;
using ScruMster.Data;
using System.Collections.Generic;
using System.Security.Claims;
using System.Globalization;
using Microsoft.AspNetCore.Identity;

namespace ScruMster.Controllers
{
    [Authorize(Roles = "Admin, Manager, User")]
    public class TeamsController : Controller
    {
        private readonly ScruMsterContext _context;
        private UserManager<ScruMsterUser> _userManager;

        public TeamsController(ScruMsterContext context, UserManager<ScruMsterUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Teams
        public async Task<IActionResult> Index()
        {
            return View(await _context.Teams.ToListAsync());
        }

        // GET: Teams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams.Include(t => t.Sprints)
                .FirstOrDefaultAsync(m => m.TeamID == id);
            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

        // GET: Teams/Create
        public IActionResult Create()
        {
            var team = new Team();
            team.ScruMsterUsers = new List<ScruMsterUser>();
            PopulateTeamScruMsterUser(team);
            return View();
        }

        // POST: Teams/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TeamID,Name")] Team team, string[] selectedScruMsterUsers)
        {
            if (selectedScruMsterUsers != null)
            {
                var owner = await _userManager.GetUserAsync(User);
                team.ownerID = owner.Id;
                team.owner = owner;
                team.ScruMsterUsers = new List<ScruMsterUser>();
                foreach (var user in selectedScruMsterUsers)
                {
                    var userToAdd = _context.ScruMsterUsers.Find(user);
                    team.ScruMsterUsers.Add(userToAdd);
                }
                foreach (var teammember in team.ScruMsterUsers)
                {
                    teammember.Assigned = true;
                }
            }
            if (ModelState.IsValid)
            {
                _context.Add(team);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(team);
        }

        // GET: Teams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                return NotFound();
            }
            return View(team);
        }

        // POST: Teams/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TeamID,Name")] Team team)
        {
            if (id != team.TeamID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(team);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeamExists(team.TeamID))
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
            return View(team);
        }

        // GET: Teams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams
                .FirstOrDefaultAsync(m => m.TeamID == id);
            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team != null)
            {
                var allScruMsterUsers = _context.ScruMsterUsers;

                foreach (var user in allScruMsterUsers)
                {
                    if (user.TeamID == id)
                    {
                        user.TeamID = null;
                        user.Assigned = false;
                    }                     
                }
            }
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TeamExists(int id)
        {
            return _context.Teams.Any(e => e.TeamID == id);
        }



        private void PopulateTeamScruMsterUser(Team team)
        {
            var allScruMsterUsers = _context.ScruMsterUsers;
            var availableUsers = new List<ScruMsterUser>();
            var teamScruMsterUsers = new HashSet<string>(team.ScruMsterUsers.Select(c => c.Id));
            var viewModel = new List<ScruMsterUser>();
            foreach (var user in allScruMsterUsers)
            {
                if (!user.Assigned)
                {
                    viewModel.Add(new ScruMsterUser
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        TeamID = user.TeamID,
                        Assigned = teamScruMsterUsers.Contains(user.Id)
                    });
                }
                    
            }
            ViewBag.TotalUsers = viewModel;
        }
    }
}
