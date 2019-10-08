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
    public class ProjectController : Controller
    {
        private readonly RadarContext _context;

        public ProjectController(RadarContext context)
        {
            _context = context;
        }

        // GET: Project
        public async Task<IActionResult> Index()
        {
            var projectList = await (from mapping in _context.UserMappingProject.Where(i => i.Username.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                                     join project in _context.Project
                                     on mapping.ProjectID equals project.ID
                                     select new ProjectViewModel
                                     {
                                         ID = project.ID,
                                         ProjectName = project.ProjectName,
                                         CanWrite = mapping.CanWrite,
                                         UpdatedBy = project.UpdatedBy,
                                         UpdatedDateTime = project.UpdatedDateTime
                                     }).OrderByDescending(i => i.UpdatedDateTime).ToListAsync();
            return View(projectList);
        }

        // GET: Project/Create
        public IActionResult Create()
        {
            Project newProject = new Project();
            newProject.SetCreator(User.Identity.Name);

            return View(newProject);
        }

        // POST: Project/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProjectName,ID,CreatedBy,CreatedDateTime,UpdatedBy,UpdatedDateTime,IsActive")] Project project)
        {
            var isDuplicated = _context.Project.Any(i => i.ProjectName.Equals(project.ProjectName, StringComparison.OrdinalIgnoreCase));
            if (isDuplicated)
            {
                ModelState.AddModelError("ProjectName", "This Project Name is already existing. ");
            }

            if (ModelState.IsValid)
            {
                project.SetCreator(User.Identity.Name);
                _context.Add(project);

                UserMappingProject newMapping = new UserMappingProject()
                {
                    Username = project.CreatedBy,
                    ProjectID = project.ID,
                    CanWrite = true
                };
                newMapping.SetCreator(User.Identity.Name);
                _context.Add(newMapping);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // GET: Project/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            if (!AuthorizeData(id))
            {
                return Unauthorized();
            }

            if (!CanWrite(User.Identity.Name, id))
            {
                return Forbid();
            }

            return View(project);
        }

        // POST: Project/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ProjectName,ID,CreatedBy,CreatedDateTime,UpdatedBy,UpdatedDateTime,IsActive")] Project project)
        {
            if (id != project.ID)
            {
                return NotFound();
            }

            if (!AuthorizeData(id))
            {
                return Unauthorized();
            }

            if (!CanWrite(User.Identity.Name, id))
            {
                return Forbid();
            }

            var isDuplicated = _context.Project.Any(i => i.ProjectName.Equals(project.ProjectName, StringComparison.OrdinalIgnoreCase));
            if (isDuplicated)
            {
                ModelState.AddModelError("ProjectName", "This Project Name is already existing. ");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    project.SetUpdater(User.Identity.Name);
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.ID))
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
            return View(project);
        }

        // GET: Project/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .FirstOrDefaultAsync(m => m.ID == id);
            if (project == null)
            {
                return NotFound();
            }

            if (!AuthorizeData(id))
            {
                return Unauthorized();
            }

            if (!CanWrite(User.Identity.Name, id))
            {
                return Forbid();
            }

            return View(project);
        }

        // POST: Project/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!AuthorizeData(id))
            {
                return Unauthorized();
            }

            if (!CanWrite(User.Identity.Name, id))
            {
                return Forbid();
            }

            if (await _context.TestScenario.AnyAsync(i => i.ProjectID.Equals(id, StringComparison.OrdinalIgnoreCase)))
            {
                return RedirectToAction("Error", "Home", new { errorMessage = "All scenarios of this project must be deleted first." });
            }

            var project = await _context.Project.FindAsync(id);
            _context.Project.Remove(project);
            var assignments = await _context.UserMappingProject.Where(i => i.ProjectID.Equals(project.ID, StringComparison.OrdinalIgnoreCase)).ToListAsync();
            _context.UserMappingProject.RemoveRange(assignments);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(string id)
        {
            return _context.Project.Any(e => e.ID == id);
        }

        [HttpGet]
        public async Task<JsonResult> GetProjectSelectList(bool onlyCanWrite = false)
        {
            var projectList = await GetProjectListItems(onlyCanWrite);
            var projectSelectList = new SelectList(projectList, "Value", "Text");
            return Json(projectSelectList);
        }

        public async Task<List<SelectListItem>> GetProjectListItems(bool onlyCanWrite)
        {
            var projectList = await (from userProject in _context.UserMappingProject.Where(i => i.Username.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)
                                     && (!onlyCanWrite || i.CanWrite))
                                     join project in _context.Project
                                     on userProject.ProjectID equals project.ID
                                     orderby project.UpdatedDateTime descending
                                     select new SelectListItem { Value = project.ID, Text = project.ProjectName }).ToListAsync();
            return projectList;
        }

        private bool AuthorizeData(string projectID)
        {
            var result = _context.UserMappingProject.Any(i => i.Username.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)
                                        && i.ProjectID.Equals(projectID, StringComparison.OrdinalIgnoreCase));
            return result;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Report(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            //if (!AuthorizeData(id))
            //{
            //    return Unauthorized();
            //}

            TestReportProjectViewModel report = new TestReportProjectViewModel();
            report.ProjectID = id;
            report.ProjectName = project.ProjectName;

            //Summary by Project
            var testCaseRaw = await (from testCase in _context.TestCase.Where(i => i.IsActive)
                                     join variant in _context.TestVariant.Where(i => i.IsActive)
                                     on testCase.TestVariantID equals variant.ID
                                     join scenario in _context.TestScenario.Where(i => i.IsActive && i.ProjectID.Equals(id, StringComparison.OrdinalIgnoreCase))
                                     on variant.ScenarioID equals scenario.ID
                                     select new
                                     {
                                         ProjectID = scenario.ProjectID,
                                         ScenarioID = scenario.ID,
                                         ScenarioName = scenario.ScenarioName,
                                         VariantID = variant.ID,
                                         VariantName = variant.VariantName,
                                         TestCase = testCase
                                     }).ToListAsync();
            var projectSummary = testCaseRaw.GroupBy(i => i.TestCase.TestStatus).Select(i => new { TestStatus = i.Key, Total = i.Count() }).ToList();

            var totalPassed = projectSummary.FirstOrDefault(i => i.TestStatus.Equals(TestStatuses.Passed, StringComparison.OrdinalIgnoreCase))?.Total;
            report.Passed = totalPassed.HasValue ? totalPassed.Value : 0;
            var totalFailed = projectSummary.FirstOrDefault(i => i.TestStatus.Equals(TestStatuses.Failed, StringComparison.OrdinalIgnoreCase))?.Total;
            report.Failed = totalFailed.HasValue ? totalFailed.Value : 0;
            var totalFixed = projectSummary.FirstOrDefault(i => i.TestStatus.Equals(TestStatuses.Fixed, StringComparison.OrdinalIgnoreCase))?.Total;
            report.Fixed = totalFixed.HasValue ? totalFixed.Value : 0;
            var totalOpen = projectSummary.FirstOrDefault(i => i.TestStatus.Equals(TestStatuses.Open, StringComparison.OrdinalIgnoreCase))?.Total;
            report.Open = totalOpen.HasValue ? totalOpen.Value : 0;
            var totalPending = projectSummary.FirstOrDefault(i => i.TestStatus.Equals(TestStatuses.Pending, StringComparison.OrdinalIgnoreCase))?.Total;
            report.Pending = totalPending.HasValue ? totalPending.Value : 0;
            var totalHold = projectSummary.FirstOrDefault(i => i.TestStatus.Equals(TestStatuses.Hold, StringComparison.OrdinalIgnoreCase))?.Total;
            report.Hold = totalHold.HasValue ? totalHold.Value : 0;
            var totalCanceled = projectSummary.FirstOrDefault(i => i.TestStatus.Equals(TestStatuses.Canceled, StringComparison.OrdinalIgnoreCase))?.Total;
            report.Canceled = totalCanceled.HasValue ? totalCanceled.Value : 0;
            if (testCaseRaw.Count() > 0)
            {
                report.StartDate = testCaseRaw.Min(i => i.TestCase.PlannedDate);
                report.EndDate = testCaseRaw.Max(i => i.TestCase.PlannedDate);
            }

            //Summry by Senario
            var scenarioRaw = await _context.TestScenario.Where(i => i.IsActive && i.ProjectID.Equals(id, StringComparison.OrdinalIgnoreCase)).ToListAsync();
            var scenarioSummary = (from scenario in scenarioRaw
                                   join passed in testCaseRaw.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Passed, StringComparison.OrdinalIgnoreCase))
                                   .GroupBy(i => i.ScenarioID).Select(i => new { ScenarioID = i.Key, Total = i.Count() })
                                   on scenario.ID equals passed.ScenarioID into groupPassed
                                   join failed in testCaseRaw.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Failed, StringComparison.OrdinalIgnoreCase))
                                   .GroupBy(i => i.ScenarioID).Select(i => new { ScenarioID = i.Key, Total = i.Count() })
                                   on scenario.ID equals failed.ScenarioID into groupFailed
                                   join cfixed in testCaseRaw.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Fixed, StringComparison.OrdinalIgnoreCase))
                                   .GroupBy(i => i.ScenarioID).Select(i => new { ScenarioID = i.Key, Total = i.Count() })
                                   on scenario.ID equals cfixed.ScenarioID into groupFixed
                                   join open in testCaseRaw.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Open, StringComparison.OrdinalIgnoreCase))
                                   .GroupBy(i => i.ScenarioID).Select(i => new { ScenarioID = i.Key, Total = i.Count() })
                                   on scenario.ID equals open.ScenarioID into groupOpen
                                   join pending in testCaseRaw.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Pending, StringComparison.OrdinalIgnoreCase))
                                   .GroupBy(i => i.ScenarioID).Select(i => new { ScenarioID = i.Key, Total = i.Count() })
                                   on scenario.ID equals pending.ScenarioID into groupPending
                                   join hold in testCaseRaw.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Hold, StringComparison.OrdinalIgnoreCase))
                                   .GroupBy(i => i.ScenarioID).Select(i => new { ScenarioID = i.Key, Total = i.Count() })
                                   on scenario.ID equals hold.ScenarioID into groupHold
                                   join canceled in testCaseRaw.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Canceled, StringComparison.OrdinalIgnoreCase))
                                   .GroupBy(i => i.ScenarioID).Select(i => new { ScenarioID = i.Key, Total = i.Count() })
                                   on scenario.ID equals canceled.ScenarioID into groupCanceled
                                   join startDate in testCaseRaw
                                   .GroupBy(i => i.ScenarioID).Select(i => new { ScenarioID = i.Key, StartDate = i.Min(g => g.TestCase.PlannedDate) })
                                   on scenario.ID equals startDate.ScenarioID into groupStartDate
                                   join endDate in testCaseRaw
                                   .GroupBy(i => i.ScenarioID).Select(i => new { ScenarioID = i.Key, EndDate = i.Max(g => g.TestCase.PlannedDate) })
                                   on scenario.ID equals endDate.ScenarioID into groupEndDate
                                   from passedLeft in groupPassed.DefaultIfEmpty()
                                   from failedLeft in groupFailed.DefaultIfEmpty()
                                   from fixedLeft in groupFixed.DefaultIfEmpty()
                                   from openLeft in groupOpen.DefaultIfEmpty()
                                   from pendingLeft in groupPending.DefaultIfEmpty()
                                   from holdLeft in groupHold.DefaultIfEmpty()
                                   from canceledLeft in groupCanceled.DefaultIfEmpty()
                                   from startDateLeft in groupStartDate.DefaultIfEmpty()
                                   from endDateLeft in groupEndDate.DefaultIfEmpty()
                                   orderby startDateLeft?.StartDate, scenario.ScenarioName
                                   select new TestReportByScenario()
                                   {
                                       ScenarioID = scenario.ID,
                                       ScenarioName = scenario.ScenarioName,
                                       Passed = passedLeft != null ? passedLeft.Total : 0,
                                       Failed = failedLeft != null ? failedLeft.Total : 0,
                                       Fixed = fixedLeft != null ? fixedLeft.Total : 0,
                                       Open = openLeft != null ? openLeft.Total : 0,
                                       Pending = pendingLeft != null ? pendingLeft.Total : 0,
                                       Hold = holdLeft != null ? holdLeft.Total : 0,
                                       Canceled = canceledLeft != null ? canceledLeft.Total : 0,
                                       StartDate = startDateLeft?.StartDate,
                                       EndDate = endDateLeft?.EndDate
                                   }).ToList();

            report.ReportByScenario = scenarioSummary;

            //Summry by PlannedDate per Day
            var planSummary = (from plan in testCaseRaw.Select(i => i.TestCase.PlannedDate).Distinct()
                               join passed in testCaseRaw.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Passed, StringComparison.OrdinalIgnoreCase))
                               .GroupBy(i => i.TestCase.PlannedDate).Select(i => new { PlannedDate = i.Key, Total = i.Count() })
                               on plan equals passed.PlannedDate into groupPassed
                               join failed in testCaseRaw.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Failed, StringComparison.OrdinalIgnoreCase))
                               .GroupBy(i => i.TestCase.PlannedDate).Select(i => new { PlannedDate = i.Key, Total = i.Count() })
                               on plan equals failed.PlannedDate into groupFailed
                               join cfixed in testCaseRaw.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Fixed, StringComparison.OrdinalIgnoreCase))
                               .GroupBy(i => i.TestCase.PlannedDate).Select(i => new { PlannedDate = i.Key, Total = i.Count() })
                               on plan equals cfixed.PlannedDate into groupFixed
                               join open in testCaseRaw.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Open, StringComparison.OrdinalIgnoreCase))
                               .GroupBy(i => i.TestCase.PlannedDate).Select(i => new { PlannedDate = i.Key, Total = i.Count() })
                               on plan equals open.PlannedDate into groupOpen
                               join pending in testCaseRaw.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Pending, StringComparison.OrdinalIgnoreCase))
                               .GroupBy(i => i.TestCase.PlannedDate).Select(i => new { PlannedDate = i.Key, Total = i.Count() })
                               on plan equals pending.PlannedDate into groupPending
                               join hold in testCaseRaw.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Hold, StringComparison.OrdinalIgnoreCase))
                               .GroupBy(i => i.TestCase.PlannedDate).Select(i => new { PlannedDate = i.Key, Total = i.Count() })
                               on plan equals hold.PlannedDate into groupHold
                               join canceled in testCaseRaw.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Canceled, StringComparison.OrdinalIgnoreCase))
                               .GroupBy(i => i.TestCase.PlannedDate).Select(i => new { PlannedDate = i.Key, Total = i.Count() })
                               on plan equals canceled.PlannedDate into groupCanceled
                               from passedLeft in groupPassed.DefaultIfEmpty()
                               from failedLeft in groupFailed.DefaultIfEmpty()
                               from fixedLeft in groupFixed.DefaultIfEmpty()
                               from openLeft in groupOpen.DefaultIfEmpty()
                               from pendingLeft in groupPending.DefaultIfEmpty()
                               from holdLeft in groupHold.DefaultIfEmpty()
                               from canceldLeft in groupCanceled.DefaultIfEmpty()
                               select new TestReportByPlannedDate()
                               {
                                   PlannedDate = plan,
                                   Passed = passedLeft != null ? passedLeft.Total : 0,
                                   Failed = failedLeft != null ? failedLeft.Total : 0,
                                   Fixed = fixedLeft != null ? fixedLeft.Total : 0,
                                   Open = openLeft != null ? openLeft.Total : 0,
                                   Pending = pendingLeft != null ? pendingLeft.Total : 0,
                                   Hold = holdLeft != null ? holdLeft.Total : 0,
                                   Canceled = canceldLeft != null ? canceldLeft.Total : 0
                               }).OrderBy(i => i.PlannedDate).ToList();
            report.ReportByPlannedDate = planSummary;

            //Summry by PlannedDate per Day Accumulation
            var runningPlanSummary = (from plan in planSummary
                                      from running in planSummary
                                      where plan.PlannedDate >= running.PlannedDate
                                      select new
                                      {
                                          plan.PlannedDate,
                                          running.Passed,
                                          running.Failed,
                                          running.Fixed,
                                          running.Open,
                                          running.Pending,
                                          running.Hold,
                                          running.Canceled,
                                          running.CurrentWorkloadPercentage,
                                          running.UpToEndWorkloadPercentage
                                      })
                                        .GroupBy(i => i.PlannedDate)
                                        .Select(i => new TestReportByPlannedDateAccumulation()
                                        {
                                            PlannedDate = i.Key,
                                            Passed = i.Sum(j => j.Passed),
                                            Failed = i.Sum(j => j.Failed),
                                            Fixed = i.Sum(j => j.Fixed),
                                            Open = i.Sum(j => j.Open),
                                            Pending = i.Sum(j => j.Pending),
                                            Hold = i.Sum(j => j.Hold),
                                            Canceled = i.Sum(j => j.Canceled),
                                            CurrentWorkloadPercentage = i.Sum(j => j.CurrentWorkloadPercentage),
                                            UpToEndWorkloadPercentage = i.Sum(j => j.UpToEndWorkloadPercentage)
                                        }).OrderBy(i => i.PlannedDate).ToList();
            report.ReportByPlannedDateAccumulation = runningPlanSummary;

            return View(report);
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
