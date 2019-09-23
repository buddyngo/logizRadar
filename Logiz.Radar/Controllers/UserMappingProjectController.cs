using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Logiz.Radar.Data.Context;
using Logiz.Radar.Data.Model;
using Microsoft.AspNetCore.Authorization;

namespace Logiz.Radar.Controllers
{
    [Authorize]
    public class UserMappingProjectController : Controller
    {
        private readonly RadarContext _context;

        public UserMappingProjectController(RadarContext context)
        {
            _context = context;
        }

        // GET: UserMappingProject
        public async Task<IActionResult> Index(string ProjectID)
        {
            var dataList = await (from project in _context.UserMappingProject.Where(i => i.Username.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                                  join mapping in _context.UserMappingProject.Where(i => i.ProjectID.Equals(ProjectID, StringComparison.OrdinalIgnoreCase))
                                  on project.ProjectID equals mapping.ProjectID
                                  select new UserMappingProject
                                  {
                                      ID = mapping.ID,
                                      Username = mapping.Username,
                                      CanWrite = mapping.CanWrite,
                                      UpdatedBy = mapping.UpdatedBy,
                                      UpdatedDateTime = mapping.UpdatedDateTime
                                  }).OrderByDescending(i => i.UpdatedDateTime).ToListAsync();

            ViewBag.ProjectID = ProjectID;
            ViewBag.CanWrite = CanWrite(User.Identity.Name, ProjectID);

            return View(dataList);
        }

        // GET: UserMappingProject/Create
        public IActionResult Create()
        {
            UserMappingProject newMapping = new UserMappingProject();
            newMapping.SetCreator(User.Identity.Name);
            return View(newMapping);
        }

        // POST: UserMappingProject/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Username,ProjectID,CanWrite,ID,CreatedBy,CreatedDateTime,UpdatedBy,UpdatedDateTime,IsActive")] UserMappingProject userMappingProject)
        {
            if (!CanWrite(User.Identity.Name, userMappingProject.ProjectID))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                userMappingProject.SetCreator(User.Identity.Name);
                _context.Add(userMappingProject);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { ProjectID = userMappingProject.ProjectID });
            }
            return View(userMappingProject);
        }

        [AcceptVerbs("Get", "Post")]
        public IActionResult CheckDuplicateMapping(string Username, string ProjectID, string ID)
        {
            var mapping = _context.UserMappingProject.FirstOrDefault(i => i.Username.Equals(Username, StringComparison.OrdinalIgnoreCase)
            && i.ProjectID.Equals(ProjectID, StringComparison.OrdinalIgnoreCase));
            if (mapping != null)
            {
                if (!mapping.ID.Equals(ID, StringComparison.OrdinalIgnoreCase))
                {
                    return Json("This mapping is already existing.");
                }
            }

            return Json(true);
        }

        // GET: UserMappingProject/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userMappingProject = await _context.UserMappingProject.FindAsync(id);
            if (userMappingProject == null)
            {
                return NotFound();
            }

            if (!AuthorizeData(id))
            {
                return Unauthorized();
            }

            if (!CanWrite(User.Identity.Name, userMappingProject.ProjectID))
            {
                return Forbid();
            }

            return View(userMappingProject);
        }

        // POST: UserMappingProject/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Username,ProjectID,CanWrite,ID,CreatedBy,CreatedDateTime,UpdatedBy,UpdatedDateTime,IsActive")] UserMappingProject userMappingProject)
        {
            if (id != userMappingProject.ID)
            {
                return NotFound();
            }

            if (!AuthorizeData(id))
            {
                return Unauthorized();
            }

            if (!CanWrite(User.Identity.Name, userMappingProject.ProjectID))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    userMappingProject.SetUpdater(User.Identity.Name);
                    _context.Update(userMappingProject);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserMappingProjectExists(userMappingProject.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { ProjectID = userMappingProject.ProjectID });
            }
            return View(userMappingProject);
        }

        // GET: UserMappingProject/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userMappingProject = await _context.UserMappingProject
                .FirstOrDefaultAsync(m => m.ID == id);
            if (userMappingProject == null)
            {
                return NotFound();
            }

            if (!AuthorizeData(id))
            {
                return Unauthorized();
            }

            if (!CanWrite(User.Identity.Name, userMappingProject.ProjectID))
            {
                return Forbid();
            }

            return View(userMappingProject);
        }

        // POST: UserMappingProject/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!AuthorizeData(id))
            {
                return Unauthorized();
            }

            var userMappingProject = await _context.UserMappingProject.FindAsync(id);

            if (!CanWrite(User.Identity.Name, userMappingProject?.ProjectID))
            {
                return Forbid();
            }

            _context.UserMappingProject.Remove(userMappingProject);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { ProjectID = userMappingProject.ProjectID });
        }

        private bool UserMappingProjectExists(string id)
        {
            return _context.UserMappingProject.Any(e => e.ID == id);
        }

        [HttpGet]
        public async Task<JsonResult> GetAssignedUserSelectList()
        {
            var userList = await GetAssignedUserListItems();
            var userSelectList = new SelectList(userList, "Value", "Text");
            return Json(userSelectList);
        }

        public async Task<List<SelectListItem>> GetAssignedUserListItems()
        {
            var userList = await (from project in _context.UserMappingProject.Where(i => i.Username.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                                  join user in _context.UserMappingProject
                                  on project.ProjectID equals user.ProjectID
                                  orderby user.UpdatedDateTime descending
                                  select user.Username)
                                  .Distinct().Select(i => new SelectListItem() { Value = i, Text = i }).ToListAsync();
            return userList;
        }

        private bool AuthorizeData(string mappingID)
        {
            var result = (from project in _context.UserMappingProject.Where(i => i.ID.Equals(mappingID, StringComparison.OrdinalIgnoreCase))
                          join mapping in _context.UserMappingProject.Where(i => i.Username.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                          on project.ProjectID equals mapping.ProjectID
                          select 1).Any();
            return result;
        }

        public bool CanWrite(string username, string projectID)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(projectID))
            {
                return false;
            }

            var canWrite = _context.UserMappingProject.Any(i => i.Username.Equals(username, StringComparison.OrdinalIgnoreCase)
                && i.ProjectID.Equals(projectID, StringComparison.OrdinalIgnoreCase)
                && i.IsActive & i.CanWrite);
            return canWrite;
        }
    }
}
