namespace UrlShortningService.Application.GetStats.Query;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UrlShortningService.Application.CreateShortUrl.Dtos;
using UrlShortningService.Data;
using UrlShortningService.Dto;

public class GetStatsQuery : IRequest<Result<UrlStats>>
{
    public string ShortUrl { get; set; }
}

public class GetStatsQueryHandler : IRequestHandler<GetStatsQuery, Result<UrlStats>>
{
    private readonly UrlShortenerDbContext _context;
    private readonly ILogger<GetStatsQueryHandler> _logger;

    public GetStatsQueryHandler(UrlShortenerDbContext context, ILogger<GetStatsQueryHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<UrlStats>> Handle(GetStatsQuery request, CancellationToken cancellationToken)
    {
        var requestTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("Handling GetStatsQuery. ShortUrl: {ShortUrl}", request.ShortUrl);

            // Validate input
            if (string.IsNullOrWhiteSpace(request.ShortUrl))
            {
                _logger.LogWarning("Invalid ShortUrl provided: {ShortUrl}", request.ShortUrl);
                return Result<UrlStats>.Failure(requestTime, "Invalid ShortUrl", StatusCodes.Status400BadRequest);
            }

            // Retrieve the URL mapping
            var urlMapping = await _context.UrlMappings
                .FirstOrDefaultAsync(u => u.ShortUrl == request.ShortUrl, cancellationToken);

            if (urlMapping == null)
            {
                _logger.LogWarning("Short URL not found: {ShortUrl}", request.ShortUrl);
                return Result<UrlStats>.Failure(requestTime, "Short URL not found", StatusCodes.Status404NotFound);
            }

            _logger.LogInformation("Successfully retrieved stats for ShortUrl: {ShortUrl}", request.ShortUrl);

            var stats = new UrlStats
            {
                ShortUrl = urlMapping.ShortUrl,
                AccessCount = urlMapping.AccessCount,
                CreatedAt = urlMapping.CreatedAt
            };

            return Result<UrlStats>.Success(requestTime, stats, StatusCodes.Status200OK, "URL stats retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving stats for ShortUrl: {ShortUrl}", request.ShortUrl);
            return Result<UrlStats>.Failure(requestTime, "An error occurred while processing the request", StatusCodes.Status500InternalServerError);
        }
    }
}
