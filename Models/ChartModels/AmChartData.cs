namespace BugTracker.Models.ChartModels
{
    public class AmChartData
    {
        public AmItem[] Data { get; set; }
    }

    public class AmItem
    {
        public string Project { get; set; }
        public int Tickets { get; set; }
        public int Developers { get; set; }
    }
}
