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
using System.IO;
using OfficeOpenXml;

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
        public async Task<IActionResult> Report(string id, string Action)
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
                                         TestCase = new
                                         {
                                             ID = testCase.ID,
                                             TestVariantID = testCase.TestVariantID,
                                             TestStatus = testCase.TestStatus,
                                             TesterName = testCase.TesterName.ToUpper(),
                                             PlannedDate = testCase.PlannedDate
                                         }
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
                                       //orderby startDateLeft?.StartDate, scenario.ScenarioName
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
                                   }).OrderByDescending(i => i.DonePercentage).ThenBy(i => i.StartDate).ThenBy(i => i.ScenarioName).ToList();

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

            //Resource summary
            var resourceRaw = (from r in testCaseRaw.Select(i => new { TesterName = i.TestCase.TesterName }).Distinct()
                               join sd in testCaseRaw.GroupBy(i => i.TestCase.TesterName).Select(i => new { TesterName = i.Key, StartDate = i.Min(g => g.TestCase.PlannedDate) })
                               on r.TesterName equals sd.TesterName into groupStartDate
                               join ed in testCaseRaw.GroupBy(i => i.TestCase.TesterName).Select(i => new { TesterName = i.Key, EndDate = i.Max(g => g.TestCase.PlannedDate) })
                               on r.TesterName equals ed.TesterName into groupEndDate
                               join totalWorkingDay in testCaseRaw.Select(i => new { i.TestCase.TesterName, i.TestCase.PlannedDate }).Distinct()
                                   .GroupBy(i => i.TesterName).Select(i => new { TesterName = i.Key, Total = i.Count() })
                                   on r.TesterName equals totalWorkingDay.TesterName into groupTotalPlannedDays
                               join remainingPendingDay in testCaseRaw.Where(i => i.TestCase.PlannedDate > DateTime.Today
                                               && (i.TestCase.TestStatus.Equals(TestStatuses.Fixed, StringComparison.OrdinalIgnoreCase)
                                               || i.TestCase.TestStatus.Equals(TestStatuses.Open, StringComparison.OrdinalIgnoreCase)
                                               || i.TestCase.TestStatus.Equals(TestStatuses.Pending, StringComparison.OrdinalIgnoreCase)
                                               || i.TestCase.TestStatus.Equals(TestStatuses.Hold, StringComparison.OrdinalIgnoreCase)
                                               || i.TestCase.TestStatus.Equals(TestStatuses.Failed, StringComparison.OrdinalIgnoreCase)))
                                           .Select(i => new { i.TestCase.TesterName, i.TestCase.PlannedDate }).Distinct()
                                  .GroupBy(i => i.TesterName).Select(i => new { TesterName = i.Key, Total = i.Count() })
                                  on r.TesterName equals remainingPendingDay.TesterName into groupRemainingPendingDays
                               join remainingWorkingDay in testCaseRaw.Where(i => i.TestCase.PlannedDate > DateTime.Today
                                            && (i.TestCase.TestStatus.Equals(TestStatuses.Fixed, StringComparison.OrdinalIgnoreCase)
                                            || i.TestCase.TestStatus.Equals(TestStatuses.Open, StringComparison.OrdinalIgnoreCase)))
                                        .Select(i => new { i.TestCase.TesterName, i.TestCase.PlannedDate }).Distinct()
                               .GroupBy(i => i.TesterName).Select(i => new { TesterName = i.Key, Total = i.Count() })
                               on r.TesterName equals remainingWorkingDay.TesterName into groupRemainingWorkingDays
                               from lsd in groupStartDate.DefaultIfEmpty()
                               from led in groupEndDate.DefaultIfEmpty()
                               from totalPlannedDayLeft in groupTotalPlannedDays.DefaultIfEmpty()
                               from remainingPendingDayLeft in groupRemainingPendingDays.DefaultIfEmpty()
                               from remainingWorkingDayLeft in groupRemainingWorkingDays.DefaultIfEmpty()
                               select new
                               {
                                   TesterName = r.TesterName,
                                   StartDate = lsd?.StartDate,
                                   EndDate = led?.EndDate,
                                   TotalPlannedDays = totalPlannedDayLeft != null ? totalPlannedDayLeft.Total : 0,
                                   RemainingPendingDays = remainingPendingDayLeft != null ? remainingPendingDayLeft.Total : 0,
                                   RemainingWorkingDays = remainingWorkingDayLeft != null ? remainingWorkingDayLeft.Total : 0
                               }).ToList();

            var resourceSummary = (from resource in resourceRaw
                                   join passed in testCaseRaw.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Passed, StringComparison.OrdinalIgnoreCase))
                                   .GroupBy(i => i.TestCase.TesterName).Select(i => new { TesterName = i.Key, Total = i.Count() })
                                   on resource.TesterName equals passed.TesterName into groupPassed
                                   join failed in testCaseRaw.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Failed, StringComparison.OrdinalIgnoreCase))
                                   .GroupBy(i => i.TestCase.TesterName).Select(i => new { TesterName = i.Key, Total = i.Count() })
                                   on resource.TesterName equals failed.TesterName into groupFailed
                                   join cfixed in testCaseRaw.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Fixed, StringComparison.OrdinalIgnoreCase))
                                   .GroupBy(i => i.TestCase.TesterName).Select(i => new { TesterName = i.Key, Total = i.Count() })
                                   on resource.TesterName equals cfixed.TesterName into groupFixed
                                   join open in testCaseRaw.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Open, StringComparison.OrdinalIgnoreCase))
                                   .GroupBy(i => i.TestCase.TesterName).Select(i => new { TesterName = i.Key, Total = i.Count() })
                                   on resource.TesterName equals open.TesterName into groupOpen
                                   join pending in testCaseRaw.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Pending, StringComparison.OrdinalIgnoreCase))
                                   .GroupBy(i => i.TestCase.TesterName).Select(i => new { TesterName = i.Key, Total = i.Count() })
                                   on resource.TesterName equals pending.TesterName into groupPending
                                   join hold in testCaseRaw.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Hold, StringComparison.OrdinalIgnoreCase))
                                   .GroupBy(i => i.TestCase.TesterName).Select(i => new { TesterName = i.Key, Total = i.Count() })
                                   on resource.TesterName equals hold.TesterName into groupHold
                                   join canceled in testCaseRaw.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Canceled, StringComparison.OrdinalIgnoreCase))
                                   .GroupBy(i => i.TestCase.TesterName).Select(i => new { TesterName = i.Key, Total = i.Count() })
                                   on resource.TesterName equals canceled.TesterName into groupCanceled
                                   join startDate in testCaseRaw
                                   .GroupBy(i => i.TestCase.TesterName).Select(i => new { TesterName = i.Key, StartDate = i.Min(g => g.TestCase.PlannedDate) })
                                   on resource.TesterName equals startDate.TesterName into groupStartDate
                                   join endDate in testCaseRaw
                                   .GroupBy(i => i.TestCase.TesterName).Select(i => new { TesterName = i.Key, EndDate = i.Max(g => g.TestCase.PlannedDate) })
                                   on resource.TesterName equals endDate.TesterName into groupEndDate
                                   from passedLeft in groupPassed.DefaultIfEmpty()
                                   from failedLeft in groupFailed.DefaultIfEmpty()
                                   from fixedLeft in groupFixed.DefaultIfEmpty()
                                   from openLeft in groupOpen.DefaultIfEmpty()
                                   from pendingLeft in groupPending.DefaultIfEmpty()
                                   from holdLeft in groupHold.DefaultIfEmpty()
                                   from canceledLeft in groupCanceled.DefaultIfEmpty()
                                   from startDateLeft in groupStartDate.DefaultIfEmpty()
                                   from endDateLeft in groupEndDate.DefaultIfEmpty()
                                   select new TestReportByResourceSummary()
                                   {
                                       TesterName = resource.TesterName,
                                       Passed = passedLeft != null ? passedLeft.Total : 0,
                                       Failed = failedLeft != null ? failedLeft.Total : 0,
                                       Fixed = fixedLeft != null ? fixedLeft.Total : 0,
                                       Open = openLeft != null ? openLeft.Total : 0,
                                       Pending = pendingLeft != null ? pendingLeft.Total : 0,
                                       Hold = holdLeft != null ? holdLeft.Total : 0,
                                       Canceled = canceledLeft != null ? canceledLeft.Total : 0,
                                       StartDate = startDateLeft?.StartDate,
                                       EndDate = endDateLeft?.EndDate,
                                       TotalPlannedDays = resource.TotalPlannedDays,
                                       RemainingPendingDays = resource.RemainingPendingDays,
                                       RemainingWorkingDays = resource.RemainingWorkingDays
                                   }).OrderByDescending(i => i.DonePercentage).ThenBy(i => i.TesterName).ToList();
            report.TestReportByResourceSummary = resourceSummary;

            //Resource workload accumulation
            var testCaseRawUntilToday = testCaseRaw.Where(i => i.TestCase.PlannedDate <= DateTime.Today).ToList();
            var resourceDate = (from resource in testCaseRawUntilToday.Select(i => new { i.TestCase.TesterName, i.TestCase.PlannedDate }).Distinct()
                                join passed in testCaseRawUntilToday.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Passed, StringComparison.OrdinalIgnoreCase))
                               .GroupBy(i => new { i.TestCase.TesterName, i.TestCase.PlannedDate }).Select(i => new { TesterName = i.Key.TesterName, PlannedDate = i.Key.PlannedDate, Total = i.Count() })
                               on new { resource.TesterName, resource.PlannedDate } equals new { passed.TesterName, passed.PlannedDate } into groupPassed
                                join failed in testCaseRawUntilToday.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Failed, StringComparison.OrdinalIgnoreCase))
                                .GroupBy(i => new { i.TestCase.TesterName, i.TestCase.PlannedDate }).Select(i => new { TesterName = i.Key.TesterName, PlannedDate = i.Key.PlannedDate, Total = i.Count() })
                                on new { resource.TesterName, resource.PlannedDate } equals new { failed.TesterName, failed.PlannedDate } into groupFailed
                                join cfixed in testCaseRawUntilToday.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Fixed, StringComparison.OrdinalIgnoreCase))
                                .GroupBy(i => new { i.TestCase.TesterName, i.TestCase.PlannedDate }).Select(i => new { TesterName = i.Key.TesterName, PlannedDate = i.Key.PlannedDate, Total = i.Count() })
                                on new { resource.TesterName, resource.PlannedDate } equals new { cfixed.TesterName, cfixed.PlannedDate } into groupFixed
                                join open in testCaseRawUntilToday.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Open, StringComparison.OrdinalIgnoreCase))
                                .GroupBy(i => new { i.TestCase.TesterName, i.TestCase.PlannedDate }).Select(i => new { TesterName = i.Key.TesterName, PlannedDate = i.Key.PlannedDate, Total = i.Count() })
                                on new { resource.TesterName, resource.PlannedDate } equals new { open.TesterName, open.PlannedDate } into groupOpen
                                join pending in testCaseRawUntilToday.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Pending, StringComparison.OrdinalIgnoreCase))
                                .GroupBy(i => new { i.TestCase.TesterName, i.TestCase.PlannedDate }).Select(i => new { TesterName = i.Key.TesterName, PlannedDate = i.Key.PlannedDate, Total = i.Count() })
                                on new { resource.TesterName, resource.PlannedDate } equals new { pending.TesterName, pending.PlannedDate } into groupPending
                                join hold in testCaseRawUntilToday.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Hold, StringComparison.OrdinalIgnoreCase))
                                .GroupBy(i => new { i.TestCase.TesterName, i.TestCase.PlannedDate }).Select(i => new { TesterName = i.Key.TesterName, PlannedDate = i.Key.PlannedDate, Total = i.Count() })
                                on new { resource.TesterName, resource.PlannedDate } equals new { hold.TesterName, hold.PlannedDate } into groupHold
                                join canceled in testCaseRawUntilToday.Where(i => i.TestCase.TestStatus.Equals(TestStatuses.Canceled, StringComparison.OrdinalIgnoreCase))
                                .GroupBy(i => new { i.TestCase.TesterName, i.TestCase.PlannedDate }).Select(i => new { TesterName = i.Key.TesterName, PlannedDate = i.Key.PlannedDate, Total = i.Count() })
                                on new { resource.TesterName, resource.PlannedDate } equals new { canceled.TesterName, canceled.PlannedDate } into groupCanceled
                                from passedLeft in groupPassed.DefaultIfEmpty()
                                from failedLeft in groupFailed.DefaultIfEmpty()
                                from fixedLeft in groupFixed.DefaultIfEmpty()
                                from openLeft in groupOpen.DefaultIfEmpty()
                                from pendingLeft in groupPending.DefaultIfEmpty()
                                from holdLeft in groupHold.DefaultIfEmpty()
                                from canceledLeft in groupCanceled.DefaultIfEmpty()
                                select new TestReportByResourceDate()
                                {
                                    TesterName = resource.TesterName,
                                    PlannedDate = resource.PlannedDate,
                                    Passed = passedLeft != null ? passedLeft.Total : 0,
                                    Failed = failedLeft != null ? failedLeft.Total : 0,
                                    Fixed = fixedLeft != null ? fixedLeft.Total : 0,
                                    Open = openLeft != null ? openLeft.Total : 0,
                                    Pending = pendingLeft != null ? pendingLeft.Total : 0,
                                    Hold = holdLeft != null ? holdLeft.Total : 0,
                                    Canceled = canceledLeft != null ? canceledLeft.Total : 0,
                                }).ToList();

            var resourceWorkload = (from r in resourceRaw
                                    join rd in resourceDate.GroupBy(i => i.TesterName)
                                   .Select(i => new ResourceWorkloadAccumulation()
                                   {
                                       TesterName = i.Key,
                                       CurrentWorkloadPercentage = i.Sum(g => g.CurrentWorkloadPercentage),
                                       UpToEndWorkloadPercentage = i.Sum(g => g.UpToEndWorkloadPercentage),
                                       Passed = i.Sum(g => g.Passed),
                                       Failed = i.Sum(g => g.Failed),
                                       Fixed = i.Sum(g => g.Fixed),
                                       Open = i.Sum(g => g.Open),
                                       Pending = i.Sum(g => g.Pending),
                                       Hold = i.Sum(g => g.Hold),
                                       Canceled = i.Sum(g => g.Canceled),
                                   })
                                   on r.TesterName equals rd.TesterName into groupResourceDate
                                    from rdl in groupResourceDate.DefaultIfEmpty()
                                    select new ResourceWorkloadAccumulation()
                                    {
                                        TesterName = r.TesterName,
                                        TotalPlannedDays = r.TotalPlannedDays,
                                        RemainingPendingDays = r.RemainingPendingDays,
                                        RemainingWorkingDays = r.RemainingWorkingDays,
                                        CurrentWorkloadPercentage = rdl != null ? decimal.Round(rdl.CurrentWorkloadPercentage, 0) : 0,
                                        UpToEndWorkloadPercentage = rdl != null ? decimal.Round(rdl.UpToEndWorkloadPercentage, 0) : 0,
                                        Passed = rdl != null ? rdl.Passed : 0,
                                        Canceled = rdl != null ? rdl.Canceled : 0,
                                        Failed = rdl != null ? rdl.Failed : 0,
                                        Fixed = rdl != null ? rdl.Fixed : 0,
                                        Open = rdl != null ? rdl.Open : 0,
                                        Pending = rdl != null ? rdl.Pending : 0,
                                        Hold = rdl != null ? rdl.Hold : 0
                                    }).OrderByDescending(i => i.DonePercentage).ThenBy(i => i.TesterName).ToList();
            report.ResourceWorkloadAccumulation = resourceWorkload;

            if (Action == "Export")
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/template/logiz.radar.test-report.xlsx");
                var fi = new FileInfo(filePath);
                using (var ep = new ExcelPackage(fi))
                {
                    //Project Summary
                    var ps = ep.Workbook.Worksheets["PS"];
                    ps.Cells[2, 1].Value = DateTime.Now;
                    ps.Cells[4, 1].Value = report.StartDate;
                    ps.Cells[4, 2].Value = report.EndDate;
                    ps.Cells[4, 3].Value = report.TestedPercentage / 100;
                    ps.Cells[4, 4].Value = report.DonePercentage / 100;
                    ps.Cells[4, 5].Value = report.Total;
                    ps.Cells[4, 6].Value = report.Passed;
                    ps.Cells[4, 7].Value = report.Canceled;
                    ps.Cells[4, 8].Value = report.Failed;
                    ps.Cells[4, 9].Value = report.Fixed;
                    ps.Cells[4, 10].Value = report.Pending;
                    ps.Cells[4, 11].Value = report.Open;
                    ps.Cells[4, 12].Value = report.Hold;
                    ps.Cells[5, 6].Value = report.PassedPercentage / 100;
                    ps.Cells[5, 7].Value = report.CanceledPercentage / 100;
                    ps.Cells[5, 8].Value = report.FailedPercentage / 100;
                    ps.Cells[5, 9].Value = report.FixedPercentage / 100;
                    ps.Cells[5, 10].Value = report.PendingPercentage / 100;
                    ps.Cells[5, 11].Value = report.OpenPercentage / 100;
                    ps.Cells[5, 12].Value = report.HoldPercentage / 100;

                    //Scenario Summary
                    var ss = ep.Workbook.Worksheets["SS"];
                    ss.Cells[2, 1].Value = DateTime.Now;
                    int i = 3;
                    foreach (var s in report.ReportByScenario)
                    {
                        i++;
                        ss.Cells[i, 1].Value = s.ScenarioName;
                        ss.Cells[i, 2].Value = s.StartDate;
                        ss.Cells[i, 3].Value = s.EndDate;
                        ss.Cells[i, 4].Value = s.TestedPercentage / 100;
                        ss.Cells[i, 5].Value = s.DonePercentage / 100;
                        ss.Cells[i, 6].Value = s.Total;
                        ss.Cells[i, 7].Value = s.Passed;
                        ss.Cells[i, 8].Value = s.Canceled;
                        ss.Cells[i, 9].Value = s.Failed;
                        ss.Cells[i, 10].Value = s.Fixed;
                        ss.Cells[i, 11].Value = s.Pending;
                        ss.Cells[i, 12].Value = s.Open;
                        ss.Cells[i, 13].Value = s.Hold;
                    }

                    //Planned Date Daily
                    var pd = ep.Workbook.Worksheets["PD"];
                    pd.Cells[2, 1].Value = DateTime.Now;
                    i = 3;
                    foreach (var p in report.ReportByPlannedDate)
                    {
                        i++;
                        pd.Cells[i, 1].Value = p.PlannedDate;
                        pd.Cells[i, 2].Value = p.CurrentWorkloadPercentage / 100;
                        pd.Cells[i, 3].Value = p.UpToEndWorkloadPercentage / 100;
                        pd.Cells[i, 4].Value = p.Total;
                        pd.Cells[i, 5].Value = p.Passed;
                        pd.Cells[i, 6].Value = p.Canceled;
                        pd.Cells[i, 7].Value = p.Failed;
                        pd.Cells[i, 8].Value = p.Fixed;
                        pd.Cells[i, 9].Value = p.Pending;
                        pd.Cells[i, 10].Value = p.Open;
                        pd.Cells[i, 11].Value = p.Hold;
                    }

                    //Planned Date Accumulation
                    var pa = ep.Workbook.Worksheets["PA"];
                    pa.Cells[2, 1].Value = DateTime.Now;
                    i = 3;
                    foreach (var p in report.ReportByPlannedDateAccumulation)
                    {
                        i++;
                        pa.Cells[i, 1].Value = p.PlannedDate;
                        pa.Cells[i, 2].Value = p.CurrentWorkloadPercentage / 100;
                        pa.Cells[i, 3].Value = p.UpToEndWorkloadPercentage / 100;
                        pa.Cells[i, 4].Value = p.Total;
                        pa.Cells[i, 5].Value = p.Passed;
                        pa.Cells[i, 6].Value = p.Canceled;
                        pa.Cells[i, 7].Value = p.Failed;
                        pa.Cells[i, 8].Value = p.Fixed;
                        pa.Cells[i, 9].Value = p.Pending;
                        pa.Cells[i, 10].Value = p.Open;
                        pa.Cells[i, 11].Value = p.Hold;
                    }

                    //Resource Summary
                    var rs = ep.Workbook.Worksheets["RS"];
                    rs.Cells[2, 1].Value = DateTime.Now;
                    i = 3;
                    foreach (var r in report.TestReportByResourceSummary)
                    {
                        i++;
                        rs.Cells[i, 1].Value = r.TesterName;
                        rs.Cells[i, 2].Value = r.StartDate;
                        rs.Cells[i, 3].Value = r.EndDate;
                        rs.Cells[i, 4].Value = r.RemainingWorkingDays;
                        rs.Cells[i, 5].Value = r.RemainingPendingDays;
                        rs.Cells[i, 6].Value = r.TotalPlannedDays;
                        rs.Cells[i, 7].Value = r.TestedPercentage / 100;
                        rs.Cells[i, 8].Value = r.DonePercentage / 100;
                        rs.Cells[i, 9].Value = r.Total;
                        rs.Cells[i, 10].Value = r.Passed;
                        rs.Cells[i, 11].Value = r.Canceled;
                        rs.Cells[i, 12].Value = r.Failed;
                        rs.Cells[i, 13].Value = r.Fixed;
                        rs.Cells[i, 14].Value = r.Pending;
                        rs.Cells[i, 15].Value = r.Open;
                        rs.Cells[i, 16].Value = r.Hold;
                    }

                    //Current Resource Workload
                    var rc = ep.Workbook.Worksheets["RW"];
                    rc.Cells[2, 1].Value = DateTime.Now;
                    i = 3;
                    foreach (var r in report.ResourceWorkloadAccumulation)
                    {
                        i++;
                        rc.Cells[i, 1].Value = r.TesterName;
                        rc.Cells[i, 2].Value = r.CurrentWorkloadPercentage / 100;
                        rc.Cells[i, 3].Value = r.UpToEndWorkloadPercentage / 100;
                        rc.Cells[i, 4].Value = r.RemainingWorkingDays;
                        rc.Cells[i, 5].Value = r.RemainingPendingDays;
                        rc.Cells[i, 6].Value = r.TotalPlannedDays;
                        rc.Cells[i, 7].Value = r.TestedPercentage / 100;
                        rc.Cells[i, 8].Value = r.DonePercentage / 100;
                        rc.Cells[i, 9].Value = r.Total;
                        rc.Cells[i, 10].Value = r.Passed;
                        rc.Cells[i, 11].Value = r.Canceled;
                        rc.Cells[i, 12].Value = r.Failed;
                        rc.Cells[i, 13].Value = r.Fixed;
                        rc.Cells[i, 14].Value = r.Pending;
                        rc.Cells[i, 15].Value = r.Open;
                        rc.Cells[i, 16].Value = r.Hold;
                    }

                    string fileNameToExport = $"logiz.radar.test-report_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
                    return File(ep.GetAsByteArray(), "application/excel", fileNameToExport);
                }
            }

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
