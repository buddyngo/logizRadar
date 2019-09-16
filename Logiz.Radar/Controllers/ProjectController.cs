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
            var projectList = await (from userProject in _context.UserMappingProject.Where(i => i.Username.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                                     join project in _context.Project
                                     on userProject.ProjectID equals project.ID
                                     select new Project
                                     {
                                         ID = project.ID,
                                         ProjectName = project.ProjectName,
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
            if (ModelState.IsValid)
            {
                project.SetCreator(User.Identity.Name);
                _context.Add(project);

                UserMappingProject newMapping = new UserMappingProject()
                {
                    Username = project.CreatedBy,
                    ProjectID = project.ID
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

            var project = await _context.Project.FindAsync(id);
            _context.Project.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(string id)
        {
            return _context.Project.Any(e => e.ID == id);
        }

        [HttpGet]
        public async Task<JsonResult> GetProjectSelectList()
        {
            var projectList = await GetProjectListItems();
            var projectSelectList = new SelectList(projectList, "Value", "Text");
            return Json(projectSelectList);
        }

        public async Task<List<SelectListItem>> GetProjectListItems()
        {
            var projectList = await (from userProject in _context.UserMappingProject.Where(i => i.Username.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
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

            if (!AuthorizeData(id))
            {
                return Unauthorized();
            }

            TestReportProjectViewModel report = new TestReportProjectViewModel();
            report.ProjectID = id;
            report.ProjectName = project.ProjectName;

            report.Open = 1;
            report.Passed = 2;
            report.Failed = 3;
            report.Pending = 4;
            report.Hold = 5;
            report.Total = 15;

            report.ReportByScenario = new List<TestReportByScenario>(){
                new TestReportByScenario(){
                    ScenarioID="1",
                    ScenarioName="Scenario Name - S1",
                    Open = 1,
                    Passed = 2,
                    Failed = 3,
                    Pending = 4,
                    Hold = 5,
                    Total = 15
                },
                new TestReportByScenario(){
                    ScenarioID="1",
                    ScenarioName="Scenario Name - S1",
                    Open = 1,
                    Passed = 2,
                    Failed = 3,
                    Pending = 4,
                    Hold = 5,
                    Total = 15
                },
            };

            report.ReportByPlannedDate = new List<TestReportByPlannedDate>(){
                new TestReportByPlannedDate(){
                    PlannedDate=new DateTime(2019,9,17),
                    Open = 1,
                    Passed = 2,
                    Failed = 3,
                    Pending = 4,
                    Hold = 5,
                    Total = 15
                },
                new TestReportByPlannedDate(){
                    PlannedDate = new DateTime(2019,9,18),
                    Open = 1,
                    Passed = 2,
                    Failed = 3,
                    Pending = 4,
                    Hold = 5,
                    Total = 15
                },
            };

            //var reportSummary = (from testCase in _context.TestCase
            //                     join variant in _context.TestVariant
            //                     on testCase.TestVariantID equals variant.ID
            //                     join scenario in _context.TestScenario.Where(i=> i.ProjectID.Equals(id, StringComparison.OrdinalIgnoreCase))
            //                     on variant.ScenarioID equals scenario.ID
            //                     )

            return View(report);
        }
    }
}
