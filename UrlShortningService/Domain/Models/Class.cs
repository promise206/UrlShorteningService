namespace UrlShortningService.Domain.Models
{
    public sealed record UrlMap
    {
        public Guid Id { get; set; } // Primary Key

        public string LongUrl { get; set; }

        public string ShortUrl { get; set; }

        public DateTime CreatedAt { get; set; }

        public int AccessCount { get; set; } // Tracks number of accesses

        public DateTime? ExpiryDate { get; set; }
    }
}
