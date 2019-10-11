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
using Microsoft.AspNetCore.Http;
using System.IO;
using OfficeOpenXml;

namespace Logiz.Radar.Controllers
{
    public static class TestStatuses
    {
        public const string Passed = "Passed";
        public const string Failed = "Failed";
        public const string Fixed = "Fixed";
        public const string Open = "Open";
        public const string Pending = "Pending";
        public const string Hold = "Hold";
        public const string Canceled = "Canceled";

        public static string GetTestStatusesString()
        {
            return $"{Passed}, {Failed}, {Fixed}, {Open}, {Pending}, {Hold}, {Canceled}";
        }

        public static List<string> GetTestStatusesList()
        {
            var testStatusList = new List<string>() {
                TestStatuses.Passed,
                TestStatuses.Failed,
                TestStatuses.Fixed,
                TestStatuses.Open,
                TestStatuses.Pending,
                TestStatuses.Hold,
                TestStatuses.Canceled
            };
            return testStatusList;
        }
    }

    [Authorize]
    public class TestCaseController : Controller
    {
        private readonly RadarContext _context;

        public TestCaseController(RadarContext context)
        {
            _context = context;
        }

        // GET: TestCase
        public async Task<IActionResult> Index(string ProjectID, string ScenarioID, string VariantID, string TesterName, DateTime? FromPlannedDate, DateTime? ToPlannedDate, string TestStatus, DateTime? FromUpdatedDate, DateTime? ToUpdatedDate, string Action)
        {
            ViewBag.ProjectID = ProjectID;
            ViewBag.ScenarioID = ScenarioID;
            ViewBag.VariantID = VariantID;
            ViewBag.TesterName = TesterName;
            ViewBag.FromPlannedDate = FromPlannedDate?.ToString("yyyy-MM-dd");
            ViewBag.ToPlannedDate = ToPlannedDate?.ToString("yyyy-MM-dd");
            ViewBag.FromUpdatedDate = FromUpdatedDate?.ToString("yyyy-MM-ddTHH:mm:ss.sss");
            ViewBag.ToUpdatedDate = ToUpdatedDate?.ToString("yyyy-MM-ddTHH:mm:ss.sss");
            ViewBag.TestStatus = TestStatus;
            ViewBag.CanWrite = CanWrite(User.Identity.Name, ProjectID);

            var caseList = await (from userProject in _context.UserMappingProject.Where(i => i.Username.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                                  join scenario in _context.TestScenario.Where(i => i.ProjectID.Equals(ProjectID, StringComparison.OrdinalIgnoreCase))
                                  on userProject.ProjectID equals scenario.ProjectID
                                  join variant in _context.TestVariant.Where(i => string.IsNullOrEmpty(ScenarioID) || i.ScenarioID.Equals(ScenarioID, StringComparison.OrdinalIgnoreCase))
                                  on scenario.ID equals variant.ScenarioID
                                  join testCase in _context.TestCase.Where(i => (string.IsNullOrEmpty(VariantID) || i.TestVariantID.Equals(VariantID, StringComparison.OrdinalIgnoreCase))
                                  && (!FromPlannedDate.HasValue || i.PlannedDate >= FromPlannedDate)
                                  && (!ToPlannedDate.HasValue || i.PlannedDate <= ToPlannedDate)
                                  && (!FromUpdatedDate.HasValue || i.UpdatedDateTime >= FromUpdatedDate)
                                  && (!ToUpdatedDate.HasValue || i.UpdatedDateTime <= ToUpdatedDate)
                                  && (string.IsNullOrEmpty(TestStatus) || i.TestStatus.Equals(TestStatus, StringComparison.OrdinalIgnoreCase))
                                  && (string.IsNullOrEmpty(TesterName) || i.TesterName.Contains(TesterName, StringComparison.OrdinalIgnoreCase)))
                                  on variant.ID equals testCase.TestVariantID
                                  join attachment in _context.TestCaseAttachment.Select(i => new { i.TestCaseID }).Distinct()
                                  on testCase.ID equals attachment.TestCaseID into groupAttachment
                                  from leftAttachment in groupAttachment.DefaultIfEmpty()
                                  orderby testCase.PlannedDate, testCase.TestCaseName
                                  select new TestCaseViewModel
                                  {
                                      ScenarioID = scenario.ID,
                                      ScenarioName = scenario.ScenarioName,
                                      VariantName = variant.VariantName,
                                      HasAttachment = leftAttachment != null ? true : false,
                                      TestCase = new TestCase()
                                      {
                                          ID = testCase.ID,
                                          TestVariantID = testCase.TestVariantID,
                                          TestCaseName = testCase.TestCaseName,
                                          TestCaseSteps = testCase.TestCaseSteps,
                                          ExpectedResult = testCase.ExpectedResult,
                                          ActualResult = testCase.ActualResult,
                                          TesterName = testCase.TesterName,
                                          PlannedDate = testCase.PlannedDate,
                                          TestStatus = testCase.TestStatus,
                                          UpdatedBy = testCase.UpdatedBy,
                                          UpdatedDateTime = testCase.UpdatedDateTime
                                      }
                                  }).ToListAsync();

            if (Action == "Export")
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/template/logiz.radar.test-case.xlsx");
                var scenario = await _context.TestScenario.FirstOrDefaultAsync(i => i.ID.Equals(ScenarioID, StringComparison.OrdinalIgnoreCase));
                var fi = new FileInfo(filePath);
                using (var p = new ExcelPackage(fi))
                {
                    var ws = p.Workbook.Worksheets["TestCase"];
                    int totalCases = caseList.Count;
                    for (int i = 0; i < totalCases; i++)
                    {
                        ws.Cells[i + 2, 1].Value = caseList[i].VariantName;
                        ws.Cells[i + 2, 2].Value = caseList[i].TestCase.TestCaseName;
                        ws.Cells[i + 2, 3].Value = caseList[i].TestCase.TestCaseSteps;
                        ws.Cells[i + 2, 4].Value = caseList[i].TestCase.ExpectedResult;
                        ws.Cells[i + 2, 5].Value = caseList[i].TestCase.ActualResult;
                        ws.Cells[i + 2, 6].Value = caseList[i].TestCase.TestStatus;
                        ws.Cells[i + 2, 7].Value = caseList[i].TestCase.TesterName;
                        ws.Cells[i + 2, 8].Value = caseList[i].TestCase.PlannedDate;
                        ws.Cells[i + 2, 9].Value = caseList[i].ScenarioID;
                        ws.Cells[i + 2, 10].Value = caseList[i].TestCase.ID;
                        //Column 11 for system validation
                        ws.Cells[i + 2, 12].Value = caseList[i].ScenarioName;
                        ws.Cells[i + 2, 13].Value = caseList[i].HasAttachment;
                        ws.Cells[i + 2, 14].Value = caseList[i].TestCase.CreatedBy;
                        ws.Cells[i + 2, 15].Value = caseList[i].TestCase.CreatedDateTime;
                        ws.Cells[i + 2, 16].Value = caseList[i].TestCase.UpdatedBy;
                        ws.Cells[i + 2, 17].Value = caseList[i].TestCase.UpdatedDateTime;
                    }
                    string fileNameToExport = "logiz.radar.test-case_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
                    return File(p.GetAsByteArray(), "application/excel", fileNameToExport);
                }
            }

            return View(caseList);
        }

        // GET: TestCase/Create
        public async Task<IActionResult> Create(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var clonedTestCase = await (from tc in _context.TestCase.Where(i => i.ID.Equals(id, StringComparison.OrdinalIgnoreCase))
                                            join variant in _context.TestVariant
                                            on tc.TestVariantID equals variant.ID
                                            join scenario in _context.TestScenario
                                            on variant.ScenarioID equals scenario.ID
                                            select new TestCaseViewModel() { ProjectID = scenario.ProjectID, ScenarioID = scenario.ID, TestCase = tc }).FirstOrDefaultAsync();

                if (clonedTestCase != null)
                {
                    clonedTestCase.TestCase.SetCreator(User.Identity.Name);
                    return View(clonedTestCase);
                }
            }

            TestCaseViewModel newCase = new TestCaseViewModel()
            {
                ProjectID = string.Empty,
                ScenarioID = string.Empty,
                TestCase = new TestCase()
                {
                    TesterName = "?",
                    PlannedDate = new DateTime(2000, 1, 1),
                    TestStatus = TestStatuses.Open
                }
            };
            newCase.TestCase.SetCreator(User.Identity.Name);

            return View(newCase);
        }

        // POST: TestCase/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string ProjectID, string ScenarioID, [Bind("TestCaseName,TestVariantID,TestCaseSteps,ExpectedResult,ActualResult,TesterName,PlannedDate,TestStatus,Note,ID,CreatedBy,CreatedDateTime,UpdatedBy,UpdatedDateTime,IsActive")] TestCase testCase)
        {
            DateTime minDate = new DateTime(2000, 1, 1);
            if (testCase?.PlannedDate < minDate)
            {
                ModelState.AddModelError("TestCase.PlannedDate", $"PlannedDate must be >= {minDate.ToString("yyyy/MM/dd")}");
            }

            if (ModelState.IsValid)
            {
                testCase.SetCreator(User.Identity.Name);
                _context.Add(testCase);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { ProjectID = ProjectID, ScenarioID = ScenarioID, VariantID = testCase.TestVariantID });
            }

            TestCaseViewModel newCase = new TestCaseViewModel()
            {
                ProjectID = ProjectID,
                ScenarioID = ScenarioID,
                TestCase = testCase
            };

            return View(newCase);
        }

        // GET: TestCase/Edit/5
        public async Task<IActionResult> Edit(string id, string TesterName, DateTime? FromPlannedDate, DateTime? ToPlannedDate, string SearchTestStatus, DateTime? FromUpdatedDate, DateTime? ToUpdatedDate)
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

            if (!CanWrite(User.Identity.Name, testVariant.ProjectID))
            {
                return Forbid();
            }

            var attachments = await _context.TestCaseAttachment.Where(i => i.TestCaseID.Equals(id, StringComparison.OrdinalIgnoreCase)).ToListAsync();

            var testCaseViewModel = new TestCaseViewModel()
            {
                ProjectID = testVariant.ProjectID,
                ScenarioID = testVariant.ScenarioID,
                TesterName = TesterName,
                FromPlannedDate = FromPlannedDate,
                ToPlannedDate = ToPlannedDate,
                SearchTestStatus = SearchTestStatus,
                FromUpdatedDate = FromUpdatedDate,
                ToUpdatedDate = ToUpdatedDate,
                TestCase = testCase,
                TestCaseAttachments = attachments
            };
            return View(testCaseViewModel);
        }

        // POST: TestCase/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string ProjectID, string ScenarioID, string TesterName, DateTime? FromPlannedDate, DateTime? ToPlannedDate, string SearchTestStatus, DateTime? FromUpdatedDate, DateTime? ToUpdatedDate, List<IFormFile> files,
            List<TestCaseAttachment> TestCaseAttachments,
            [Bind("TestCaseName,TestVariantID,TestCaseSteps,ExpectedResult,ActualResult,TesterName,PlannedDate,TestStatus,Note,ID,CreatedBy,CreatedDateTime,UpdatedBy,UpdatedDateTime,IsActive")] TestCase testCase)
        {
            if (id != testCase.ID)
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

            DateTime minDate = new DateTime(2000, 1, 1);
            if (testCase?.PlannedDate < minDate)
            {
                ModelState.AddModelError("TestCase.PlannedDate", $"PlannedDate must be >= {minDate.ToString("yyyy/MM/dd")}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    testCase.SetUpdater(User.Identity.Name);
                    _context.Update(testCase);
                    if (files.Count > 0)
                    {
                        List<TestCaseAttachment> attachments = new List<TestCaseAttachment>();
                        string subFolder = Path.Combine("datafile/testCaseAttachment/", ProjectID, ScenarioID, id);
                        string fullFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", subFolder);
                        if (!Directory.Exists(fullFolder))
                        {
                            Directory.CreateDirectory(fullFolder);
                        }
                        foreach (var formFile in files)
                        {
                            if (formFile.Length > 0)
                            {
                                string filename = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);
                                string fullPath = Path.Combine(fullFolder, filename);
                                string filePath = Path.Combine(subFolder, filename);
                                using (var stream = new FileStream(fullPath, FileMode.Create))
                                {
                                    await formFile.CopyToAsync(stream);
                                    var attachment = new TestCaseAttachment()
                                    {
                                        TestCaseID = testCase.ID,
                                        OriginalFileName = formFile.FileName,
                                        FullFileName = filePath,
                                        ContentType = formFile.ContentType
                                    };
                                    attachment.SetCreator(User.Identity.Name);
                                    attachments.Add(attachment);
                                }
                            }
                        }
                        _context.TestCaseAttachment.AddRange(attachments);
                    }
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
                return RedirectToAction(nameof(Index), new
                {
                    ProjectID = ProjectID,
                    ScenarioID = ScenarioID,
                    VariantID = testCase.TestVariantID,
                    TesterName = TesterName,
                    FromPlannedDate = FromPlannedDate,
                    ToPlannedDate = ToPlannedDate,
                    TestStatus = SearchTestStatus,
                    FromUpdatedDate = FromUpdatedDate,
                    ToUpdatedDate = ToUpdatedDate
                });
            }
            var testCaseViewModel = new TestCaseViewModel()
            {
                ProjectID = ProjectID,
                ScenarioID = ScenarioID,
                TesterName = TesterName,
                FromPlannedDate = FromPlannedDate,
                ToPlannedDate = ToPlannedDate,
                SearchTestStatus = SearchTestStatus,
                FromUpdatedDate = FromUpdatedDate,
                ToUpdatedDate = ToUpdatedDate,
                TestCase = testCase,
                TestCaseAttachments = TestCaseAttachments
            };
            return View(testCaseViewModel);
        }

        [AllowAnonymous]
        public async Task<IActionResult> DownloadAttachment(string id)
        {
            var attachment = await _context.TestCaseAttachment.FirstOrDefaultAsync(i => i.ID == id);

            if (attachment == null)
            {
                return NotFound();
            }

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.FullFileName);

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }

            memory.Position = 0;
            return File(memory, attachment.ContentType, attachment.OriginalFileName);
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

            var testVariant = await (from variant in _context.TestVariant.Where(i => i.ID.Equals(testCase.TestVariantID, StringComparison.OrdinalIgnoreCase))
                                     join scenario in _context.TestScenario
                                     on variant.ScenarioID equals scenario.ID
                                     select new { ProjectID = scenario.ProjectID, ScenarioID = scenario.ID }).FirstOrDefaultAsync();

            if (!CanWrite(User.Identity.Name, testVariant?.ProjectID))
            {
                return Forbid();
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

            if (testCase == null)
            {
                return NotFound();
            }

            var testVariant = await (from variant in _context.TestVariant.Where(i => i.ID.Equals(testCase.TestVariantID, StringComparison.OrdinalIgnoreCase))
                                     join scenario in _context.TestScenario
                                     on variant.ScenarioID equals scenario.ID
                                     select new { ProjectID = scenario.ProjectID, ScenarioID = scenario.ID }).FirstOrDefaultAsync();

            if (!CanWrite(User.Identity.Name, testVariant?.ProjectID))
            {
                return Forbid();
            }

            if (await _context.TestCaseAttachment.AnyAsync(i => i.TestCaseID.Equals(testCase.ID, StringComparison.OrdinalIgnoreCase)))
            {
                return RedirectToAction("Error", "Home", new { errorMessage = "All attachments of this test case must be deleted first." });
            }

            _context.TestCase.Remove(testCase);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { ProjectID = testVariant?.ProjectID, ScenarioID = testVariant?.ScenarioID, VariantID = testCase.TestVariantID });
        }

        // GET: TestCase/View/5
        [AllowAnonymous]
        public async Task<IActionResult> View(string id, string TesterName, DateTime? FromPlannedDate, DateTime? ToPlannedDate, string SearchTestStatus, DateTime? FromUpdatedDate, DateTime? ToUpdatedDate)
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
                                     join project in _context.Project
                                     on scenario.ProjectID equals project.ID
                                     select new
                                     {
                                         ProjectID = scenario.ProjectID,
                                         ProjectName = project.ProjectName,
                                         ScenarioID = scenario.ID,
                                         ScenarioName = scenario.ScenarioName,
                                         VariantID = variant.ID,
                                         VariantName = variant.VariantName
                                     }).FirstOrDefaultAsync();

            if (testVariant == null)
            {
                return NotFound();
            }

            //if (!AuthorizeData(id))
            //{
            //    return Unauthorized();
            //}

            var attachments = await _context.TestCaseAttachment.Where(i => i.TestCaseID.Equals(id, StringComparison.OrdinalIgnoreCase)).ToListAsync();

            var testCaseViewModel = new TestCaseViewModel()
            {
                ProjectID = testVariant.ProjectID,
                ProjectName = testVariant.ProjectName,
                ScenarioID = testVariant.ScenarioID,
                ScenarioName = testVariant.ScenarioName,
                VariantName = testVariant.VariantName,
                TesterName = TesterName,
                FromPlannedDate = FromPlannedDate,
                ToPlannedDate = ToPlannedDate,
                SearchTestStatus = SearchTestStatus,
                FromUpdatedDate = FromUpdatedDate,
                ToUpdatedDate = ToUpdatedDate,
                TestCase = testCase,
                TestCaseAttachments = attachments
            };
            ViewBag.CanWrite = CanWrite(User.Identity.Name, testVariant.ProjectID);
            return View(testCaseViewModel);
        }

        // GET: TestCase/DeleteAttachment/5
        public async Task<IActionResult> DeleteAttachment(string id, string TesterName, DateTime? FromPlannedDate, DateTime? ToPlannedDate, string SearchTestStatus, DateTime? FromUpdatedDate, DateTime? ToUpdatedDate)
        {
            if (id == null)
            {
                return NotFound();
            }

            var testCaseAttachment = await _context.TestCaseAttachment
                .FirstOrDefaultAsync(m => m.ID == id);
            if (testCaseAttachment == null)
            {
                return NotFound();
            }

            if (!AuthorizeData(testCaseAttachment.TestCaseID))
            {
                return Unauthorized();
            }

            var testVariant = await (from attachement in _context.TestCaseAttachment.Where(i => i.ID.Equals(id, StringComparison.OrdinalIgnoreCase))
                                     join testCase in _context.TestCase
                                     on attachement.TestCaseID equals testCase.ID
                                     join variant in _context.TestVariant
                                     on testCase.TestVariantID equals variant.ID
                                     join scenario in _context.TestScenario
                                     on variant.ScenarioID equals scenario.ID
                                     select new { ProjectID = scenario.ProjectID, ScenarioID = scenario.ID }).FirstOrDefaultAsync();

            if (!CanWrite(User.Identity.Name, testVariant?.ProjectID))
            {
                return Forbid();
            }

            var testCaseAttachmentViewModel = new TestCaseAttachmentViewModel()
            {
                TesterName = TesterName,
                FromPlannedDate = FromPlannedDate,
                ToPlannedDate = ToPlannedDate,
                SearchTestStatus = SearchTestStatus,
                FromUpdatedDate = FromUpdatedDate,
                ToUpdatedDate = ToUpdatedDate,
                TestCaseAttachment = testCaseAttachment
            };

            return View(testCaseAttachmentViewModel);
        }

        // POST: TestCase/DeleteAttachmentConfirmed/5
        [HttpPost, ActionName("DeleteAttachment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttachmentConfirmed(string id, string TesterName, DateTime? FromPlannedDate, DateTime? ToPlannedDate, string SearchTestStatus, DateTime? FromUpdatedDate, DateTime? ToUpdatedDate)
        {
            var testCaseAttachment = await _context.TestCaseAttachment.FindAsync(id);

            if (testCaseAttachment == null)
            {
                return NotFound();
            }

            if (!AuthorizeData(testCaseAttachment.TestCaseID))
            {
                return Unauthorized();
            }

            var testVariant = await (from attachement in _context.TestCaseAttachment.Where(i => i.ID.Equals(id, StringComparison.OrdinalIgnoreCase))
                                     join testCase in _context.TestCase
                                     on attachement.TestCaseID equals testCase.ID
                                     join variant in _context.TestVariant
                                     on testCase.TestVariantID equals variant.ID
                                     join scenario in _context.TestScenario
                                     on variant.ScenarioID equals scenario.ID
                                     select new TestCaseViewModel() { ProjectID = scenario.ProjectID, ScenarioID = scenario.ID, TestCase = testCase }).FirstOrDefaultAsync();
            if (!CanWrite(User.Identity.Name, testVariant?.ProjectID))
            {
                return Forbid();
            }

            _context.TestCaseAttachment.Remove(testCaseAttachment);
            testVariant.TestCase.SetUpdater(User.Identity.Name);
            _context.Update(testVariant.TestCase);
            await _context.SaveChangesAsync();
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", testCaseAttachment.FullFileName);
            System.IO.File.Delete(filePath);
            return RedirectToAction(nameof(Edit), new
            {
                id = testCaseAttachment.TestCaseID,
                TesterName = TesterName,
                FromPlannedDate = FromPlannedDate,
                ToPlannedDate = ToPlannedDate,
                SearchTestStatus = SearchTestStatus,
                FromUpdatedDate = FromUpdatedDate,
                ToUpdatedDate = ToUpdatedDate
            });
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

        [HttpGet]
        public JsonResult GetTestStatusSelectList()
        {
            var statusList = new List<SelectListItem>();
            var list = TestStatuses.GetTestStatusesList();
            foreach (var s in list)
            {
                statusList.Add(new SelectListItem()
                {
                    Value = s,
                    Text = s
                });
            }
            var statusSelectList = new SelectList(statusList, "Value", "Text");
            return Json(statusSelectList);
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

        // GET: TestCase/Import
        public IActionResult Import()
        {
            return View();
        }

        // POST: TestCase/Import
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import([Bind("ProjectID, DataFile")] ImportTestCaseViewModel importModel)
        {
            if (ModelState.IsValid)
            {
                if (!CanWrite(User.Identity.Name, importModel.ProjectID))
                {
                    return Forbid();
                }

                using (var stream = importModel.DataFile.OpenReadStream())
                {
                    using (var excelPackage = new ExcelPackage(stream))
                    {
                        var workSheet = excelPackage.Workbook.Worksheets["TestCase"];

                        if (workSheet == null)
                        {
                            ModelState.AddModelError("", "Sheet 'TestCase' is not found.");
                            return View(importModel);
                        }

                        int rowCount = workSheet.Dimension.Rows + 1;
                        List<TestCaseViewModel> inputTestCaseList = new List<TestCaseViewModel>();

                        for (int i = 2; i < rowCount; i++)
                        {
                            var inputTestCase = new TestCaseViewModel()
                            {
                                Index = i,
                                ScenarioID = workSheet.Cells[i, 9].GetValue<string>(),
                                VariantName = workSheet.Cells[i, 1].GetValue<string>(),
                                TestCase = new TestCase()
                                {
                                    ID = workSheet.Cells[i, 10].GetValue<string>(),
                                    TestCaseName = workSheet.Cells[i, 2].GetValue<string>(),
                                    TestCaseSteps = workSheet.Cells[i, 3].GetValue<string>(),
                                    ExpectedResult = workSheet.Cells[i, 4].GetValue<string>(),
                                    ActualResult = workSheet.Cells[i, 5].GetValue<string>(),
                                    TestStatus = workSheet.Cells[i, 6].GetValue<string>(),
                                    TesterName = workSheet.Cells[i, 7].GetValue<string>(),
                                    PlannedDate = workSheet.Cells[i, 8].GetValue<DateTime>().Date
                                }
                            };
                            inputTestCaseList.Add(inputTestCase);
                        }

                        int errorCount = 0;
                        var newVariantList = new List<TestVariant>();
                        var newTestCaseList = new List<TestCase>();
                        var updatedTestCaseList = new List<TestCase>();
                        var inputScenarioIDList = inputTestCaseList.Select(i => new { i.ScenarioID }).Distinct().ToList();
                        var existingScenarioList = await (from s in _context.TestScenario.Where(i => i.ProjectID.Equals(importModel.ProjectID, StringComparison.OrdinalIgnoreCase))
                                                          join iS in inputScenarioIDList
                                                          on s.ID equals iS.ScenarioID
                                                          select s).ToListAsync();
                        var inputVariantIDList = inputTestCaseList.Select(i => new { i.ScenarioID, i.VariantName }).Distinct().ToList();
                        var existingVariantList = await (from v in _context.TestVariant
                                                         join iS in existingScenarioList
                                                         on v.ScenarioID equals iS.ID
                                                         join iv in inputVariantIDList
                                                         on new { v.ScenarioID, v.VariantName } equals new { iv.ScenarioID, iv.VariantName }
                                                         select v).ToListAsync();
                        var inputTestCaseIDList = inputTestCaseList.Select(i => new { i.TestCase.ID }).Distinct().ToList();
                        var existingTestCaseList = await (from c in _context.TestCase
                                                          join iv in existingVariantList
                                                          on c.TestVariantID equals iv.ID
                                                          join ic in inputTestCaseIDList
                                                          on c.ID equals ic.ID
                                                          select c).ToListAsync();

                        foreach (var inputTestCase in inputTestCaseList)
                        {
                            string errorMessage = string.Empty;

                            if (string.IsNullOrEmpty(inputTestCase.VariantName))
                            {
                                errorCount++;
                                errorMessage += "TestVariant is required. ";
                            }

                            if (string.IsNullOrEmpty(inputTestCase.TestCase.TestCaseName))
                            {
                                errorCount++;
                                errorMessage += "TestCaseName is required. ";
                            }

                            if (string.IsNullOrEmpty(inputTestCase.TestCase.TestCaseSteps))
                            {
                                errorCount++;
                                errorMessage += "TestCaseSteps is required. ";
                            }

                            if (string.IsNullOrEmpty(inputTestCase.TestCase.ExpectedResult))
                            {
                                errorCount++;
                                errorMessage += "ExpectedResult is required. ";
                            }

                            if (string.IsNullOrEmpty(inputTestCase.TestCase.TesterName))
                            {
                                errorCount++;
                                errorMessage += "TesterName is required. ";
                            }

                            var minDate = new DateTime(2000, 1, 1);
                            if (inputTestCase.TestCase.PlannedDate < minDate)
                            {
                                errorCount++;
                                errorMessage += $"PlannedDate must be a Date value >= {minDate.ToString("yyyy/MM/dd")}. ";
                            }

                            if (!TestStatuses.GetTestStatusesList().Contains(inputTestCase.TestCase.TestStatus))
                            {
                                errorCount++;
                                errorMessage += $"TestStatus must be {TestStatuses.GetTestStatusesString()}. ";
                            }

                            if (string.IsNullOrEmpty(inputTestCase.ScenarioID))
                            {
                                errorCount++;
                                errorMessage += "ScenarioID is required. ";
                            }
                            else
                            {
                                if (!existingScenarioList.Any(c => c.ID.Equals(inputTestCase.ScenarioID, StringComparison.OrdinalIgnoreCase)))
                                {
                                    errorCount++;
                                    errorMessage += "ScenarioID is invalid. ";
                                }
                            }

                            TestCase updatedTestCase = null;
                            if (!string.IsNullOrEmpty(inputTestCase.TestCase.ID))
                            {
                                updatedTestCase = existingTestCaseList.FirstOrDefault(c => c.ID.Equals(inputTestCase.TestCase.ID, StringComparison.OrdinalIgnoreCase));
                                if (updatedTestCase == null)
                                {
                                    errorCount++;
                                    errorMessage += "TestCaseID is invalid. ";
                                }
                            }

                            if (!string.IsNullOrEmpty(errorMessage))
                            {
                                workSheet.Cells[inputTestCase.Index, 11].Value = errorMessage;
                            }
                            else
                            {
                                var variant = existingVariantList.FirstOrDefault(v => v.VariantName.Equals(inputTestCase.VariantName, StringComparison.OrdinalIgnoreCase)
                                                                                    && v.ScenarioID.Equals(inputTestCase.ScenarioID, StringComparison.OrdinalIgnoreCase));
                                if (variant == null)
                                {
                                    variant = newVariantList.FirstOrDefault(v => v.VariantName.Equals(inputTestCase.VariantName, StringComparison.OrdinalIgnoreCase)
                                                                                    && v.ScenarioID.Equals(inputTestCase.ScenarioID, StringComparison.OrdinalIgnoreCase));
                                    if (variant == null)
                                    {
                                        variant = new TestVariant()
                                        {
                                            ScenarioID = inputTestCase.ScenarioID,
                                            VariantName = inputTestCase.VariantName
                                        };
                                        variant.SetCreator(User.Identity.Name);
                                        newVariantList.Add(variant);
                                    }
                                }

                                if (updatedTestCase != null)
                                {
                                    updatedTestCase.TestVariantID = variant.ID;
                                    updatedTestCase.TestCaseName = inputTestCase.TestCase.TestCaseName;
                                    updatedTestCase.TestCaseSteps = inputTestCase.TestCase.TestCaseSteps;
                                    updatedTestCase.ExpectedResult = inputTestCase.TestCase.ExpectedResult;
                                    updatedTestCase.ActualResult = inputTestCase.TestCase.ActualResult;
                                    updatedTestCase.TesterName = inputTestCase.TestCase.TesterName;
                                    updatedTestCase.PlannedDate = inputTestCase.TestCase.PlannedDate;
                                    updatedTestCase.TestStatus = inputTestCase.TestCase.TestStatus;
                                    updatedTestCase.SetUpdater(User.Identity.Name);
                                    updatedTestCaseList.Add(updatedTestCase);
                                }
                                else
                                {
                                    var newTestCase = new TestCase()
                                    {
                                        TestVariantID = variant.ID,
                                        TestCaseName = inputTestCase.TestCase.TestCaseName,
                                        TestCaseSteps = inputTestCase.TestCase.TestCaseSteps,
                                        ExpectedResult = inputTestCase.TestCase.ExpectedResult,
                                        ActualResult = inputTestCase.TestCase.ActualResult,
                                        TesterName = inputTestCase.TestCase.TesterName,
                                        PlannedDate = inputTestCase.TestCase.PlannedDate,
                                        TestStatus = inputTestCase.TestCase.TestStatus
                                    };
                                    newTestCase.SetCreator(User.Identity.Name);
                                    newTestCaseList.Add(newTestCase);
                                }
                            }
                        }

                        if (errorCount > 0)
                        {
                            return File(excelPackage.GetAsByteArray(), importModel.DataFile.ContentType, importModel.DataFile.FileName);
                        }

                        await _context.AddRangeAsync(newVariantList);
                        _context.UpdateRange(updatedTestCaseList);
                        await _context.AddRangeAsync(newTestCaseList);
                        await _context.SaveChangesAsync();
                    }
                }
                return RedirectToAction(nameof(Index), new { ProjectID = importModel.ProjectID });
            }
            return View(importModel);
        }
    }
}
