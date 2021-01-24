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
            if (User.IsInRole("Admin"))
            {
                return View(await _context.Teams.ToListAsync());
            }
            foreach (var user in _context.ScruMsterUsers)
            {
                if (user == currentUser)
                {
                    return View(await _context.Teams.Where(s => s.TeamID == user.TeamID).ToListAsync());
                }
            }
            return View(await _context.Teams.ToListAsync());
        }

        // GET: Teams/Details/5
        [Authorize(Roles = "Admin, Manager, User")]
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
            ViewBag.teamLeader = _context.ScruMsterUsers.FirstOrDefault(m => m.Id == _context.Teams.FirstOrDefault(m => m.TeamID == id).ownerID);
            ViewBag.teamUsers = _context.ScruMsterUsers.Where(m => m.TeamID == id);
            return View(team);
        }

        // GET: Teams/Create
        [Authorize(Roles = "Admin, Manager")]
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
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Create([Bind("TeamID,Name")] Team team, string[] selectedScruMsterUsers)
        {
            if (selectedScruMsterUsers != null)
            {
                var owner = await _userManager.GetUserAsync(User);
                team.ownerID = owner.Id;
                team.owner = owner;
                team.ScruMsterUsers = new List<ScruMsterUser>();
                team.ScruMsterUsers.Add(owner); // owner restricted to 1 team
                foreach (var user in selectedScruMsterUsers)
                {
                    var userToAdd = _context.ScruMsterUsers.Find(user);
                    team.ScruMsterUsers.Add(userToAdd);
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
            if (id != team.TeamID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ///
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
                        teammember.Team = team;
                    }
                    foreach (var user in _context.ScruMsterUsers)
                    {
                        if (!selectedScruMsterUsers.Contains(user.Id) && user.TeamID == team.TeamID)
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
                        user.Team = null;
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
            var allUserRoles = _context.UserRoles;
            var allRoles = _context.Roles;
            string adminID = "";
            string managerID = "";
            var adminList = new List<ScruMsterUser>();
            foreach (var role in allRoles)
            {
                if (role.Name == "Admin")
                {
                    adminID = role.Id;
                    break;
                }
            }
            foreach (var role in allRoles)
            {
                if (role.Name == "Manager")
                {
                    managerID = role.Id;
                    break;
                }
            }
            foreach (var userRole in allUserRoles)
            {
                if ((string)userRole.RoleId == adminID || (string)userRole.RoleId == managerID)
                {
                    foreach (var user in allScruMsterUsers)
                        if (user.Id == userRole.UserId)
                            adminList.Add(user);
                }
            }
            var teamScruMsterUsers = new HashSet<string>(team.ScruMsterUsers.Select(c => c.Id));
            var viewModel = new List<ScruMsterUser>();
            foreach (var user in allScruMsterUsers)
            {
                if (!user.Assigned && !adminList.Contains(user))
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

        private void PopulateTeamScruMsterUserForEdit(Team team)
        {
            var allScruMsterUsers = _context.ScruMsterUsers;
            var allUserRoles = _context.UserRoles;
            var allRoles = _context.Roles;
            string adminID = "";
            string managerID = "";
            var adminList = new List<ScruMsterUser>();
            foreach (var role in allRoles)
            {
                if (role.Name == "Admin")
                {
                    adminID = role.Id;
                    break;
                }
            }
            foreach (var role in allRoles)
            {
                if (role.Name == "Manager")
                {
                    managerID = role.Id;
                    break;
                }
            }
            foreach (var userRole in allUserRoles)
            {
                if ((string)userRole.RoleId == adminID || (string)userRole.RoleId == managerID)
                {
                    foreach (var user in allScruMsterUsers)
                        if (user.Id == userRole.UserId)
                            adminList.Add(user);
                }
            }
            var viewModel = new List<ScruMsterUser>();
            foreach (var user in allScruMsterUsers)
            {
                if (!adminList.Contains(user) && (user.TeamID == team.TeamID || user.Assigned == false))
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

            }
            ViewBag.TotalUsers = viewModel;

        }
    }
}
