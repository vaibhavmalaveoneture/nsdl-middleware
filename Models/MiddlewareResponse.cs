namespace NSDL.Middleware.Models
{
    public class MiddlewareResponse
    {
        public int StatusCode { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
        public Object? Data { get; set; }
    }
}