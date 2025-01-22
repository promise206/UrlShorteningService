namespace UrlShortningService.Application.CreateShortUrl.Command;

using MediatR;
using Microsoft.Extensions.Logging;
using UrlShortningService.Data;
using UrlShortningService.Domain.Models;
using UrlShortningService.Dto;
using UrlShortningService.Services.Interfaces;

public class CreateShortUrlCommand : IRequest<Result<string>>
{
    public string LongUrl { get; set; }
}

public class CreateShortUrlCommandHandler : IRequestHandler<CreateShortUrlCommand, Result<string>>
{
    private readonly UrlShortenerDbContext _context;
    private readonly ILogger<CreateShortUrlCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public CreateShortUrlCommandHandler(UrlShortenerDbContext context, ILogger<CreateShortUrlCommandHandler> logger, ICacheService cacheService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    public async Task<Result<string>> Handle(CreateShortUrlCommand request, CancellationToken cancellationToken)
    {
        var requestTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("Handling CreateShortUrlCommand. LongUrl: {LongUrl}", request.LongUrl);

            // Validate input
            if (string.IsNullOrWhiteSpace(request.LongUrl))
            {
                _logger.LogWarning("Invalid LongUrl provided: {LongUrl}", request.LongUrl);
                return Result<string>.Failure(requestTime, "Invalid LongUrl", StatusCodes.Status400BadRequest);
            }

            // Check cache to see if the URL has already been shortened
            var cachedShortUrl = await _cacheService.GetAsync(request.LongUrl);
            if (cachedShortUrl != null)
            {
                _logger.LogInformation("Returning cached short URL: {ShortUrl}", cachedShortUrl);
                return Result<string>.Success(requestTime, cachedShortUrl, StatusCodes.Status200OK, "Short URL retrieved from cache");
            }

            var shortUrl = GenerateShortUrl();

            var urlMapping = new UrlMap
            {
                Id = Guid.NewGuid(),
                LongUrl = request.LongUrl,
                ShortUrl = shortUrl,
                CreatedAt = DateTime.UtcNow
            };

            // Save to database
            _context.UrlMappings.Add(urlMapping);
            await _context.SaveChangesAsync(cancellationToken);

            // Cache the short URL
            await _cacheService.SetAsync(request.LongUrl, shortUrl, TimeSpan.FromDays(30));  // Cache for 30 days

            _logger.LogInformation("Short URL created successfully. LongUrl: {LongUrl}, ShortUrl: {ShortUrl}", request.LongUrl, shortUrl);

            return Result<string>.Success(requestTime, shortUrl, StatusCodes.Status201Created, "Short URL created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating a short URL for LongUrl: {LongUrl}", request.LongUrl);
            return Result<string>.Failure(requestTime, "An error occurred while processing the request", StatusCodes.Status500InternalServerError);
        }
    }

    private string GenerateShortUrl()
    {
        _logger.LogDebug("Generating a short URL");
        var shortUrl = Guid.NewGuid().ToString("N").Substring(0, 8);
        _logger.LogDebug("Generated ShortUrl: {ShortUrl}", shortUrl);
        return shortUrl;
    }
}
