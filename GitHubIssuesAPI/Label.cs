namespace GitHubIssuesAPI.Test
{
    public class Label
    {
        public long id { get; set; }
        public string node_id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public string color { get; set; }
        public bool @default { get; set; }
        public string description { get; set; }
    }
}