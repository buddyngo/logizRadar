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
    public class TestScenarioController : Controller
    {
        private readonly RadarContext _context;

        public TestScenarioController(RadarContext context)
        {
            _context = context;
        }

        // GET: TestScenario
        public async Task<IActionResult> Index(string ProjectID)
        {
            var scenarioList = await (from project in _context.UserMappingProject.Where(i => i.Username.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                                      join scenario in _context.TestScenario.Where(i => i.ProjectID.Equals(ProjectID, StringComparison.OrdinalIgnoreCase))
                                      on project.ProjectID equals scenario.ProjectID
                                      select new TestScenario
                                      {
                                          ID = scenario.ID,
                                          ScenarioName = scenario.ScenarioName,
                                          UpdatedBy = scenario.UpdatedBy,
                                          UpdatedDateTime = scenario.UpdatedDateTime
                                      }).OrderBy(i => i.ScenarioName).ToListAsync();

            ViewBag.ProjectID = ProjectID;
            ViewBag.CanWrite = CanWrite(User.Identity.Name, ProjectID);

            return View(scenarioList);
        }

        // GET: TestScenario/Create
        public IActionResult Create()
        {
            TestScenario newSenario = new TestScenario();
            newSenario.SetCreator(User.Identity.Name);

            return View(newSenario);
        }

        // POST: TestScenario/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ScenarioName,ProjectID,ID,CreatedBy,CreatedDateTime,UpdatedBy,UpdatedDateTime,IsActive")] TestScenario testScenario)
        {
            if (!CanWrite(User.Identity.Name, testScenario.ProjectID))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                testScenario.SetCreator(User.Identity.Name);
                _context.Add(testScenario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { ProjectID = testScenario.ProjectID });
            }
            return View(testScenario);
        }

        // GET: TestScenario/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var testScenario = await _context.TestScenario.FindAsync(id);
            if (testScenario == null)
            {
                return NotFound();
            }

            if (!AuthorizeData(id))
            {
                return Unauthorized();
            }

            if (!CanWrite(User.Identity.Name, testScenario.ProjectID))
            {
                return Forbid();
            }

            return View(testScenario);
        }

        // POST: TestScenario/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ScenarioName,ProjectID,ID,CreatedBy,CreatedDateTime,UpdatedBy,UpdatedDateTime,IsActive")] TestScenario testScenario)
        {
            if (id != testScenario.ID)
            {
                return NotFound();
            }

            if (!AuthorizeData(id))
            {
                return Unauthorized();
            }

            if (!CanWrite(User.Identity.Name, testScenario.ProjectID))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    testScenario.SetUpdater(User.Identity.Name);
                    _context.Update(testScenario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TestScenarioExists(testScenario.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { ProjectID = testScenario.ProjectID });
            }
            return View(testScenario);
        }

        // GET: TestScenario/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var testScenario = await _context.TestScenario
                .FirstOrDefaultAsync(m => m.ID == id);
            if (testScenario == null)
            {
                return NotFound();
            }

            if (!AuthorizeData(id))
            {
                return Unauthorized();
            }

            if (!CanWrite(User.Identity.Name, testScenario.ProjectID))
            {
                return Forbid();
            }

            return View(testScenario);
        }

        // POST: TestScenario/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!AuthorizeData(id))
            {
                return Unauthorized();
            }

            var testScenario = await _context.TestScenario.FindAsync(id);

            if (!CanWrite(User.Identity.Name, testScenario?.ProjectID))
            {
                return Forbid();
            }

            if (await _context.TestVariant.AnyAsync(i => i.ScenarioID.Equals(id, StringComparison.OrdinalIgnoreCase)))
            {
                return RedirectToAction("Error", "Home", new { errorMessage = "All variants of this scenario must be deleted first." });
            }

            _context.TestScenario.Remove(testScenario);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { ProjectID = testScenario.ProjectID });
        }

        private bool TestScenarioExists(string id)
        {
            return _context.TestScenario.Any(e => e.ID == id);
        }

        [HttpGet]
        public async Task<JsonResult> GetTestScenarioSelectList(string ProjectId)
        {
            var scenarioList = await GetTestScenarioListItems(ProjectId);
            var scenarioListSelectList = new SelectList(scenarioList, "Value", "Text");
            return Json(scenarioListSelectList);
        }

        public async Task<List<SelectListItem>> GetTestScenarioListItems(string ProjectId)
        {
            var scenarioList = await (from scenario in _context.TestScenario.Where(i => i.ProjectID.Equals(ProjectId, StringComparison.OrdinalIgnoreCase))
                                      orderby scenario.ScenarioName
                                      select new SelectListItem { Value = scenario.ID, Text = scenario.ScenarioName }).ToListAsync();
            return scenarioList;
        }

        private bool AuthorizeData(string scenarioID)
        {
            var result = (from scenario in _context.TestScenario.Where(i => i.ID.Equals(scenarioID, StringComparison.OrdinalIgnoreCase))
                          join mapping in _context.UserMappingProject.Where(i => i.Username.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                          on scenario.ProjectID equals mapping.ProjectID
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
