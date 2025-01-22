namespace UrlShortningService.Application.CreateShortUrl.Dtos;
public record UrlStats
{
    public string ShortUrl { get; set; }
    public int AccessCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
