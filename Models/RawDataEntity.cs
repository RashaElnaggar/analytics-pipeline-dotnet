namespace AnalyticsPipeline.Models
{
    public class RawDataEntity
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Page { get; set; } = "";
        public int Users { get; set; }
        public int Sessions { get; set; }
        public int Views { get; set; }
        public double? PerformanceScore { get; set; }
        public int? LCPms { get; set; }
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    }
}
