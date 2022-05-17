using System.Net;

namespace GitHubIssuesAPI.Test
{
    public class Comment
    {
        public long id { get; set; }
        public string body { get; set; }
        public HttpStatusCode statusCode { get; set; }
    }
}