﻿using System;
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
        public int Fixed { get; set; }
        public int Open { get; set; }
        public int Hold { get; set; }
        public int Pending { get; set; }
        public int Canceled { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<TestReportByScenario> ReportByScenario { get; set; }
        public List<TestReportByPlannedDate> ReportByPlannedDate { get; set; }
        public List<TestReportByPlannedDateAccumulation> ReportByPlannedDateAccumulation { get; set; }
        public List<TestReportByResourceSummary> TestReportByResourceSummary { get; set; }
        public List<ResourceWorkloadAccumulation> ResourceWorkloadAccumulation { get; set; }
        public int Total
        {
            get
            {
                return Passed + Failed + Fixed + Open + Pending + Hold + Canceled;
            }
        }
        public decimal PassedPercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Round(Decimal.Divide(Passed * 100, Total), 0);
            }
        }
        public decimal FailedPercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Round(Decimal.Divide(Failed * 100, Total), 0);
            }
        }
        public decimal FixedPercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Round(Decimal.Divide(Fixed * 100, Total), 0);
            }
        }
        public decimal OpenPercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Round(Decimal.Divide(Open * 100, Total), 0);
            }
        }
        public decimal PendingPercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Round(Decimal.Divide(Pending * 100, Total), 0);
            }
        }
        public decimal HoldPercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Round(Decimal.Divide(Hold * 100, Total), 0);
            }
        }

        public decimal CanceledPercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Round(Decimal.Divide(Canceled * 100, Total), 0);
            }
        }

        public decimal DonePercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Round(Decimal.Divide((Passed + Canceled) * 100, Total), 0);
            }
        }

        public decimal TestedPercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Round(Decimal.Divide((Passed + Canceled + Failed) * 100, Total), 0);
            }
        }
    }

    public class TestReportByScenario
    {
        public string ScenarioID { get; set; }
        public string ScenarioName { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public int Fixed { get; set; }
        public int Open { get; set; }
        public int Hold { get; set; }
        public int Pending { get; set; }
        public int Canceled { get; set; }
        public int Total
        {
            get
            {
                return Passed + Failed + Fixed + Open + Pending + Hold + Canceled;
            }
        }
        public decimal DonePercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Round(Decimal.Divide((Passed + Canceled) * 100, Total), 0);
            }
        }
        public decimal TestedPercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Round(Decimal.Divide((Passed + Canceled + Failed) * 100, Total), 0);
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
        public int Fixed { get; set; }
        public int Open { get; set; }
        public int Hold { get; set; }
        public int Pending { get; set; }
        public int Canceled { get; set; }
        public int Total
        {
            get
            {
                return Passed + Failed + Fixed + Open + Pending + Hold + Canceled;
            }
        }
        public Decimal CurrentWorkloadPercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Round(Decimal.Divide((Fixed + Open) * 100, Total), 0);
            }
        }
        public Decimal UpToEndWorkloadPercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Round(Decimal.Divide((Failed + Fixed + Pending + Open + Hold) * 100, Total), 0);
            }
        }
    }

    public class TestReportByPlannedDateAccumulation
    {
        public DateTime PlannedDate { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public int Fixed { get; set; }
        public int Open { get; set; }
        public int Hold { get; set; }
        public int Pending { get; set; }
        public int Canceled { get; set; }
        public int Total
        {
            get
            {
                return Passed + Failed + Fixed + Open + Pending + Hold + Canceled;
            }
        }
        public Decimal CurrentWorkloadPercentage { get; set; }
        public Decimal UpToEndWorkloadPercentage { get; set; }
    }

    public class TestReportByResourceSummary
    {
        public string TesterName { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public int Fixed { get; set; }
        public int Open { get; set; }
        public int Hold { get; set; }
        public int Pending { get; set; }
        public int Canceled { get; set; }
        public int TotalPlannedDays { get; set; }
        public int RemainingPendingDays { get; set; }
        public int RemainingWorkingDays { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Total
        {
            get
            {
                return Passed + Failed + Fixed + Open + Pending + Hold + Canceled;
            }
        }
        public decimal DonePercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Round(Decimal.Divide((Passed + Canceled) * 100, Total), 0);
            }
        }
        public decimal TestedPercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Round(Decimal.Divide((Passed + Canceled + Failed) * 100, Total), 0);
            }
        }
    }
    public class TestReportByResourceDate
    {
        public string TesterName { get; set; }
        public DateTime PlannedDate { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public int Fixed { get; set; }
        public int Open { get; set; }
        public int Hold { get; set; }
        public int Pending { get; set; }
        public int Canceled { get; set; }
        public int Total
        {
            get
            {
                return Passed + Failed + Fixed + Open + Pending + Hold + Canceled;
            }
        }

        public Decimal CurrentWorkloadPercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Divide((Fixed + Open) * 100, Total);
            }
        }
        public Decimal UpToEndWorkloadPercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Divide((Failed + Fixed + Pending + Open + Hold) * 100, Total);
            }
        }
    }

    public class ResourceWorkloadAccumulation
    {
        public string TesterName { get; set; }
        public int TotalPlannedDays { get; set; }
        public int RemainingPendingDays { get; set; }
        public int RemainingWorkingDays { get; set; }
        public Decimal CurrentWorkloadPercentage { get; set; }
        public Decimal UpToEndWorkloadPercentage { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public int Fixed { get; set; }
        public int Open { get; set; }
        public int Hold { get; set; }
        public int Pending { get; set; }
        public int Canceled { get; set; }
        public int Total
        {
            get
            {
                return Passed + Failed + Fixed + Open + Pending + Hold + Canceled;
            }
        }
        public decimal DonePercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Round(Decimal.Divide((Passed + Canceled) * 100, Total), 0);
            }
        }
        public decimal TestedPercentage
        {
            get
            {
                return Total == 0 ? 0 : Decimal.Round(Decimal.Divide((Passed + Canceled + Failed) * 100, Total), 0);
            }
        }
    }
}
