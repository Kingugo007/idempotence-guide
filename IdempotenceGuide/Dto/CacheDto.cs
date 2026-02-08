namespace IdempotenceGuide.Dto
{
    public class CachedResponse
    {
        public int StatusCode { get; set; }
        public string ContentType { get; set; }
        public string Body { get; set; }
    }
}
