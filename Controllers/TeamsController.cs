using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScruMster.Areas.Identity.Data;
using ScruMster.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.ShowCreate = true;
            if (User.IsInRole("Admin"))
            {
                return View(await _context.Teams.ToListAsync());
            }


            var teams = await _context.Teams.Where(s => s.TeamID == currentUser.TeamID).ToListAsync();
            if (teams.Any())
            {
                if (teams.Where(t => t.ownerID == currentUser.Id).Any())
                {
                    ViewBag.ShowCreate = false;
                }
            }

            return View(teams);
        }

        // GET: Teams/Details/5
        [Authorize(Roles = "Admin, Manager, User")]
        public async Task<IActionResult> Details(int? id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
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
            ViewBag.teamLeader = _context.ScruMsterUsers.FirstOrDefault(m => m.Id == _context.Teams.FirstOrDefault(m => m.TeamID == id).ownerID);
            ViewBag.teamUsers = _context.ScruMsterUsers.Where(m => m.TeamID == id);
            if ((currentUser.TeamID != id || currentUser.TeamID == null) && currentUser.Id != "AdminID") throw new Exception("You can't access other teams data!");
            return View(team);
        }

        // GET: Teams/Create
        [Authorize(Roles = "Admin, Manager")]
        public IActionResult Create()
        {
            var currentUser = _userManager.GetUserId(User);


            if (_context.Teams.Where(t => t.ownerID == currentUser).Any() && currentUser != "AdminID")
            {
                throw new Exception("You already own a team!");
            }



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
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Create([Bind("TeamID,Name")] Team team, string[] selectedScruMsterUsers)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser.Id == "AdminID") throw new Exception("Section under development");
            if (selectedScruMsterUsers != null)
            {
                var owner = await _userManager.GetUserAsync(User);
                team.ownerID = owner.Id;
                team.owner = owner;
                team.ScruMsterUsers = new List<ScruMsterUser>();
                team.ScruMsterUsers.Add(owner); // owner restricted to 1 team


                foreach (var user in selectedScruMsterUsers)
                {
                    team.ScruMsterUsers.Add(_context.ScruMsterUsers.Find(user));
                }
                foreach (var teammember in team.ScruMsterUsers)
                {
                    teammember.Assigned = true;
                    teammember.Team = team;
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
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if ((currentUser.TeamID != id || currentUser.TeamID == null) && currentUser.Id != "AdminID") throw new Exception("You can't edit other teams data!");
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                return NotFound();
            }
            PopulateTeamScruMsterUserForEdit(team);

            return View(team);
        }

        // POST: Teams/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("TeamID,Name")] Team team, string[] selectedScruMsterUsers)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser.Id == "AdminID") throw new Exception("Section under development");
            if (currentUser.TeamID != id || currentUser.TeamID == null) throw new Exception("You can't edit other teams data!");
            if (id != team.TeamID)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var owner = await _userManager.GetUserAsync(User);
                    team.ownerID = owner.Id;
                    team.owner = owner;
                    team.ScruMsterUsers = new List<ScruMsterUser>();
                    foreach (var user in selectedScruMsterUsers)
                    {
                        team.ScruMsterUsers.Add(_context.ScruMsterUsers.Find(user));
                    }
                    foreach (var teammember in team.ScruMsterUsers)
                    {
                        teammember.Assigned = true;
                        teammember.Team = team;
                    }
                    foreach (var user in _context.ScruMsterUsers.Where(u => u.TeamID == team.TeamID))
                    {
                        if (!selectedScruMsterUsers.Contains(user.Id))
                        {
                            user.TeamID = null;
                            user.Assigned = false;
                            user.Team = null;
                        }
                    }
                    team.ownerID = owner.Id;
                    team.owner = owner;
                    owner.Assigned = true;
                    team.ScruMsterUsers.Add(owner);
                    ///
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
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if ((currentUser.TeamID != id || currentUser.TeamID == null) && currentUser.Id != "AdminID") throw new Exception("You can't delete other teams!");
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
            var viewModel = new List<ScruMsterUser>();

            ViewBag.TotalUsers = viewModel;
            return View(team);
        }
        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if ((currentUser.TeamID != id || currentUser.TeamID == null) && currentUser.Id != "AdminID") throw new Exception("You can't delete other teams!");
            var team = await _context.Teams.FindAsync(id);
            if (team != null)
            {

                foreach (var user in _context.ScruMsterUsers.Where(u => u.TeamID == id))
                {
                    user.TeamID = null;
                    user.Assigned = false;
                    user.Team = null;
                }
            }
            currentUser.TeamID = null;
            currentUser.Assigned = false;
            currentUser.Team = null;
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
            var adminList = new List<ScruMsterUser>();
            foreach (var userRole in _context.UserRoles.Where(u => u.RoleId == _context.Roles.Where(r => r.Name == "Admin")
            .FirstOrDefault().Id).Union(_context.UserRoles.Where(u => u.RoleId == _context.Roles.Where(r => r.Name == "Manager").FirstOrDefault().Id)))
            {
                foreach (var user in _context.ScruMsterUsers.Where(u => u.Id == userRole.UserId))
                    adminList.Add(user);
            }
            var teamScruMsterUsers = new HashSet<string>(team.ScruMsterUsers.Select(c => c.Id));
            var viewModel = new List<ScruMsterUser>();
            foreach (var user in _context.ScruMsterUsers.Where(u => !u.Assigned).Where(u => !adminList.Contains(u)))
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
            ViewBag.TotalUsers = viewModel;
        }

        private void PopulateTeamScruMsterUserForEdit(Team team)
        {
            var adminList = new List<ScruMsterUser>();
            foreach (var userRole in _context.UserRoles.Where(u => u.RoleId == _context.Roles.Where(r => r.Name == "Admin")
            .FirstOrDefault().Id).Union(_context.UserRoles.Where(u => u.RoleId == _context.Roles.Where(r => r.Name == "Manager").FirstOrDefault().Id)))
            {
                foreach (var user in _context.ScruMsterUsers.Where(u => u.Id == userRole.UserId))
                    adminList.Add(user);
            }
            var viewModel = new List<ScruMsterUser>();
            foreach (var user in _context.ScruMsterUsers.Where(u => !adminList.Contains(u)).Where(u => u.TeamID == team.TeamID)
                .Union(_context.ScruMsterUsers.Where(u => !adminList.Contains(u)).Where(u => !u.Assigned)))
            {
                viewModel.Add(new ScruMsterUser
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    TeamID = user.TeamID,
                    Assigned = (team.TeamID == user.TeamID)
                });
            }
            ViewBag.TotalUsers = viewModel;
        }
    }
}