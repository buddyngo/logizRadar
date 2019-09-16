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
    public class TestCaseController : Controller
    {
        private readonly RadarContext _context;

        public TestCaseController(RadarContext context)
        {
            _context = context;
        }

        // GET: TestCase
        public async Task<IActionResult> Index(string ProjectID, string ScenarioID, string VariantID)
        {
            var caseList = await (from userProject in _context.UserMappingProject.Where(i => i.Username.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                                  join scenario in _context.TestScenario.Where(i => i.ProjectID.Equals(ProjectID, StringComparison.OrdinalIgnoreCase))
                                  on userProject.ProjectID equals scenario.ProjectID
                                  join variant in _context.TestVariant.Where(i => i.ScenarioID.Equals(ScenarioID, StringComparison.OrdinalIgnoreCase))
                                  on scenario.ID equals variant.ScenarioID
                                  join testCase in _context.TestCase.Where(i => i.TestVariantID.Equals(VariantID, StringComparison.OrdinalIgnoreCase))
                                  on variant.ID equals testCase.TestVariantID
                                  select new TestCase
                                  {
                                      ID = testCase.ID,
                                      TestVariantID = variant.VariantName,
                                      TestCaseName = testCase.TestCaseName,
                                      ExpectedResult = testCase.ExpectedResult,
                                      ActualResult = testCase.ActualResult,
                                      PlannedDate = testCase.PlannedDate,
                                      TestStatus = testCase.TestStatus,
                                      UpdatedBy = testCase.UpdatedBy,
                                      UpdatedDateTime = testCase.UpdatedDateTime
                                  }).OrderBy(i => i.TestCaseName).ToListAsync();

            ViewBag.ProjectID = ProjectID;
            ViewBag.ScenarioID = ScenarioID;
            ViewBag.VariantID = VariantID;

            return View(caseList);
        }

        // GET: TestCase/Create
        public IActionResult Create()
        {
            TestCase testCase = new TestCase();
            testCase.SetCreator(User.Identity.Name);
            testCase.TestStatus = TestCaseStatus.Open;
            TestCaseViewModel newCase = new TestCaseViewModel()
            {
                ProjectID = string.Empty,
                ScenarioID = string.Empty,
                TestCase = testCase
            };
            return View(newCase);
        }

        // POST: TestCase/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string ProjectID, string ScenarioID, [Bind("TestCaseName,TestVariantID,TestCaseSteps,ExpectedResult,ActualResult,TesterName,PlannedDate,TestStatus,Note,ID,CreatedBy,CreatedDateTime,UpdatedBy,UpdatedDateTime,IsActive")] TestCase testCase)
        {
            if (ModelState.IsValid)
            {
                testCase.SetCreator(User.Identity.Name);
                _context.Add(testCase);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { ProjectID = ProjectID, ScenarioID = ScenarioID, VariantID = testCase.TestVariantID });
            }
            return View(testCase);
        }

        // GET: TestCase/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var testCase = await _context.TestCase.FindAsync(id);
            if (testCase == null)
            {
                return NotFound();
            }

            var testVariant = await (from variant in _context.TestVariant.Where(i => i.ID.Equals(testCase.TestVariantID, StringComparison.OrdinalIgnoreCase))
                                     join scenario in _context.TestScenario
                                     on variant.ScenarioID equals scenario.ID
                                     select new { ProjectID = scenario.ProjectID, ScenarioID = scenario.ID }).FirstOrDefaultAsync();

            if (testVariant == null)
            {
                return NotFound();
            }

            if (!AuthorizeData(id))
            {
                return Unauthorized();
            }

            var testCaseViewModel = new TestCaseViewModel()
            {
                ProjectID = testVariant.ProjectID,
                ScenarioID = testVariant.ScenarioID,
                TestCase = testCase
            };
            return View(testCaseViewModel);
        }

        // POST: TestCase/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string ProjectID, string ScenarioID, [Bind("TestCaseName,TestVariantID,TestCaseSteps,ExpectedResult,ActualResult,TesterName,PlannedDate,TestStatus,Note,ID,CreatedBy,CreatedDateTime,UpdatedBy,UpdatedDateTime,IsActive")] TestCase testCase)
        {
            if (id != testCase.ID)
            {
                return NotFound();
            }

            if (!AuthorizeData(id))
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    testCase.SetUpdater(User.Identity.Name);
                    _context.Update(testCase);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TestCaseExists(testCase.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { ProjectID = ProjectID, ScenarioID = ScenarioID, VariantID = testCase.TestVariantID });
            }
            var testCaseViewModel = new TestCaseViewModel()
            {
                ProjectID = ProjectID,
                ScenarioID = ScenarioID,
                TestCase = testCase
            };
            return View(testCaseViewModel);
        }

        // GET: TestCase/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var testCase = await _context.TestCase
                .FirstOrDefaultAsync(m => m.ID == id);
            if (testCase == null)
            {
                return NotFound();
            }

            if (!AuthorizeData(id))
            {
                return Unauthorized();
            }

            return View(testCase);
        }

        // POST: TestCase/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!AuthorizeData(id))
            {
                return Unauthorized();
            }

            var testCase = await _context.TestCase.FindAsync(id);

            var testVariant = await (from variant in _context.TestVariant.Where(i => i.ID.Equals(testCase.TestVariantID, StringComparison.OrdinalIgnoreCase))
                                     join scenario in _context.TestScenario
                                     on variant.ScenarioID equals scenario.ID
                                     select new { ProjectID = scenario.ProjectID, ScenarioID = scenario.ID }).FirstOrDefaultAsync();

            _context.TestCase.Remove(testCase);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { ProjectID = testVariant?.ProjectID, ScenarioID = testVariant?.ScenarioID, VariantID = testCase.TestVariantID });
        }

        private bool TestCaseExists(string id)
        {
            return _context.TestCase.Any(e => e.ID == id);
        }

        private bool AuthorizeData(string caseID)
        {
            var result = (from testCase in _context.TestCase.Where(i => i.ID.Equals(caseID, StringComparison.OrdinalIgnoreCase))
                          join variant in _context.TestVariant
                          on testCase.TestVariantID equals variant.ID
                          join scenario in _context.TestScenario
                          on variant.ScenarioID equals scenario.ID
                          join mapping in _context.UserMappingProject.Where(i => i.Username.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                          on scenario.ProjectID equals mapping.ProjectID
                          select 1).Any();
            return result;
        }
    }
}
