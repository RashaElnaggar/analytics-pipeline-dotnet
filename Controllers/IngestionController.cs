using Microsoft.AspNetCore.Mvc;
using AnalyticsPipeline.Services;
using AnalyticsPipeline.Models;
using AnalyticsPipeline.Data;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace AnalyticsPipeline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IngestionController : ControllerBase
    {
        private readonly DataLoader _dataLoader;
        private readonly AnalyticsDbContext _db;
        private readonly IConfiguration _config;

        public IngestionController(DataLoader dataLoader, AnalyticsDbContext db, IConfiguration config)
        {
            _dataLoader = dataLoader;
            _db = db;
            _config = config;
        }

        [HttpPost("publish")]
        public IActionResult Publish()
        {
            try
            {
                var data = _dataLoader.ReadCombinedData();

                // RabbitMQ setup
                var factory = new ConnectionFactory()
                {
                    HostName = _config.GetValue<string>("RabbitMQ:Host") ?? "localhost",
                    UserName = _config.GetValue<string>("RabbitMQ:User") ?? "guest",
                    Password = _config.GetValue<string>("RabbitMQ:Pass") ?? "guest"
                };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                // Exchange for publishing
                channel.ExchangeDeclare(exchange: "analytics.raw", type: ExchangeType.Fanout, durable: false);

                foreach (var item in data)
                {
                    // Publish to RabbitMQ
                    var json = JsonSerializer.Serialize(item);
                    var body = Encoding.UTF8.GetBytes(json);
                    channel.BasicPublish(exchange: "analytics.raw", routingKey: "", basicProperties: null, body: body);

                    // Save to DB
                    _db.RawData.Add(new RawDataEntity
                    {
                        Page = item.Page,
                        Date = item.Date,
                        Users = item.Users,
                        Sessions = item.Sessions,
                        Views = item.Views,
                        PerformanceScore = item.PerformanceScore,
                        LCPms = item.LCP_ms
                        // ReceivedAt auto-filled
                    });
                }

                _db.SaveChanges();

                // Aggregate DailyStats
                var dailyStats = data.GroupBy(d => d.Date)
                    .Select(g => new DailyStatsEntity
                    {
                        Date = g.Key,
                        TotalUsers = g.Sum(x => x.Users),
                        TotalSessions = g.Sum(x => x.Sessions),
                        TotalViews = g.Sum(x => x.Views),
                        AvgPerformance = g.Average(x => x.PerformanceScore),
                        LastUpdatedAt = DateTime.UtcNow
                    });

                _db.DailyStats.AddRange(dailyStats);
                _db.SaveChanges();

                return Ok(new { message = "Published to RabbitMQ and saved to DB", count = data.Count() });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error loading data: {ex.Message}");
            }
        }
    }
}
