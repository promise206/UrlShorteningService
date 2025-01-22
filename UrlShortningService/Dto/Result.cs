namespace UrlShortningService.Dto
{
    public class Result<T>
    {
        internal Result(DateTime requestTime, T? data, string? description = null)
        {
            this.Data = data;
            this.RequestTime = requestTime;
            this.ResponseDescription = description;
            this.ResponseTime = DateTime.UtcNow;
            this.ActivityTime = this.ResponseTime - this.RequestTime;
            this.IsSuccess = true;
        }

        internal Result(DateTime requestTime, T? data, int responseCode, string? description = null, string? requestId = null)
        {
            this.Data = data;
            this.RequestTime = requestTime;
            this.ResponseDescription = description;
            this.ResponseCode = responseCode;
            this.RequestId = requestId;
            this.ResponseTime = DateTime.UtcNow;
            this.ActivityTime = this.ResponseTime - this.RequestTime;
            this.IsSuccess = true;
        }


        internal Result(string errorMessage) => this.ErrorMessage = errorMessage;


        internal Result(DateTime requestTime, string errorMessage, IDictionary<string, string[]> Errors)
        {
            this.ErrorMessage = errorMessage;
            this.Errors = Errors;
            this.RequestTime = requestTime;
            this.ResponseTime = DateTime.UtcNow;
            this.ActivityTime = this.ResponseTime - this.RequestTime;
            this.IsSuccess = false;
        }

        public Result()
        {

        }
        internal Result(DateTime requestTime, string errorMessage)
        {
            this.RequestTime = requestTime;
            this.ErrorMessage = errorMessage;
            this.ResponseTime = DateTime.UtcNow;
            this.ActivityTime = this.ResponseTime - this.RequestTime;
            this.IsSuccess = false;
        }

        internal Result(DateTime requestTime, string errorMessage, int responseCode)
        {
            this.RequestTime = requestTime;
            this.ErrorMessage = errorMessage;
            this.ResponseCode = responseCode;
            this.ResponseTime = DateTime.UtcNow;
            this.ActivityTime = this.ResponseTime - this.RequestTime;
            this.IsSuccess = false;
        }

        public T? Data { get; set; }
        public string? RequestId { get; set; } = "";
        public string? ErrorMessage { get; set; } = "";
        public bool HasError => this.ErrorMessage != "";
        public int? ResponseCode { get; set; }
        public string? ResponseDescription { get; set; }
        public DateTime? RequestTime { get; set; } = DateTime.UtcNow;
        public DateTime? ResponseTime { get; set; } //= DateTime.UtcNow;
        public TimeSpan? ActivityTime { get; set; } //;

        public bool? IsSuccess { get; set; } = true;
        public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();

        public static Result<T> Success(DateTime requestTime, T value, int responseCode = 000, string? ResponseDescription = null, string? requestId = null) => new(requestTime, value, responseCode, ResponseDescription, requestId);

        public static Result<T> Success(DateTime requestTime, long id, T data, string v) => new(requestTime, data);

        public static Result<T> Failure(DateTime requestTime, string errorMessage, int responseCode) =>
         new(requestTime, errorMessage, responseCode);

        public static Result<T> Failure(DateTime requestTime, string errorMessage) =>
         new(requestTime, errorMessage);

        public static Result<T> Failure(string errorMessage) =>
         new(errorMessage);
        public static Result<T> Failure(DateTime requestTime, string errorMessage, IDictionary<string, string[]> Errors) =>
         new(requestTime, errorMessage, Errors);
    }

}
