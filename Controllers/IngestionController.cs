using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class IngestionController : ControllerBase
{
    private readonly DataLoader _loader;
    private readonly RabbitMqPublisher _publisher;

    public IngestionController(DataLoader loader, RabbitMqPublisher publisher)
    {
        _loader = loader;
        _publisher = publisher;
    }

    [HttpPost("publish")]
    public IActionResult Publish()
    {
        var data = _loader.ReadCombinedData();
        foreach (var item in data)
            _publisher.Publish(item);

        return Ok(new { count = data.Count() });
    }
}
