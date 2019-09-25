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
        public const string Open = "Open";
        public const string Pending = "Pending";
        public const string Hold = "Hold";
        public const string Canceled = "Canceled";
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
        public async Task<IActionResult> Index(string ProjectID, string ScenarioID, string VariantID, string TesterName, DateTime? FromPlannedDate, DateTime? ToPlannedDate, string TestStatus, string Action)
        {
            ViewBag.ProjectID = ProjectID;
            ViewBag.ScenarioID = ScenarioID;
            ViewBag.VariantID = VariantID;
            ViewBag.TesterName = TesterName;
            ViewBag.FromPlannedDate = FromPlannedDate?.ToString("yyyy-MM-dd");
            ViewBag.ToPlannedDate = ToPlannedDate?.ToString("yyyy-MM-dd");
            ViewBag.TestStatus = TestStatus;

            ViewBag.CanWrite = CanWrite(User.Identity.Name, ProjectID);

            if (Action == "Export" && string.IsNullOrEmpty(ScenarioID))
            {
                ModelState.AddModelError("", "ScenarioID is required.");
                return View(new List<TestCase>());
            }

            var caseList = await (from userProject in _context.UserMappingProject.Where(i => i.Username.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                                  join scenario in _context.TestScenario.Where(i => i.ProjectID.Equals(ProjectID, StringComparison.OrdinalIgnoreCase))
                                  on userProject.ProjectID equals scenario.ProjectID
                                  join variant in _context.TestVariant.Where(i => string.IsNullOrEmpty(ScenarioID) || i.ScenarioID.Equals(ScenarioID, StringComparison.OrdinalIgnoreCase))
                                  on scenario.ID equals variant.ScenarioID
                                  join testCase in _context.TestCase.Where(i => (string.IsNullOrEmpty(VariantID) || i.TestVariantID.Equals(VariantID, StringComparison.OrdinalIgnoreCase))
                                  && (!FromPlannedDate.HasValue || i.PlannedDate >= FromPlannedDate)
                                  && (!ToPlannedDate.HasValue || i.PlannedDate <= ToPlannedDate)
                                  && (string.IsNullOrEmpty(TestStatus) || i.TestStatus.Equals(TestStatus, StringComparison.OrdinalIgnoreCase))
                                  && (string.IsNullOrEmpty(TesterName) || i.TesterName.Contains(TesterName, StringComparison.OrdinalIgnoreCase)))
                                  on variant.ID equals testCase.TestVariantID
                                  orderby testCase.PlannedDate, testCase.TestCaseName
                                  select new TestCase
                                  {
                                      ID = testCase.ID,
                                      TestVariantID = variant.VariantName,
                                      TestCaseName = testCase.TestCaseName,
                                      TestCaseSteps = testCase.TestCaseSteps,
                                      ExpectedResult = testCase.ExpectedResult,
                                      ActualResult = testCase.ActualResult,
                                      TesterName = testCase.TesterName,
                                      PlannedDate = testCase.PlannedDate,
                                      TestStatus = testCase.TestStatus,
                                      UpdatedBy = testCase.UpdatedBy,
                                      UpdatedDateTime = testCase.UpdatedDateTime
                                  }).ToListAsync();

            if (Action == "Export")
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/template/logiz.radar.test-case.xlsx");
                var fi = new FileInfo(filePath);
                using (var p = new ExcelPackage(fi))
                {
                    var ws = p.Workbook.Worksheets["TestCase"];
                    int totalCases = caseList.Count;
                    for (int i = 0; i < totalCases; i++)
                    {
                        ws.Cells[i + 2, 1].Value = caseList[i].TestVariantID;
                        ws.Cells[i + 2, 2].Value = caseList[i].TestCaseName;
                        ws.Cells[i + 2, 3].Value = caseList[i].TestCaseSteps;
                        ws.Cells[i + 2, 4].Value = caseList[i].ExpectedResult;
                        ws.Cells[i + 2, 5].Value = caseList[i].ActualResult;
                        ws.Cells[i + 2, 6].Value = caseList[i].TesterName;
                        ws.Cells[i + 2, 7].Value = caseList[i].PlannedDate;
                        ws.Cells[i + 2, 8].Value = caseList[i].TestStatus;
                        ws.Cells[i + 2, 9].Value = caseList[i].ID;
                    }
                    return File(p.GetAsByteArray(), "application/excel", $"logiz.radar.test-case_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");
                }
            }

            return View(caseList);
        }

        // GET: TestCase/Create
        public IActionResult Create()
        {
            TestCase testCase = new TestCase()
            {
                TesterName = "?",
                PlannedDate = new DateTime(2000, 1, 1),
                TestStatus = TestStatuses.Open
            };
            testCase.SetCreator(User.Identity.Name);
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
        public async Task<IActionResult> Edit(string id, string TesterName, DateTime? FromPlannedDate, DateTime? ToPlannedDate, string SearchTestStatus)
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
        public async Task<IActionResult> Edit(string id, string ProjectID, string ScenarioID, string TesterName, DateTime? FromPlannedDate, DateTime? ToPlannedDate, string SearchTestStatus, List<IFormFile> files,
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

            if (ModelState.IsValid)
            {
                try
                {
                    testCase.SetUpdater(User.Identity.Name);
                    _context.Update(testCase);
                    List<TestCaseAttachment> attachments = new List<TestCaseAttachment>();
                    string subFolder = Path.Combine("wwwroot", "datafile", DateTime.Today.ToString("yyyyMMdd"));
                    string rootPath = Path.Combine(Directory.GetCurrentDirectory(), subFolder);
                    if (!Directory.Exists(rootPath))
                    {
                        Directory.CreateDirectory(rootPath);
                    }
                    foreach (var formFile in files)
                    {
                        if (formFile.Length > 0)
                        {
                            string filename = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);
                            string filePath = Path.Combine(rootPath, filename);
                            using (var stream = new FileStream(filePath, FileMode.Create))
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
                    if (attachments.Count > 0)
                    {
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
                return RedirectToAction(nameof(Index), new { ProjectID = ProjectID, ScenarioID = ScenarioID, VariantID = testCase.TestVariantID, TesterName = TesterName, FromPlannedDate = FromPlannedDate, ToPlannedDate = ToPlannedDate, TestStatus = SearchTestStatus });
            }
            var testCaseViewModel = new TestCaseViewModel()
            {
                ProjectID = ProjectID,
                ScenarioID = ScenarioID,
                TesterName = TesterName,
                FromPlannedDate = FromPlannedDate,
                ToPlannedDate = ToPlannedDate,
                SearchTestStatus = SearchTestStatus,
                TestCase = testCase
            };
            return View(testCaseViewModel);
        }

        public async Task<IActionResult> DownloadAttachment(string id)
        {
            var attachment = await _context.TestCaseAttachment.FirstOrDefaultAsync(i => i.ID == id);

            if (attachment == null)
            {
                return NotFound();
            }

            var path = Path.Combine(
                           Directory.GetCurrentDirectory(),
                           "wwwroot", attachment.FullFileName);

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

            _context.TestCase.Remove(testCase);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { ProjectID = testVariant?.ProjectID, ScenarioID = testVariant?.ScenarioID, VariantID = testCase.TestVariantID });
        }

        // GET: TestCase/View/5
        [AllowAnonymous]
        public async Task<IActionResult> View(string id, string TesterName, DateTime? FromPlannedDate, DateTime? ToPlannedDate, string SearchTestStatus)
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
                TestCase = testCase,
                TestCaseAttachments = attachments
            };
            ViewBag.CanWrite = CanWrite(User.Identity.Name, testVariant.ProjectID);
            return View(testCaseViewModel);
        }

        // GET: TestCase/DeleteAttachment/5
        public async Task<IActionResult> DeleteAttachment(string id, string TesterName, DateTime? FromPlannedDate, DateTime? ToPlannedDate, string SearchTestStatus)
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
                TestCaseAttachment = testCaseAttachment
            };

            return View(testCaseAttachmentViewModel);
        }

        // POST: TestCase/DeleteAttachmentConfirmed/5
        [HttpPost, ActionName("DeleteAttachment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttachmentConfirmed(string id, string TesterName, DateTime? FromPlannedDate, DateTime? ToPlannedDate, string SearchTestStatus)
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
                                     select new { ProjectID = scenario.ProjectID, ScenarioID = scenario.ID }).FirstOrDefaultAsync();

            if (!CanWrite(User.Identity.Name, testVariant?.ProjectID))
            {
                return Forbid();
            }

            _context.TestCaseAttachment.Remove(testCaseAttachment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Edit), new { id = testCaseAttachment.TestCaseID, TesterName = TesterName, FromPlannedDate = FromPlannedDate, ToPlannedDate = ToPlannedDate, SearchTestStatus = SearchTestStatus });
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
            var list = GetTestStatusList();
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

        public List<string> GetTestStatusList()
        {
            var testStatusList = new List<string>() {
                TestStatuses.Passed,
                TestStatuses.Failed,
                TestStatuses.Open,
                TestStatuses.Pending,
                TestStatuses.Hold,
                TestStatuses.Canceled
            };
            return testStatusList;
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
        public async Task<IActionResult> Import([Bind("ProjectID, ScenarioID, DataFile")] ImportTestCaseViewModel importModel)
        {
            if (ModelState.IsValid)
            {
                if (!CanWrite(User.Identity.Name, importModel.ProjectID))
                {
                    return Forbid();
                }

                var scenario = await (from s in _context.TestScenario.Where(i => i.ID.Equals(importModel.ScenarioID, StringComparison.OrdinalIgnoreCase) && i.IsActive)
                                      join p in _context.Project.Where(i => i.ID.Equals(importModel.ProjectID, StringComparison.OrdinalIgnoreCase) && i.IsActive)
                                      on s.ProjectID equals p.ID
                                      select s).FirstOrDefaultAsync();

                if (scenario == null)
                {
                    return NotFound();
                }

                using (var stream = importModel.DataFile.OpenReadStream())
                {
                    using (var excelPackage = new ExcelPackage(stream))
                    {
                        int errorCount = 0;
                        var newTestCaseList = new List<TestCase>();
                        var newVariantList = new List<TestVariant>();
                        var existingVariantList = await _context.TestVariant.Where(i => i.ScenarioID.Equals(importModel.ScenarioID, StringComparison.OrdinalIgnoreCase)).ToListAsync();
                        var existingTestCaseList = await (from testCase in _context.TestCase
                                                          join variant in existingVariantList
                                                          on testCase.TestVariantID equals variant.ID
                                                          select testCase).ToListAsync();
                        var updatedTestCaseList = new List<TestCase>();
                        var workSheet = excelPackage.Workbook.Worksheets["TestCase"];

                        if (workSheet == null)
                        {
                            ModelState.AddModelError("", "Sheet 'TestCase' is not found.");
                            return View(importModel);
                        }

                        int rows = workSheet.Dimension.Rows;
                        var statusList = GetTestStatusList();
                        var statusString = string.Empty;
                        foreach (var s in statusList)
                        {
                            statusString += s + ", ";
                        }
                        if (statusString.Length > 0)
                        {
                            statusString = statusString.Remove(statusString.Length - 2);
                        }
                        for (var i = 2; i <= rows; i++)
                        {
                            string errorMessage = string.Empty;

                            string variantName = workSheet.Cells[i, 1].GetValue<string>();
                            if (string.IsNullOrEmpty(variantName))
                            {
                                errorCount++;
                                errorMessage += "TestVariantName is required. ";
                            }

                            string testCaseName = workSheet.Cells[i, 2].GetValue<string>();
                            if (string.IsNullOrEmpty(testCaseName))
                            {
                                errorCount++;
                                errorMessage += "TestCaseName is required. ";
                            }

                            string testCaseSteps = workSheet.Cells[i, 3].GetValue<string>();
                            if (string.IsNullOrEmpty(testCaseSteps))
                            {
                                errorCount++;
                                errorMessage += "TestCaseSteps is required. ";
                            }

                            string expectedResult = workSheet.Cells[i, 4].GetValue<string>();
                            if (string.IsNullOrEmpty(expectedResult))
                            {
                                errorCount++;
                                errorMessage += "ExpectedResult is required. ";
                            }

                            string actualResult = workSheet.Cells[i, 5].GetValue<string>();

                            string testerName = workSheet.Cells[i, 6].GetValue<string>();
                            if (string.IsNullOrEmpty(testerName))
                            {
                                errorCount++;
                                errorMessage += "TesterName is required. ";
                            }

                            DateTime? plannedDate = new DateTime(2000, 1, 1);
                            var minDate = new DateTime(2000, 1, 1);
                            try
                            {
                                plannedDate = workSheet.Cells[i, 7].GetValue<DateTime>().Date;
                                if (!plannedDate.HasValue || plannedDate.Value < minDate)
                                {
                                    errorCount++;
                                    errorMessage += $"PlannedDate must be >= {minDate.ToString("yyyy/MM/dd")}. ";
                                }
                            }
                            catch
                            {
                                errorCount++;
                                errorMessage += $"PlannedDate must be a Date value >= {minDate.ToString("yyyy/MM/dd")}. ";
                            }

                            string testStatus = workSheet.Cells[i, 8].GetValue<string>();
                            if (!statusList.Contains(testStatus))
                            {
                                errorCount++;
                                errorMessage += $"TestStatus must be {statusString}. ";
                            }

                            string testCaseID = workSheet.Cells[i, 9].GetValue<string>();
                            TestCase updatedTestCase = null;
                            if (!string.IsNullOrEmpty(testCaseID))
                            {
                                updatedTestCase = existingTestCaseList.FirstOrDefault(c => c.ID.Equals(testCaseID, StringComparison.OrdinalIgnoreCase));
                                if (updatedTestCase == null)
                                {
                                    errorCount++;
                                    errorMessage += "TestCaseID is not found. ";
                                }
                            }

                            if (!string.IsNullOrEmpty(errorMessage))
                            {
                                workSheet.Cells[i, 10].Value = errorMessage;
                            }
                            else
                            {
                                var variant = existingVariantList.FirstOrDefault(v => v.VariantName.Equals(variantName, StringComparison.OrdinalIgnoreCase));
                                if (variant == null)
                                {
                                    variant = newVariantList.FirstOrDefault(v => v.VariantName.Equals(variantName, StringComparison.OrdinalIgnoreCase));
                                    if (variant == null)
                                    {
                                        variant = new TestVariant()
                                        {
                                            ScenarioID = importModel.ScenarioID,
                                            VariantName = variantName
                                        };
                                        variant.SetCreator(User.Identity.Name);
                                        newVariantList.Add(variant);
                                    }
                                }

                                if (updatedTestCase != null)
                                {
                                    updatedTestCase.TestVariantID = variant.ID;
                                    updatedTestCase.TestCaseName = testCaseName;
                                    updatedTestCase.TestCaseSteps = testCaseSteps;
                                    updatedTestCase.ExpectedResult = expectedResult;
                                    updatedTestCase.ActualResult = actualResult;
                                    updatedTestCase.TesterName = testerName;
                                    updatedTestCase.PlannedDate = plannedDate.Value;
                                    updatedTestCase.TestStatus = testStatus;
                                    updatedTestCase.SetUpdater(User.Identity.Name);
                                    updatedTestCaseList.Add(updatedTestCase);
                                }
                                else
                                {
                                    var newTestCase = new TestCase()
                                    {
                                        TestVariantID = variant.ID,
                                        TestCaseName = testCaseName,
                                        TestCaseSteps = testCaseSteps,
                                        ExpectedResult = expectedResult,
                                        ActualResult = actualResult,
                                        TesterName = testerName,
                                        PlannedDate = plannedDate.Value,
                                        TestStatus = testStatus
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
                return RedirectToAction(nameof(Index), new { ProjectID = importModel.ProjectID, ScenarioID = importModel.ScenarioID });
            }
            return View(importModel);
        }
    }
}
