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
using Logiz.Radar.Models;

namespace Logiz.Radar.Controllers
{
    [Authorize]
    public class TestVariantController : Controller
    {
        private readonly RadarContext _context;

        public TestVariantController(RadarContext context)
        {
            _context = context;
        }

        // GET: TestVariant
        public async Task<IActionResult> Index(string ProjectID, string ScenarioID)
        {
            var variantList = await (from userProject in _context.UserMappingProject.Where(i => i.Username.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                                     join scenario in _context.TestScenario.Where(i => i.ProjectID.Equals(ProjectID, StringComparison.OrdinalIgnoreCase))
                                     on userProject.ProjectID equals scenario.ProjectID
                                     join variant in _context.TestVariant.Where(i => string.IsNullOrEmpty(ScenarioID) || i.ScenarioID.Equals(ScenarioID, StringComparison.OrdinalIgnoreCase))
                                     on scenario.ID equals variant.ScenarioID
                                     select new TestVariant
                                     {
                                         ID = variant.ID,
                                         ScenarioID = scenario.ScenarioName,
                                         VariantName = variant.VariantName,
                                         UpdatedBy = variant.UpdatedBy,
                                         UpdatedDateTime = variant.UpdatedDateTime
                                     }).OrderBy(i => i.VariantName).ToListAsync();

            ViewBag.ProjectID = ProjectID;
            ViewBag.ScenarioID = ScenarioID;
            ViewBag.CanWrite = CanWrite(User.Identity.Name, ProjectID);

            return View(variantList);
        }

        // GET: TestVariant/Create
        public IActionResult Create()
        {
            TestVariant testVariant = new TestVariant();
            testVariant.SetCreator(User.Identity.Name);
            TestVariantViewModel newVariant = new TestVariantViewModel()
            {
                ProjectID = string.Empty,
                TestVariant = testVariant
            };
            return View(newVariant);
        }

        // POST: TestVariant/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string ProjectID, [Bind("VariantName,ScenarioID,ID,CreatedBy,CreatedDateTime,UpdatedBy,UpdatedDateTime,IsActive")] TestVariant testVariant)
        {
            if (!CanWrite(User.Identity.Name, ProjectID))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                testVariant.SetCreator(User.Identity.Name);
                _context.Add(testVariant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { ProjectID = ProjectID, ScenarioID = testVariant.ScenarioID });
            }
            TestVariantViewModel newVariant = new TestVariantViewModel()
            {
                ProjectID = ProjectID,
                TestVariant = testVariant
            };
            return View(newVariant);
        }

        // GET: TestVariant/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var testVariant = await _context.TestVariant.FindAsync(id);
            if (testVariant == null)
            {
                return NotFound();
            }

            var scenario = await _context.TestScenario.FindAsync(testVariant.ScenarioID);
            if (scenario == null)
            {
                return NotFound();
            }

            if (!AuthorizeData(id))
            {
                return Unauthorized();
            }

            if (!CanWrite(User.Identity.Name, scenario.ProjectID))
            {
                return Forbid();
            }

            var testVariantViewModel = new TestVariantViewModel()
            {
                ProjectID = scenario.ProjectID,
                TestVariant = testVariant
            };
            return View(testVariantViewModel);
        }

        // POST: TestVariant/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string ProjectID, [Bind("VariantName,ScenarioID,ID,CreatedBy,CreatedDateTime,UpdatedBy,UpdatedDateTime,IsActive")] TestVariant testVariant)
        {
            if (id != testVariant.ID)
            {
                return NotFound();
            }

            if (!AuthorizeData(id))
            {
                return Unauthorized();
            }

            if (!CanWrite(User.Identity.Name, ProjectID))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    testVariant.SetUpdater(User.Identity.Name);
                    _context.Update(testVariant);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TestVariantExists(testVariant.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { ProjectID = ProjectID, ScenarioID = testVariant.ScenarioID });
            }

            var testVariantViewModel = new TestVariantViewModel()
            {
                ProjectID = ProjectID,
                TestVariant = testVariant
            };
            return View(testVariantViewModel);
        }

        // GET: TestVariant/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var testVariant = await _context.TestVariant
                .FirstOrDefaultAsync(m => m.ID == id);
            if (testVariant == null)
            {
                return NotFound();
            }

            if (!AuthorizeData(id))
            {
                return Unauthorized();
            }

            var scenario = await _context.TestScenario.FindAsync(testVariant.ScenarioID);
            if (scenario == null)
            {
                return NotFound();
            }

            if (!CanWrite(User.Identity.Name, scenario.ProjectID))
            {
                return Forbid();
            }

            return View(testVariant);
        }

        // POST: TestVariant/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!AuthorizeData(id))
            {
                return Unauthorized();
            }

            var testVariant = await _context.TestVariant.FindAsync(id);
            var scenario = await _context.TestScenario.FindAsync(testVariant?.ScenarioID);

            if (!CanWrite(User.Identity.Name, scenario?.ProjectID))
            {
                return Forbid();
            }

            _context.TestVariant.Remove(testVariant);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { ProjectID = scenario?.ProjectID, ScenarioID = testVariant.ScenarioID });
        }

        private bool TestVariantExists(string id)
        {
            return _context.TestVariant.Any(e => e.ID == id);
        }

        [HttpGet]
        public async Task<JsonResult> GetTestVariantSelectList(string ScenarioId)
        {
            var variantList = await GetTestVariantListItems(ScenarioId);
            var variantListSelectList = new SelectList(variantList, "Value", "Text");
            return Json(variantListSelectList);
        }

        public async Task<List<SelectListItem>> GetTestVariantListItems(string ScenarioId)
        {
            var variantList = await (from variant in _context.TestVariant.Where(i => i.ScenarioID.Equals(ScenarioId, StringComparison.OrdinalIgnoreCase))
                                     orderby variant.VariantName
                                     select new SelectListItem { Value = variant.ID, Text = variant.VariantName }).ToListAsync();
            return variantList;
        }

        private bool AuthorizeData(string variantID)
        {
            var result = (from variant in _context.TestVariant.Where(i => i.ID.Equals(variantID, StringComparison.OrdinalIgnoreCase))
                          join scenario in _context.TestScenario
                          on variant.ScenarioID equals scenario.ID
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
