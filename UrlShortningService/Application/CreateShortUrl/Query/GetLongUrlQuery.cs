namespace UrlShortningService.Application.GetLongUrl.Query
{
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using UrlShortningService.Data;
    using UrlShortningService.Domain.Models;
    using UrlShortningService.Dto;
    using UrlShortningService.Services.Interfaces;

    public class GetLongUrlQuery : IRequest<Result<string>>
    {
        public string ShortUrl { get; set; }
    }

    public class GetLongUrlQueryHandler : IRequestHandler<GetLongUrlQuery, Result<string>>
    {
        private readonly UrlShortenerDbContext _context;
        private readonly ILogger<GetLongUrlQueryHandler> _logger;
        private readonly ICacheService _cacheService;

        public GetLongUrlQueryHandler(UrlShortenerDbContext context, ILogger<GetLongUrlQueryHandler> logger, ICacheService cacheService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        public async Task<Result<string>> Handle(GetLongUrlQuery request, CancellationToken cancellationToken)
        {
            var requestTime = DateTime.UtcNow;

            try
            {
                _logger.LogInformation("Handling GetLongUrlQuery. ShortUrl: {ShortUrl}", request.ShortUrl);

                // Validate input
                if (string.IsNullOrWhiteSpace(request.ShortUrl))
                {
                    _logger.LogWarning("Invalid ShortUrl provided: {ShortUrl}", request.ShortUrl);
                    return Result<string>.Failure(requestTime, "Invalid ShortUrl", StatusCodes.Status400BadRequest);
                }
                var urlMapping = new UrlMap();

                // Check cache first
                var cachedLongUrl = await _cacheService.GetAsync(request.ShortUrl);
                if (cachedLongUrl != null)
                {
                    urlMapping = await _context.UrlMappings
                    .FirstOrDefaultAsync(u => u.ShortUrl == request.ShortUrl, cancellationToken);

                    if (urlMapping == null)
                    {
                        _logger.LogWarning("Short URL not found: {ShortUrl}", request.ShortUrl);
                        return Result<string>.Failure(requestTime, "Short URL not found", StatusCodes.Status404NotFound);
                    }

                    urlMapping.AccessCount++;

                    // Save the changes to the database
                    await _context.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Returning cached LongUrl for ShortUrl: {ShortUrl}", request.ShortUrl);
                    return Result<string>.Success(requestTime, cachedLongUrl, StatusCodes.Status200OK, "Long URL retrieved from cache");
                }

                // Retrieve the URL mapping from database if not found in cache
                urlMapping = await _context.UrlMappings
                    .FirstOrDefaultAsync(u => u.ShortUrl == request.ShortUrl, cancellationToken);

                if (urlMapping == null)
                {
                    _logger.LogWarning("Short URL not found: {ShortUrl}", request.ShortUrl);
                    return Result<string>.Failure(requestTime, "Short URL not found", StatusCodes.Status404NotFound);
                }

                // Increment the access count
                urlMapping.AccessCount++;

                // Save the changes to the database
                await _context.SaveChangesAsync(cancellationToken);

                // Cache the LongUrl for future lookups
                await _cacheService.SetAsync(request.ShortUrl, urlMapping.LongUrl, TimeSpan.FromDays(30));  // Cache for 30 days

                _logger.LogInformation("Successfully retrieved LongUrl for ShortUrl: {ShortUrl}", request.ShortUrl);

                return Result<string>.Success(requestTime, urlMapping.LongUrl, StatusCodes.Status200OK, "Long URL retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving LongUrl for ShortUrl: {ShortUrl}", request.ShortUrl);
                return Result<string>.Failure(requestTime, "An error occurred while processing the request", StatusCodes.Status500InternalServerError);
            }
        }
    }
}
