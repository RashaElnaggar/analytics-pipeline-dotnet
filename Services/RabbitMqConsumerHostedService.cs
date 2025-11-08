using AnalyticsPipeline.Models;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

public class RabbitMqConsumerHostedService : BackgroundService
{
    private readonly IServiceProvider _provider;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqConsumerHostedService(IServiceProvider provider) => _provider = provider;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare("analytics.raw.q", durable: true, exclusive: false, autoDelete: false);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var record = JsonSerializer.Deserialize<CombinedRecord>(json)!;

            var success = await SaveToDbAsync(record);
            if (success)
                _channel.BasicAck(ea.DeliveryTag, false);
            else
                _channel.BasicNack(ea.DeliveryTag, false, false);
        };

        _channel.BasicConsume("analytics.raw.q", autoAck: false, consumer);
        return Task.CompletedTask;
    }

    private async Task<bool> SaveToDbAsync(CombinedRecord record)
    {
        try
        {
            using var scope = _provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();

            db.RawData.Add(new RawDataEntity
            {
                Date = record.Date,
                Page = record.Page,
                Users = record.Users,
                Sessions = record.Sessions,
                Views = record.Views,
                PerformanceScore = record.PerformanceScore,
                LCPms = record.LCP_ms,
                ReceivedAt = DateTime.UtcNow
            });

            // update or insert DailyStats
            var daily = await db.DailyStats.FirstOrDefaultAsync(d => d.Date == record.Date);
            if (daily == null)
            {
                daily = new DailyStatsEntity
                {
                    Date = record.Date,
                    TotalUsers = record.Users,
                    TotalSessions = record.Sessions,
                    TotalViews = record.Views,
                    AvgPerformance = record.PerformanceScore
                };
                db.DailyStats.Add(daily);
            }
            else
            {
                daily.TotalUsers += record.Users;
                daily.TotalSessions += record.Sessions;
                daily.TotalViews += record.Views;
                daily.AvgPerformance = (daily.AvgPerformance + record.PerformanceScore) / 2;
                daily.LastUpdatedAt = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();
            return true;
        }
        catch
        {
            await Task.Delay(1000);
            return false;
        }
    }
}
