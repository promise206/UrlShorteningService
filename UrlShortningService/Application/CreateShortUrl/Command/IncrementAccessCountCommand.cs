namespace UrlShortningService.Application.IncrementAccessCount.Command;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UrlShortningService.Data;
using UrlShortningService.Dto;

public class IncrementAccessCountCommand : IRequest<Result<Unit>>
{
    public string ShortUrl { get; set; }
}

public class IncrementAccessCountCommandHandler : IRequestHandler<IncrementAccessCountCommand, Result<Unit>>
{
    private readonly UrlShortenerDbContext _context;
    private readonly ILogger<IncrementAccessCountCommandHandler> _logger;

    public IncrementAccessCountCommandHandler(UrlShortenerDbContext context, ILogger<IncrementAccessCountCommandHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Unit>> Handle(IncrementAccessCountCommand request, CancellationToken cancellationToken)
    {
        var requestTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("Handling IncrementAccessCountCommand. ShortUrl: {ShortUrl}", request.ShortUrl);

            // Validate input
            if (string.IsNullOrWhiteSpace(request.ShortUrl))
            {
                _logger.LogWarning("Invalid ShortUrl provided: {ShortUrl}", request.ShortUrl);
                return Result<Unit>.Failure(requestTime, "Invalid ShortUrl", StatusCodes.Status400BadRequest);
            }

            // Retrieve the URL mapping
            var urlMapping = await _context.UrlMappings
                .FirstOrDefaultAsync(u => u.ShortUrl == request.ShortUrl, cancellationToken);

            if (urlMapping == null)
            {
                _logger.LogWarning("Short URL not found: {ShortUrl}", request.ShortUrl);
                return Result<Unit>.Failure(requestTime, "Short URL not found", StatusCodes.Status404NotFound);
            }

            // Increment access count
            urlMapping.AccessCount++;
            _logger.LogInformation("Incremented AccessCount for ShortUrl: {ShortUrl}. New AccessCount: {AccessCount}",
                                    urlMapping.ShortUrl, urlMapping.AccessCount);

            // Save changes to the database
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Access count updated successfully for ShortUrl: {ShortUrl}", request.ShortUrl);

            return Result<Unit>.Success(requestTime, Unit.Value, StatusCodes.Status200OK, "Access count incremented successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while incrementing access count for ShortUrl: {ShortUrl}", request.ShortUrl);
            return Result<Unit>.Failure(requestTime, "An error occurred while processing the request", StatusCodes.Status500InternalServerError);
        }
    }
}
