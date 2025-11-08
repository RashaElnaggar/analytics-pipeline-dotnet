using AnalyticsPipeline.Models;
using Microsoft.EntityFrameworkCore;
namespace AnalyticsPipeline.Data
{
    public class AnalyticsDbContext : DbContext
    {
        public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;

        public DbSet<RawDataEntity> RawData { get; set; } = null!;
        public DbSet<DailyStatsEntity> DailyStats { get; set; } = null!;

    }
}
