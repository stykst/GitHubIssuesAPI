namespace GitHubIssuesAPI.Test
{
    using System.Net;

    public class Issue
    {
        public int id { get; set; }
        public int number { get; set; }
        public string title { get; set; }
        public string body { get; set; }
        public string state { get; set; }
        public HttpStatusCode statusCode { get; set; }
    }
}