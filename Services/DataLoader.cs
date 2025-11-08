using System.Text.Json;

public class DataLoader
{
    public IEnumerable<CombinedRecord> ReadCombinedData()
    {
        var gaData = JsonSerializer.Deserialize<List<GARecord>>(File.ReadAllText("MockData/ga.json"))!;
        var psiData = JsonSerializer.Deserialize<List<PSIRecord>>(File.ReadAllText("MockData/psi.json"))!;

        var combined = from ga in gaData
                       join psi in psiData
                       on new { ga.page, ga.date } equals new { psi.page, psi.date }
                       select new CombinedRecord
                       {
                           Page = ga.page,
                           Date = DateTime.Parse(ga.date),
                           Users = ga.users,
                           Sessions = ga.sessions,
                           Views = ga.views,
                           PerformanceScore = psi.performanceScore,
                           LCP_ms = psi.LCP_ms
                       };
        return combined.ToList();
    }
}

public record GARecord(string date, string page, int users, int sessions, int views);
public record PSIRecord(string date, string page, double performanceScore, int LCP_ms);
public record CombinedRecord
{
    public string Page { get; set; } = "";
    public DateTime Date { get; set; }
    public int Users { get; set; }
    public int Sessions { get; set; }
    public int Views { get; set; }
    public double PerformanceScore { get; set; }
    public int LCP_ms { get; set; }
}
