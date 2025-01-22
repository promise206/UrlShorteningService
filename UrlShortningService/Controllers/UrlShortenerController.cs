using MediatR;
using Microsoft.AspNetCore.Mvc;
using UrlShortningService.Application.CreateShortUrl.Command;
using UrlShortningService.Application.GetLongUrl.Query;
using UrlShortningService.Application.GetStats.Query;

namespace UrlShortningService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UrlShortenerController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UrlShortenerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("shorten")]
        public async Task<IActionResult> Shorten([FromBody] CreateShortUrlCommand command)
        {
            var shortUrl = await _mediator.Send(command);
            return Ok(new { ShortUrl = shortUrl });
        }

        [HttpGet("{shortUrl}")]
        public async Task<IActionResult> RedirectToOriginal(string shortUrl)
        {
            var longUrl = await _mediator.Send(new GetLongUrlQuery { ShortUrl = shortUrl });
            return Ok(longUrl);
        }

        [HttpGet("stats/{shortUrl}")]
        public async Task<IActionResult> GetStats(string shortUrl)
        {
            var stats = await _mediator.Send(new GetStatsQuery { ShortUrl = shortUrl });
            return Ok(stats);
        }
    }

}
