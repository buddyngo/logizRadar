using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Logiz.Radar.Models
{
    public class TestReportProjectViewModel
    {
        public string ProjectID { get; set; }
        public string ProjectName { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public int Open { get; set; }
        public int Hold { get; set; }
        public int Pending { get; set; }
        public int Canceled { get; set; }
        public int Total
        {
            get
            {
                return Passed + Failed + Open + Pending + Hold + Canceled;
            }
        }
        public decimal PassedPercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Round(Decimal.Divide(Passed * 100, Total), 2);
            }
        }
        public decimal FailedPercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Round(Decimal.Divide(Failed * 100, Total), 2);
            }
        }
        public decimal OpenPercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Round(Decimal.Divide(Open * 100, Total), 2);
            }
        }
        public decimal PendingPercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Round(Decimal.Divide(Pending * 100, Total), 2);
            }
        }
        public decimal HoldPercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Round(Decimal.Divide(Hold * 100, Total), 2);
            }
        }

        public decimal CanceledPercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Round(Decimal.Divide(Canceled * 100, Total), 2);
            }
        }

        public decimal DonePercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Round(Decimal.Divide((Passed + Failed) * 100, Passed + Failed + Open + Pending + Hold), 0);
            }
        }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<TestReportByScenario> ReportByScenario { get; set; }
        public List<TestReportByPlannedDate> ReportByPlannedDate { get; set; }
        public List<TestReportByPlannedDateAccumulation> ReportByPlannedDateAccumulation { get; set; }
    }

    public class TestReportByScenario
    {
        public string ScenarioID { get; set; }
        public string ScenarioName { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public int Open { get; set; }
        public int Hold { get; set; }
        public int Pending { get; set; }
        public int Canceled { get; set; }
        public int Total
        {
            get
            {
                return Passed + Failed + Open + Pending + Hold + Canceled;
            }
        }
        public decimal DonePercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Round(Decimal.Divide((Passed + Failed) * 100, Passed + Failed + Open + Pending + Hold), 0);
            }
        }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class TestReportByPlannedDate
    {
        public DateTime PlannedDate { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public int Open { get; set; }
        public int Hold { get; set; }
        public int Pending { get; set; }
        public int Canceled { get; set; }
        public int Total
        {
            get
            {
                return Passed + Failed + Open + Pending + Hold + Canceled;
            }
        }
        public Decimal WorkloadPercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Round(Decimal.Divide((Open + Pending + Hold) * 100, Passed + Failed + Open + Pending + Hold), 0);
            }
        }
    }

    public class TestReportByPlannedDateAccumulation
    {
        public DateTime PlannedDate { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public int Open { get; set; }
        public int Hold { get; set; }
        public int Pending { get; set; }
        public int Canceled { get; set; }
        public int Total
        {
            get
            {
                return Passed + Failed + Open + Pending + Hold + Canceled;
            }
        }
        public Decimal WorkloadPercentage { get; set; }
    }
}
