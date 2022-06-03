using System;
using System.Collections.Generic;
using System.Drawing;

namespace PhishingPortal.Dto.Dashboard
{

    public enum ChartType
    {
        Pie,
        Bar,
        Line
    }

    /// <summary>
    /// Category wise pie chart
    /// </summary>
    public class CategoryWisePhishingTestData
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalCampaigns { get; set; }
        public List<PhisingPronePercentEntry> Entries { get; set; }
        public Dictionary<string, decimal> CategoryClickRatioDictionary { get; set; }

    }

    /// <summary>
    /// Campaign prone percent entries
    /// </summary>
    public class PhisingPronePercentEntry
    {
        public Campaign Campaign { get; set; }
        public int Count { get; set; }
        public int Hits { get; set; }
        public decimal PronePercentage
        {
            get
            {
                if (Count == 0)
                    return 0;

                return (Hits / Count) * 100;
            }
        }
    }

    /// <summary>
    /// Represents entries for monthly campaigns and hits
    /// </summary>
    public class MonthlyPhishingBarChart
    {
        public string Title { get; set; }
        public int Year { get; set; }

        public List<MonthlyPhishingBarChartEntry> Entries { get; set; }
    }

    /// <summary>
    /// Represent data entry for monthly cammpaign stats
    /// </summary>
    public class MonthlyPhishingBarChartEntry
    {
        public Months Month { get; set; }
        public int TotalCampaigns { get; set; }
        public int TotalHits { get; set; }
        public decimal HitPronePercent { get; set; }
    }

    /// <summary>
    /// Month enums
    /// </summary>
    public enum Months
    {
        Jan = 1,
        Feb,
        Mar,
        Apr,
        May,
        June,
        Jul,
        August,
        Sep,
        Oct,
        Nove,
        December,
    }

    public class ChartJsDataset
    {
        public string Label { get; set; } = "Default Label";
        public string[] Data { get; set; }
        public string[] BackgroundColor { get; set; }
       
    }

}
