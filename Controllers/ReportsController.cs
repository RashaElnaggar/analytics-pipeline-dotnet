using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AnalyticsPipeline.Data; // your DbContext namespace
using AnalyticsPipeline.Models; // your models namespace

namespace AnalyticsPipeline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // ✅ JWT required
    public class ReportsController : ControllerBase
    {
        private readonly AnalyticsDbContext _context;

        public ReportsController(AnalyticsDbContext context)
        {
            _context = context;
        }

        // ✅ GET /api/reports/overview
        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview()
        {
            var overview = await _context.DailyStats
                .GroupBy(d => 1)
                .Select(g => new
                {
                    totalUsers = g.Sum(x => x.TotalUsers),
                    totalSessions = g.Sum(x => x.TotalSessions),
                    totalViews = g.Sum(x => x.TotalViews),
                    avgPerformance = g.Average(x => x.AvgPerformance)
                })
                .FirstOrDefaultAsync();

            if (overview == null)
                return NotFound("No report data found.");

            return Ok(overview);
        }

        // ✅ GET /api/reports/pages
        [HttpGet("pages")]
        public async Task<IActionResult> GetByPages()
        {
            // Join DailyStats with RawData to group by Page
            var pagesReport = await _context.RawData
                .GroupBy(r => r.Page)
                .Select(g => new
                {
                    page = g.Key,
                    totalUsers = g.Sum(x => x.Users),
                    totalSessions = g.Sum(x => x.Sessions),
                    totalViews = g.Sum(x => x.Views),
                    avgPerformance = g.Average(x => x.PerformanceScore)
                })
                .ToListAsync();

            if (pagesReport.Count == 0)
                return NotFound("No data found per page.");

            return Ok(pagesReport);
        }
    }
}
